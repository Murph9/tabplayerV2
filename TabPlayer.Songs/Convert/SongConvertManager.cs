using murph9.TabPlayer.Songs.Models;
using Newtonsoft.Json;
using Rocksmith2014PsarcLib.Psarc;
using System;
using System.IO;

namespace murph9.TabPlayer.Songs.Convert
{
    public class SongConvertManager
    {
        public enum SongType {
            Psarc, Midi
        }
        public static async Task<bool> ConvertFile(SongType type, string location, bool reconvert, Action<string> output) {
            bool success = false;
            
            try {
                if (type == SongType.Midi)
                    success = SongConvertManager.ExportMidi(new DirectoryInfo(location), output);
                if (type == SongType.Psarc)
                    success = await SongConvertManager.ExportPsarc(new FileInfo(location), reconvert, output);
            } catch (Exception e) {
                Console.WriteLine(e);
                Console.WriteLine($"Failed to convert file '{location}'");
                return false;
            }
            
            return success;
        }

        private static async Task<bool> ExportPsarc(FileInfo file, bool reconvert, Action<string> output) {
            PsarcFile p = null;
            try {
                output("Converting: " + file.FullName);
                p = new PsarcFile(file.FullName);

                var songNames = p.ExtractArrangementManifests().Select(x => x.Attributes.BlockAsset).Distinct();
                if (songNames.Count() <= 1)
                    await ExportSinglePsarc(p, null, reconvert, output);
                else
                    foreach (var name in songNames) {
                        try {
                            await ExportSinglePsarc(p, name.Split(':').Last(), reconvert, output);
                        } catch (System.IO.IOException e) {
                            Console.WriteLine(e);
                            output("Failed to convert: " + name.Split(':').Last());
                        } catch (Exception) {
                            throw;
                        }
                    }
                
            } catch (FileLoadException ef) {
                output("Failed to convert file, see last error in log");
                Console.WriteLine(ef);
                Console.WriteLine($"File format was weird for {file.FullName}");
                return false;
            } catch (Exception e) {
                output("Failed to convert file, see last error in log");
                Console.WriteLine(e);
                Console.WriteLine($"Failed to convert file: {file.FullName}");
                return false;
            } finally {
                Console.WriteLine($"Converted {file.FullName}");
                p?.Dispose();
            }
            
            return true;
        }

        private static async Task<bool> ExportSinglePsarc(PsarcFile p, string songFilter, bool reconvert, Action<string> output)
        {
            var converter = new PsarcFileConverter(p, songFilter);
            var noteInfo = converter.ConvertToSongInfo();

            var outputFolder = SongFileManager.GetOrCreateSongFolder(noteInfo);
            if (!reconvert && outputFolder.GetFiles("*.json").Length > 0) {
                // don't convert a file again
                Console.WriteLine("Output folder already exists, please use arg to reconvert");
                output("Did not convert because it already exists, please start application with a special arg to reconvert");
                return true; // already converted
            }
            foreach (var f in outputFolder.GetFiles("*.json"))
                f.Delete();
            
            await WriteSongInfo(noteInfo, outputFolder.FullName, "data");

            output("Writing audio to " + outputFolder.FullName);

            var wemFile = converter.ExportPsarcMainWem(outputFolder.FullName, forceConvert: reconvert);
            var oggFile = AudioConverter.ConvertWemToOgg(wemFile, forceConvert: reconvert);
            Console.WriteLine($"Created ogg file: {oggFile}");

            output("Wrote " + noteInfo?.Metadata?.Name + " info to " + outputFolder.FullName);

            return true;
            
        }

        private static async Task<string> WriteSongInfo(SongInfo info, string tempFolder, string dataFileName) {
            var jsonFile = Path.Join(tempFolder, dataFileName + ".json");
            await File.WriteAllTextAsync(jsonFile, JsonConvert.SerializeObject(info, Newtonsoft.Json.Formatting.None, 
                        new JsonSerializerSettings { 
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        }));
            Console.WriteLine($"Wrote song info to file: {jsonFile}");
            return jsonFile;
        }

        private static bool ExportMidi(DirectoryInfo f, Action<string> output) {
            throw new Exception("Midi not supported, here for reference");
        }
    }
}