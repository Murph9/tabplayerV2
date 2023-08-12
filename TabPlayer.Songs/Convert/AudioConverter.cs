using System;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;

namespace murph9.TabPlayer.Songs.Convert;

public class AudioConverter
{
    public static string ConvertWemToOgg(string file, bool forceConvert = false)
    {
        var outputFile = Path.Join(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + ".ogg";
		
        if (File.Exists(outputFile) && !forceConvert)
        {
            return outputFile;
        }

        try
        {
		    using var fileS = File.Open(file, FileMode.Open);
            var wemClass = new WEMSharp.WEMFile(fileS, WEMSharp.WEMForcePacketFormat.NoForcePacketFormat);
            using var outS = new MemoryStream();
			wemClass.GenerateOGG(outS, false, false);
            outS.Seek(0, SeekOrigin.Begin);

            using var outFile = File.Open(outputFile, FileMode.Create);
            var revorb = RevorbStd.Revorb.Jiggle(outS);
            revorb.CopyTo(outFile);
		} catch (Exception e) {
            Console.WriteLine(e);
            throw;
		}
        return outputFile;
    }
}
