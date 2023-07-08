using NAudio.Vorbis;
using NAudio.Wave;
using System.Diagnostics;
using System.IO.Packaging;

namespace murph9.TabPlayer.Songs.Convert;

public class AudioConverter
{
    public static void ConvertOggToWav(FileInfo oggFile, Stream stream)
    {
        using (VorbisWaveReader vorbisStream = new VorbisWaveReader(oggFile.FullName))
        {
            WaveFileWriter.WriteWavFileToStream(stream, vorbisStream);
        }
    }

    public static async Task<string> ConvertWemToOgg(string file, bool runOggPerf = false, bool forceConvert = false)
    {
        var outputFile = Path.Join(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + ".ogg";
        if (File.Exists(outputFile) && !forceConvert)
        {
            return outputFile;
        }

        var p = new Process
        {
            StartInfo =
                    {
                        FileName = "ww2ogg.exe",
                        Arguments = $"\"{file}\" --pcb ./packed_codebooks_aoTuV_603.bin",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
        };
        p.Start();
        await p.WaitForExitAsync();

        var oggFile = Directory.GetFiles(Path.GetDirectoryName(file), "*.ogg").SingleOrDefault();
        if (oggFile == null)
        {
            throw new Exception($"ww2ogg.exe has failed could not convert {oggFile}");
        }

        if (runOggPerf)
        {
            var result = false;
            int attempts = 10;
            while (!result && attempts > 0)
            {
                // try again
                result = await Revorb(oggFile);
                attempts -= 1;
                Thread.Sleep(10); // wait a very short time for file locks to clear
            }
            if (attempts == 0)
            {
                throw new Exception($"revorb could not convert {oggFile}");
            }
        }

        return oggFile;
    }

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
}
