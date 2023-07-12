using murph9.TabPlayer.Songs.Convert;
using murph9.TabPlayer.Songs.Models;
using Newtonsoft.Json;

namespace murph9.TabPlayer.Songs;

public class SongLoader {
    
    public static SongState Load(DirectoryInfo folder, string instrumentType) {
        
        var noteInfoFile = folder.GetFiles("*.json").First();
        var noteInfo = JsonConvert.DeserializeObject<SongInfo>(File.ReadAllText(noteInfoFile.FullName));

        var wavFile = folder.GetFiles("*.wav").SingleOrDefault();
        Stream stream;
        if (wavFile != null) {
            stream = File.OpenRead(wavFile.FullName);
        } else {
            // its an ogg, so convert it
            var oggFile = folder.GetFiles("*.ogg").SingleOrDefault();
            
            stream = new MemoryStream();
            AudioConverter.ConvertOggToWav(oggFile, stream); // ogg conversion makes 100mb files
            stream.Seek(0, SeekOrigin.Begin);
        }
        
        return new SongState() {
            SongInfo = noteInfo,
            Instrument = noteInfo?.Instruments.First(x => x.Name == instrumentType),
            AudioStream = stream
        };
    }
}

public class SongState {
    public SongInfo? SongInfo;
    public Instrument? Instrument;
    public Stream? AudioStream;
}