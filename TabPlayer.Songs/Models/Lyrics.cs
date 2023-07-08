using System.Text;
using Newtonsoft.Json;

namespace murph9.TabPlayer.Songs.Models;

public class Lyrics
{
    public ICollection<LyricLine> Lines { get; private set; }

    public Lyrics(List<LyricLine> lines) {
        Lines = lines ?? new List<LyricLine>();
    }
}

public class LyricLine
{
    [JsonIgnore]
    public float StartTime { get; private set; }
    [JsonIgnore]
    public float EndTime { get; private set; }

    public ICollection<Lyric> Words { get; private set; }

    public LyricLine(List<Lyric> words) {
        if (words.Count() < 1)
            throw new ArgumentException("You must give at least one word", nameof(words));
        Words = words;

        StartTime = words.First().Time;
        EndTime = words.Last().Time + words.Last().Length;
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var s in Words) {
            sb.Append(s.Text);
            if (!s.Text.EndsWith("-"))
                sb.Append(" ");
        }
        return sb.ToString();
    }

    public (string, string) GetParts(double songPosition) {
        var firstB = new StringBuilder();
        var secondB = new StringBuilder();
        foreach (var s in Words) {
            if (s.Time <= songPosition) {
                firstB.Append(s.Text);
                if (!s.Text.EndsWith("-"))
                    firstB.Append(" ");
            } else {
                secondB.Append(s.Text);
                if (!s.Text.EndsWith("-"))
                    secondB.Append(" ");
            }
        }
        return (firstB.ToString(), secondB.ToString());
    }
}

public record Lyric(string Text, float Time, float Length);
