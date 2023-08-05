using System;
using System.IO;

namespace WEMSharp
{
    internal readonly struct Packet8
    {
        private readonly uint _offset;
        private readonly uint _size;
        private readonly uint _absoluteGranule;

        internal Packet8(Stream stream, uint offset)
        {
            _offset = offset;
            _size = 0xFFFF;
            _absoluteGranule = 0;

            stream.Seek(_offset, SeekOrigin.Begin);

            byte[] sizeBuffer = new byte[4];
            stream.Read(sizeBuffer, 0, 4);
            _size = BitConverter.ToUInt32(sizeBuffer, 0);

            byte[] granuleBuffer = new byte[4];
            stream.Read(granuleBuffer, 0, 4);
            _absoluteGranule = BitConverter.ToUInt32(granuleBuffer, 0);
        }

        internal uint GetHeaderSize() => 8;
        
        internal uint GetOffset()
        {
            return GetHeaderSize() + _offset;
        }

        internal uint GetSize()
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
