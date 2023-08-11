namespace murph9.TabPlayer.Songs;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using murph9.TabPlayer.Songs.Models;
using Newtonsoft.Json;

public class SongFileManager
{
    public readonly static string SONG_FOLDER;
    public readonly static string PLAY_DATA_FILE;
    static SongFileManager() {
        SONG_FOLDER = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "murph9.TabPlayer");
        PLAY_DATA_FILE = Path.Combine(SONG_FOLDER, "playData.json");
    }

    private static FileStream EnsureConfigExists() {
        var dir = new DirectoryInfo(SONG_FOLDER);
        if (!dir.Exists)
            dir.Create();

        if (!File.Exists(PLAY_DATA_FILE))
            return File.Create(PLAY_DATA_FILE);
        return new FileStream(PLAY_DATA_FILE, FileMode.Open);
    }

    public static DirectoryInfo GetOrCreateSongFolder(SongInfo info) {
        EnsureConfigExists();

        var folderName = $"{info.Metadata.Artist}_{info.Metadata.Name}".Replace('/', '-').Replace(' ', '-');
        folderName = string.Concat(folderName.Split(Path.GetInvalidFileNameChars()));
        var dir = new DirectoryInfo(Path.Combine(SONG_FOLDER, folderName));
        
        if (!dir.Exists)
            dir.Create();
        return dir;
    }

    public static SongFileList GetSongFileList(Action<string> output, bool update = false) {
        if (!update) {
            var configFile = EnsureConfigExists();

            using var sr = new StreamReader(configFile);
            string result = sr.ReadToEnd();
            output("Reading existing song file from: " + result);
            
            return JsonConvert.DeserializeObject<SongFileList>(result) ?? new SongFileList();
        }
        
        var songList = new SongFileList();
        output("Loading all songs from: " + SONG_FOLDER);
        var songDirs = Directory.EnumerateDirectories(SONG_FOLDER).Select(x => new DirectoryInfo(x)).ToList();
        
        int total = songDirs.Count;
        int i = 0;
        foreach (var songDir in songDirs) {
            var file = songDir.GetFiles("*.json").FirstOrDefault();
            if (file == null) continue;
            
            var noteInfo = JsonConvert.DeserializeObject<SongInfo>(File.ReadAllText(file.FullName));
            output($"Reading all songs {i}/{total}. Current: {noteInfo?.Metadata?.Name}");
            var instruments = noteInfo.Instruments.Select(x => new SongFileInstrument(x.Name, x == noteInfo.MainInstrument, x.Config.Tuning, x.Notes.Count, x.Config.CapoFret)).ToArray();
            var lyrics = noteInfo.Lyrics != null ? new SongFileLyrics(noteInfo.Lyrics.Lines?.Sum(x => x.Words?.Count) ?? 0) : null;
            songList.Data.Add(new SongFile(songDir.Name, noteInfo.Metadata.Name,
                noteInfo.Metadata.Artist, noteInfo.Metadata.Album, noteInfo.Metadata.Year,
                noteInfo.MainInstrument.LastNoteTime, instruments, lyrics));
            
            i++;
        }
        songList.Data.Sort((x, y) => x.SongName.CompareTo(y.SongName)); // default sort of name

        output($"Completed {total}/{total}. Sorting");
        var songListContents = JsonConvert.SerializeObject(songList);
        File.WriteAllText(PLAY_DATA_FILE, songListContents);

        output($"Completed all.");
        return songList;
    }
}

public class SongFileList {
    public readonly List<SongFile> Data = new();
}

public record SongFile(string FolderName, string SongName, string Artist, string Album, int? Year, float Length, ICollection<SongFileInstrument> Instruments, SongFileLyrics Lyrics)
{
    public string GetInstrumentChars()
    {
        var chars = new char[] { ' ', ' ', ' ', ' ', ' '};
        if (this.Instruments.Any(x => x.Name == SongInfo.LEAD_NAME))
            chars[0] = 'L';
        if (this.Instruments.Any(x => x.Name == SongInfo.RHYTHM_NAME))
            chars[1] = 'R';
        if (this.Instruments.Any(x => x.Name == SongInfo.BASS_NAME))
            chars[2] = 'B';
        if (this.Lyrics != null && this.Lyrics.WordCount > 0)
            chars[3] = 'V';
        
        var otherInstruments = this.Instruments.Count(x => !SongInfo.STANDARD_INSTRUMENT_TYPES.Contains(x.Name));
        if (otherInstruments > 0) {
            if (otherInstruments.ToString().Length > 0)
                chars[4] = otherInstruments.ToString()[0];
        }
        
        return new string(chars);
    }

    public SongFileInstrument GetMainInstrument() {
        return this.Instruments.FirstOrDefault(x => x.IsMain);
    }
}

public record SongFileInstrument(string Name, bool IsMain, short[] Tuning, int NoteCount, float CapoFret)
{
    public float GetNoteDensity(SongFile song) {
        return this.NoteCount / song.Length;
    }
}

public record SongFileLyrics(int WordCount);
