using System;
using System.Collections.Generic;
using System.IO;

namespace Rocksmith2014PsarcLib.Psarc.Asset
{
    public class BkhdAsset : PsarcAsset
    {
        private UInt32 _bkhd_length;
        private UInt32 _bkhd_version;
        private UInt32 _bkhd_id;

        private UInt32 _didx_length;
        private DIDX[] _didx;
        struct DIDX {
            public UInt32 wem_id;
            public UInt32 offset;
            public UInt32 length;
        }

        private Int32 _data_length;
        
        public override void ReadFrom(MemoryStream stream)
        {
            base.ReadFrom(stream);
            
            using (var reader = new BinaryReader(stream))
            {
                var bkhdLabel = reader.ReadChars(4);
                _bkhd_length = reader.ReadUInt32();
                _bkhd_version = reader.ReadUInt32();
                _bkhd_id = reader.ReadUInt32();
                var cur = _bkhd_length - 8;
                while (cur > 0) {
                    reader.ReadInt32();
                    cur -= 4;
                }

                var didxLabel = reader.ReadChars(4);
                _didx_length = reader.ReadUInt32();
                cur = _didx_length;
                var dList = new List<DIDX>();
                while (cur > 0) {
                    var d = new DIDX() {
                        wem_id = reader.ReadUInt32(),
                        offset = reader.ReadUInt32(),
                        length = reader.ReadUInt32(),
                    };
                    dList.Add(d);
                    
                    cur -= 12;
                }
                _didx = dList.ToArray();

                var dataLabel = reader.ReadChars(4);
                _data_length = reader.ReadInt32();
            }
        }

        public UInt32 GetWemId() {
            return _didx[0].wem_id;
        }
    }
}