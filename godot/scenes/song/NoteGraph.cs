using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;

namespace murph9.TabPlayer.scenes.song;

public partial class NoteGraph : Node {
    
    private const float BUCKET_SIZE = 3;
    private const float BUCKET_PIXEL_WIDTH = 7;
    private const float BUCKET_NOTE_PIXEL_HEIGHT = 5;
    private const float BUCKET_BOTTOM_OFFSET = 10;

    private Color _chordColour = new(0, 0, 0.3f, 0.5f);
    private Color _noteColour = new(0.3f, 0f, 0, 0.5f);
    private Color _uniqueColour = new(0, 0, 0, 0.5f);

    private SongState _songState;
    private IAudioStreamPosition _audio;

    private readonly Dictionary<int, ColorRect> _graphChordBars = new();
    private readonly Dictionary<int, ColorRect> _graphNoteBars = new();
    private readonly Dictionary<int, ColorRect> _graphUniqueBars = new();
    // TODO show which strings each section has

    public void _init(SongState songState, IAudioStreamPosition audio) {
        _songState = songState;
        _audio = audio;
    }

	public override void _Ready()
	{
        var barNode = new Control() {
            Name = "BarNode",
            LayoutMode = 3,
            AnchorsPreset = 3,
        };
        AddChild(barNode);
        
        // TODO align to section starts
        var noteBuckets = _songState.Instrument.Notes.ToLookup(x => Math.Round(x.Time/BUCKET_SIZE));
        var maxBucket = Math.Round(noteBuckets.Last().Key + 1);
        for (var i = 0; i < maxBucket; i++) {
            // get count of note and count of chords
            var totalCount = noteBuckets[i].Count();

            var chordCount = noteBuckets[i].Count(x => x.Notes.Count() > 1);
            var chordHeight = chordCount*BUCKET_NOTE_PIXEL_HEIGHT;
            
            var noteCount = totalCount - chordCount;
            var noteHeight = noteCount*BUCKET_NOTE_PIXEL_HEIGHT;

            var uniqueList = new List<NoteBlock>();
            foreach (var n in noteBuckets[i]) {
                var foundSimilar = false;
                foreach (var u in uniqueList) {
                    if (u.IsSameChordAs(n, maxInterval: float.MaxValue)) {
                        foundSimilar = true;
                        break;
                    }
                }
                if (!foundSimilar) {
                    uniqueList.Add(n);
                }
            }
            var uniquePercent = uniqueList.Count / Math.Max(1f, totalCount);

            _graphChordBars.Add(i, new ColorRect() {
                Color = _chordColour,
                AnchorsPreset = 3,
                OffsetTop = -BUCKET_BOTTOM_OFFSET - chordHeight,
                OffsetLeft = -BUCKET_PIXEL_WIDTH * (float)(maxBucket - i),
                Size = new Vector2(BUCKET_PIXEL_WIDTH, chordHeight)
            });
            _graphNoteBars.Add(i, new ColorRect() {
                Color = _noteColour,
                AnchorsPreset = 3,
                OffsetTop = -BUCKET_BOTTOM_OFFSET - chordHeight - noteHeight,
                OffsetLeft = -BUCKET_PIXEL_WIDTH*(float)(maxBucket - i),
                Size = new Vector2(BUCKET_PIXEL_WIDTH, noteHeight)
            });
            _graphUniqueBars.Add(i, new ColorRect() {
                Color = _uniqueColour,
                AnchorsPreset = 3,
                OffsetTop = -BUCKET_BOTTOM_OFFSET,
                OffsetLeft = -BUCKET_PIXEL_WIDTH*(float)(maxBucket - i),
                Size = new Vector2(BUCKET_PIXEL_WIDTH, BUCKET_BOTTOM_OFFSET * uniquePercent)
            });
        }
        foreach (var n in _graphChordBars) {
            barNode.AddChild(n.Value);
        }
        foreach (var n in _graphNoteBars) {
            barNode.AddChild(n.Value);
        }
        foreach (var n in _graphUniqueBars) {
            barNode.AddChild(n.Value);
        }
	}

	public override void _Process(double delta)
	{
        if (!_audio.SongPlaying)
			return;

        var bucket = Math.Round(_audio.GetSongPosition()/BUCKET_SIZE);

        foreach (var bar in _graphChordBars) {
            if (bar.Key == bucket)
                bar.Value.Color = _chordColour * 2;
            else
                bar.Value.Color = _chordColour;
        }

        foreach (var bar in _graphNoteBars) {
            if (bar.Key == bucket)
                bar.Value.Color = _noteColour * 2;
            else
                bar.Value.Color = _noteColour;
        }

        foreach (var bar in _graphUniqueBars) {
            if (bar.Key == bucket)
                bar.Value.Color = _uniqueColour * 2;
            else
                bar.Value.Color = _uniqueColour;
        }
	}
}
