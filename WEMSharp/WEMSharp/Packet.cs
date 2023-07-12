using System;
using System.IO;

namespace WEMSharp
{
    internal struct Packet
    {
        private uint _offset;
        private ushort _size;
        private uint _absoluteGranule;
        private bool _noGranule;

        internal Packet(Stream stream, uint offset, bool noGranule = false)
        {
            _offset = offset;
            _size = 0xFFFF;
            _absoluteGranule = 0;
            _noGranule = noGranule;

            stream.Seek(_offset, SeekOrigin.Begin);

            byte[] sizeBuffer = new byte[2];
            stream.Read(sizeBuffer, 0, 2);
            _size = BitConverter.ToUInt16(sizeBuffer, 0);

            if (!_noGranule)
            {
                byte[] granuleBuffer = new byte[4];
                stream.Read(granuleBuffer, 0, 4);
                _absoluteGranule = BitConverter.ToUInt32(granuleBuffer, 0);
            }
        }

        internal uint GetHeaderSize()
        {
            return _noGranule ? (uint)2 : 6;
        }

        internal uint GetOffset()
        {
            return GetHeaderSize() + _offset;
        }

        internal ushort GetSize()
        {
            return _size;
        }

        internal uint GetGranule()
        {
            return _absoluteGranule;
        }

        internal uint NextOffset()
        {
            return _offset + GetHeaderSize() + _size;
        }
    }
}
