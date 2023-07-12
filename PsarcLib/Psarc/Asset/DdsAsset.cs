using System;
using System.IO;

namespace Rocksmith2014PsarcLib.Psarc.Asset
{
    public class DdsAsset : PsarcAsset
    {
        // TODO this file was removed as it didn't work
        // also was blocking .net 6 migration
        // see https://github.com/kokolihapihvi/Rocksmith2014PsarcLib/blob/main/Psarc/Asset/DdsAsset.cs

        public override void ReadFrom(MemoryStream stream)
        {
            throw new NotImplementedException();
        }
    }
}
