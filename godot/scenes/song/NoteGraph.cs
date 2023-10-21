using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.Songs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace murph9.TabPlayer.scenes.song;

public partial class NoteGraph : Node {
    
    private const float BUCKET_SIZE = 3;
    private const float BUCKET_PIXEL_WIDTH = 7;
    private const float BUCKET_NOTE_PIXEL_HEIGHT = 5;
    private const float BUCKET_BOTTOM_OFFSET = 20;

    partial class GraphSection : Control {
        public Vector2 Pos;
        public readonly ColorRect Chord;
        public readonly ColorRect Note;
        public readonly ColorRect[] StringNums;

        public GraphSection(Vector2 offsetPos, ColorRect chord, ColorRect note, ColorRect[] strings) {
            AnchorsPreset = 3;
            OffsetTop = offsetPos.Y;
            OffsetLeft = offsetPos.X;
            Pos = offsetPos;
            Chord = chord;
            Note = note;
            StringNums = strings;

            AddChild(chord);
            AddChild(note);
            foreach (var s in StringNums) {
                AddChild(s);
            }
        }
    }

    private Color _chordColour = new(0, 0, 0.3f, 0.5f);
    private Color _noteColour = new(0.3f, 0f, 0, 0.5f);
    private Color _uniqueColour = new(0, 0, 0, 0.5f);

    private SongState _songState;
    private IAudioStreamPosition _audio;

    private readonly Dictionary<int, GraphSection> _graphBars = new();
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

        var stringColours = SettingsService.Settings().StringColours.ToArray();
        var stringOrder = Enumerable.Range(0, 6);
            if (SettingsService.Settings().LowStringIsLow)
                stringOrder = stringOrder.Reverse();

        // TODO align to section starts
        var noteBuckets = _songState.Instrument.Notes.ToLookup(x => Math.Round(x.Time/BUCKET_SIZE));
        var maxBucket = Math.Round(noteBuckets.Last().Key + 3);
        for (var i = 0; i < maxBucket; i++) {
            // get count of note and count of chords
            var totalCount = noteBuckets[i].Count();

            var chordCount = noteBuckets[i].Count(x => x.Notes.Count() > 1);
            var chordHeight = chordCount*BUCKET_NOTE_PIXEL_HEIGHT;
            
            var noteCount = totalCount - chordCount;
            var noteHeight = noteCount*BUCKET_NOTE_PIXEL_HEIGHT;

            var stringCounts = new float[] { 0,0,0,0,0,0 };
            foreach (var n in noteBuckets[i]) {
                foreach (var a in n.Notes) {
                    stringCounts[a.StringNum]++;
                }
            }
            var stringSum = stringCounts.Sum();
            
            var chord = new ColorRect() {
                Color = _chordColour,
                AnchorsPreset = 3,
                OffsetTop = -chordHeight,
                Size = new Vector2(BUCKET_PIXEL_WIDTH, chordHeight)
            };
            var note = new ColorRect() {
                Color = _noteColour,
                AnchorsPreset = 3,
                OffsetTop = -chordHeight - noteHeight,
                Size = new Vector2(BUCKET_PIXEL_WIDTH, noteHeight)
            };

            var stringOffsetPos = 0f;
            var stringNums = stringOrder.Select(x => {
                var newOffset = stringOffsetPos;
                var y = 0f;
                if (stringSum > 0) {
                    y = BUCKET_BOTTOM_OFFSET * stringCounts[x]/stringSum;
                }
                stringOffsetPos += y; // for the next one
                return new ColorRect() { 
                    Color = stringColours[x] * 0.6f,
                    AnchorsPreset = 3,
                    OffsetTop = newOffset,
                    Size = new Vector2(BUCKET_PIXEL_WIDTH - 2, y)
                };
            }).ToArray();

            var g = new GraphSection(new Vector2(-BUCKET_PIXEL_WIDTH*(float)(maxBucket - i), -BUCKET_BOTTOM_OFFSET), chord, note, stringNums);
            _graphBars.Add(i, g);
            barNode.AddChild(g);
        }
	}

	public override void _Process(double delta)
	{
        if (!_audio.SongPlaying)
			return;

        var bucket = Math.Round(_audio.GetSongPosition()/BUCKET_SIZE);

        foreach (var bar in _graphBars) {
            if (bar.Key == bucket) {
                bar.Value.Chord.Color = _chordColour * 2;
                bar.Value.Note.Color = _noteColour * 2;
            } else {
                bar.Value.Chord.Color = _chordColour;
                bar.Value.Note.Color = _noteColour;
            }
        }
	}
}
