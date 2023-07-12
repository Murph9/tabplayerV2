using System;
using System.IO;

namespace WEMSharp
{
    internal struct Packet8
    {
        private uint _offset;
        private uint _size;
        private uint _absoluteGranule;

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

        internal uint GetHeaderSize()
        {
            return 8;
        }

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
