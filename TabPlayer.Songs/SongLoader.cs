using murph9.TabPlayer.Songs.Models;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace murph9.TabPlayer.Songs;

public class SongLoader {
    
    public static SongState Load(string folderName, string instrumentType) {
        var folder = new DirectoryInfo(Path.Combine(SongFileManager.SONG_FOLDER, folderName));
        var noteInfoFile = folder.GetFiles("*.json").First();
        var noteInfo = JsonConvert.DeserializeObject<SongInfo>(File.ReadAllText(noteInfoFile.FullName));

        var oggFile = folder.GetFiles("*.ogg").SingleOrDefault();
        var ms = new MemoryStream();
        File.OpenRead(oggFile.FullName).CopyTo(ms);
        var audio = ms.ToArray();
        
        return new SongState() {
            SongInfo = noteInfo,
            Instrument = noteInfo?.Instruments.First(x => x.Name == instrumentType),
            Audio = audio
        };
    }
}

public class SongState {
    public SongInfo? SongInfo;
    public Instrument? Instrument;
    public byte[]? Audio;
}
