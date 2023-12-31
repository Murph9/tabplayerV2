﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using murph9.TabPlayer.Songs.Models;
using Newtonsoft.Json;

namespace murph9.TabPlayer.Songs;

public class SongFileManager
{
    public readonly static string SONG_FOLDER;
    public readonly static string PLAY_DATA_FILE;
    public readonly static string ALBUM_ART_NAME = "album.png";
    static SongFileManager() {
        SONG_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "murph9.TabPlayer");
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

    public static SongFileList GetSongFileList() {
        var configFile = EnsureConfigExists();

        using var sr = new StreamReader(configFile);
        string result = sr.ReadToEnd();
        
        return JsonConvert.DeserializeObject<SongFileList>(result) ?? new SongFileList();
    }

    public static void UpdateSongList(Action<string> output) {
        var songList = new SongFileList();
        output("Loading all songs from: " + SONG_FOLDER);
        var songDirs = Directory.EnumerateDirectories(SONG_FOLDER).Select(x => new DirectoryInfo(x)).ToList();
        
        int total = songDirs.Count;
        int i = 0;
        foreach (var songDir in songDirs) {
            output($"{i}/{total} Reading all songs, current: {songDir.Name}");
            var song = ReadSingleSong(songDir);
            if (song == null)
                continue;
            songList.Data.Add(song);

            i++;
        }
        songList.Data.Sort((x, y) => x.SongName.CompareTo(y.SongName)); // default sort of name

        output($"Completed {total}/{total}. Saving");
        var songListContents = JsonConvert.SerializeObject(songList);
        File.WriteAllText(PLAY_DATA_FILE, songListContents);

        output($"Loaded all songs {total}/{total} into {PLAY_DATA_FILE}");
    }

    public static bool AddSingleSong(string dataFolderName) {
        var songList = GetSongFileList();
        var song = ReadSingleSong(new DirectoryInfo(dataFolderName));
        if (song == null)
            return false;
        songList.Data.Add(song);
        var songListContents = JsonConvert.SerializeObject(songList);
        File.WriteAllText(PLAY_DATA_FILE, songListContents);
        return true;
    }

    private static SongFile? ReadSingleSong(DirectoryInfo songDir) {
        var file = songDir.GetFiles("*.json").FirstOrDefault();
        if (file == null) return null;
        
        var noteInfo = JsonConvert.DeserializeObject<SongInfo>(File.ReadAllText(file.FullName));
        
        var instruments = noteInfo.Instruments.Select(x => new SongFileInstrument(x.Name, x == noteInfo.MainInstrument, x.Config.Tuning, x.TotalNoteCount(), x.Config.CapoFret)).ToArray();
        var lyrics = noteInfo.Lyrics != null ? new SongFileLyrics(noteInfo.Lyrics.Lines?.Sum(x => x.Words?.Count) ?? 0) : null;
        return new SongFile(songDir.Name, noteInfo.Metadata.Name,
            noteInfo.Metadata.Artist, noteInfo.Metadata.Album, noteInfo.Metadata.Year,
            noteInfo.MainInstrument.LastNoteTime, instruments, lyrics);
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
        if (Instruments.Any(x => x.Name == SongInfo.LEAD_NAME || x.Name == SongInfo.LEAD1_NAME || x.Name == SongInfo.LEAD2_NAME))
            chars[0] = 'L';
        if (Instruments.Any(x => x.Name == SongInfo.RHYTHM_NAME || x.Name == SongInfo.RHYTHM1_NAME || x.Name == SongInfo.RHYTHM2_NAME))
            chars[1] = 'R';
        if (Instruments.Any(x => x.Name == SongInfo.BASS_NAME || x.Name == SongInfo.BASS1_NAME || x.Name == SongInfo.BASS2_NAME))
            chars[2] = 'B';
        if (Lyrics != null && Lyrics.WordCount > 0)
            chars[3] = 'V';
        
        var otherInstruments = Instruments.Count(x => !SongInfo.STANDARD_INSTRUMENT_TYPES.Contains(x.Name));
        if (otherInstruments > 0) {
            if (otherInstruments.ToString().Length > 0)
                chars[4] = otherInstruments.ToString()[0];
        }
        
        return new string(chars);
    }

    public SongFileInstrument GetMainInstrument() {
        return Instruments.FirstOrDefault(x => x.IsMain);
    }
}

public record SongFileInstrument(string Name, bool IsMain, short[] Tuning, int NoteCount, float CapoFret)
{
    public float GetNoteDensity(SongFile song) {
        return NoteCount / song.Length;
    }
}

public record SongFileLyrics(int WordCount);
