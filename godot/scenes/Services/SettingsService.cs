using System.Collections.Generic;
using System.IO;
using Godot;
using murph9.TabPlayer.Songs;
using Newtonsoft.Json;

namespace murph9.TabPlayer.scenes.Services;

public record Settings(IList<Color> StringColours, bool LowStringIsLow);

public class SettingsService {
    
    private readonly static string SETTINGS_FILE = Path.Combine(SongFileManager.SONG_FOLDER, "settings.json");

    private readonly static Settings DEFAULT = new(new List<Color>() {
            Colors.Red,
            Colors.Yellow,
            Colors.Blue,
            Colors.Orange,
            Colors.Green,
            Colors.Purple
        }, true);

    private static Settings _settings;
    public static Settings Settings() {
        if (_settings == null) {
            var dir = new DirectoryInfo(SongFileManager.SONG_FOLDER);
            if (!dir.Exists)
                dir.Create();

            if (!File.Exists(SETTINGS_FILE)) {
                UpdateSettings(null);
            }

            using var sr = new StreamReader(SETTINGS_FILE);
            string result = sr.ReadToEnd();
        
            _settings = JsonConvert.DeserializeObject<Settings>(result) ?? DEFAULT;
        }

        return _settings;
    }

    public static void UpdateSettings(Settings settings) {
        using var file = File.CreateText(SETTINGS_FILE);
        var content = JsonConvert.SerializeObject(settings ?? DEFAULT, Formatting.Indented);
        file.Write(content);
        file.Flush(); // so we can read it right now?

        ReloadSettings();
    }

    public static void ReloadSettings() {
        _settings = null; // very good reload
    }

    public static Color GetColorFromStringNum(int num) {
        return Settings().StringColours[num];
    }
}
