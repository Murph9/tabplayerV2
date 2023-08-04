using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;

namespace murph9.TabPlayer.Songs.Convert;

internal class AudioConverter
{
    public static void ConvertOggToWav(FileInfo oggFile, Stream stream)
    {
        using var vorbisStream = new VorbisWaveReader(oggFile.FullName);
        WaveFileWriter.WriteWavFileToStream(stream, vorbisStream);
    }

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
            using var outS = File.Open(outputFile, FileMode.Create);
			wemClass.GenerateOGG(outS, false, false);
		} catch (Exception) {
			throw new Exception($"We have failed to convert {outputFile}");
		}
        return outputFile;
    }

/* TODO discuss this
    private static async Task<bool> Revorb(string file)
    {
        var p = new Process
        {
            StartInfo =
                    {
                        FileName = "revorb.exe",
                        Arguments = $"\"{file}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
        };
        p.Start();
        await p.WaitForExitAsync();
        if (p.ExitCode != 0)
        {
            var output = p.StandardOutput.ReadToEnd();
            return false;
        }
        return true;
    }
*/
}
