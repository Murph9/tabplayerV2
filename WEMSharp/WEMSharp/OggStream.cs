using System;
using System.IO;
using System.Text;

namespace WEMSharp
{
    public class OggStream : BinaryWriter
    {
        private const uint HEADER_SIZE = 27;
        private const uint MAX_SEGMENTS = 255;
        private const uint SEGMENT_SIZE = 255;

        private byte _bitBuffer;
        private int _bitsStored;
        private uint _payloadBytes;
        private bool _first = true;
        private bool _continued;
        private readonly byte[] _pageBuffer = new byte[HEADER_SIZE + MAX_SEGMENTS + SEGMENT_SIZE * MAX_SEGMENTS];
        private uint _granule;
        private uint _sequenceNumber;

        internal OggStream(Stream file) : base(file) { }
        ~OggStream()
        {
            FlushPage();
        }

        internal void WriteBit(byte bit)
        {
            if (bit == 1)
            {
                _bitBuffer |= (byte)(1U << _bitsStored);
            }

            _bitsStored++;

            if (_bitsStored == 8)
            {
                FlushBits();
            }
        }

        internal void FlushBits()
        {
            if (_bitsStored != 0)
            {
                if (_payloadBytes == SEGMENT_SIZE * MAX_SEGMENTS)
                {
                    throw new Exception("Ran out of space in an OGG packet");
                }

                _pageBuffer[HEADER_SIZE + MAX_SEGMENTS + _payloadBytes] = _bitBuffer;
                _payloadBytes++;

                _bitBuffer = 0;
                _bitsStored = 0;
            }
        }

        internal void FlushPage(bool nextContinued = false, bool last = false)
        {
            if (_payloadBytes != MAX_SEGMENTS * SEGMENT_SIZE)
            {
                FlushBits();
            }

            if (_payloadBytes != 0)
            {
                uint segments = (_payloadBytes + SEGMENT_SIZE) / SEGMENT_SIZE;
                if (segments == MAX_SEGMENTS + 1)
                {
                    segments = MAX_SEGMENTS;
                }

                for (int i = 0; i < _payloadBytes; i++)
                {
                    _pageBuffer[HEADER_SIZE + segments + i] = _pageBuffer[HEADER_SIZE + MAX_SEGMENTS + i];
                }

                _pageBuffer[0] = (byte)'O';
                _pageBuffer[1] = (byte)'g';
                _pageBuffer[2] = (byte)'g';
                _pageBuffer[3] = (byte)'S';
                _pageBuffer[4] = 0;
                _pageBuffer[5] = (byte)((_continued ? 1 : 0) | (_first ? 2 : 0) | (last ? 4 : 0));
                Buffer.BlockCopy(BitConverter.GetBytes(_granule), 0, _pageBuffer, 6, 4);

                if (_granule == 0xFFFFFFFF)
                {
                    _pageBuffer[10] = 0xFF;
                    _pageBuffer[11] = 0xFF;
                    _pageBuffer[12] = 0xFF;
                    _pageBuffer[13] = 0xFF;
                }
                else
                {
                    _pageBuffer[10] = 0;
                    _pageBuffer[11] = 0;
                    _pageBuffer[12] = 0;
                    _pageBuffer[13] = 0;
                }

                _pageBuffer[14] = 1;
                Buffer.BlockCopy(BitConverter.GetBytes(_sequenceNumber), 0, _pageBuffer, 18, 4);
                _pageBuffer[22] = 0;
                _pageBuffer[23] = 0;
                _pageBuffer[24] = 0;
                _pageBuffer[25] = 0;
                _pageBuffer[26] = (byte)segments;

                for (uint i = 0, bytesLeft = _payloadBytes; i < segments; i++)
                {
                    if (bytesLeft >= SEGMENT_SIZE)
                    {
                        bytesLeft -= SEGMENT_SIZE;
                        _pageBuffer[27 + i] = (byte)SEGMENT_SIZE;
                    }
                    else
                    {
                        _pageBuffer[27 + i] = (byte)bytesLeft;
                    }
                }

                Buffer.BlockCopy(BitConverter.GetBytes(CRC32.Compute(_pageBuffer, HEADER_SIZE + segments + _payloadBytes)), 0, _pageBuffer, 22, 4);

                for (int i = 0; i < HEADER_SIZE + segments + _payloadBytes; i++)
                {
                    Write(_pageBuffer[i]);
                }

                _sequenceNumber++;
                _first = false;
                _continued = nextContinued;
                _payloadBytes = 0;
            }
        }

        internal void BitWrite(byte value, byte bitCount = 8)
        {
            BitWrite(value, (int)bitCount);
        }

        internal void BitWrite(ushort value, byte bitCount = 16)
        {
            BitWrite(value, (int)bitCount);
        }

        internal void BitWrite(uint value, byte bitCount = 32)
        {
            BitWrite(value, (int)bitCount);
        }

        private void BitWrite(uint value, int size)
        {
            for (int i = 0; i < size; i++)
            {
                WriteBit((value & (1 << i)) != 0 ? (byte)1 : (byte)0);
            }
        }

        internal void WriteVorbisHeader(byte type)
        {
            byte[] vorbisString = Encoding.UTF8.GetBytes("vorbis");

            BitWrite(type);
            for (int i = 0; i < vorbisString.Length; i++)
            {
                BitWrite(vorbisString[i]);
            }
        }

        internal void SetGranule(uint granule)
        {
            _granule = granule;
        }
    }
}
