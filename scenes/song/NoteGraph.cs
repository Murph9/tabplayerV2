using Godot;
using murph9.TabPlayer.Songs;

namespace murph9.TabPlayer.scenes.song;

public partial class NoteGraph : Node {
    
    private const float BUCKET_SIZE = 3;
    private const float BUCKET_PIXEL_WIDTH = 7;
    private const float BUCKET_NOTE_PIXEL_HEIGHT = 5;

    private Color _chordColour = new(0, 0, 0.3f, 0.5f);
    private Color _noteColour = new(0.3f, 0f, 0, 0.5f);

    private SongState _songState;
    private AudioController _audioController;

    private readonly Dictionary<int, ColorRect> _graphChordBars = new();
    private readonly Dictionary<int, ColorRect> _graphNoteBars = new();

    public void _init(SongState songState, AudioController audio) {
        _songState = songState;
        _audioController = audio;
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
            
            var chordCount = noteBuckets[i]?.Count(x => x.Notes.Count() > 1) ?? 0;
            var chordHeight = chordCount*BUCKET_NOTE_PIXEL_HEIGHT;
            
            var noteCount = (noteBuckets[i]?.Count() ?? 0) - chordCount;
            var noteHeight = noteCount*BUCKET_NOTE_PIXEL_HEIGHT;
            
            _graphChordBars.Add(i, new ColorRect() {
                Color = _chordColour,
                AnchorsPreset = 3,
                OffsetTop = -5 - chordHeight,
                OffsetLeft = -BUCKET_PIXEL_WIDTH * (float)(maxBucket - i),
                Size = new Vector2(BUCKET_PIXEL_WIDTH, chordHeight)
            });
            _graphNoteBars.Add(i, new ColorRect() {
                Color = _noteColour,
                AnchorsPreset = 3,
                OffsetTop = -5 - chordHeight - noteHeight,
                OffsetLeft = -BUCKET_PIXEL_WIDTH*(float)(maxBucket - i),
                Size = new Vector2(BUCKET_PIXEL_WIDTH, noteHeight)
            });
        }
        foreach (var n in _graphChordBars) {
            barNode.AddChild(n.Value);
        }
        foreach (var n in _graphNoteBars) {
            barNode.AddChild(n.Value);
        }
	}

	public override void _Process(double delta)
	{
        if (!_audioController.Active)
			return;

        var bucket = Math.Round(_audioController.SongPosition/BUCKET_SIZE);

        foreach (var bar in _graphChordBars) {
            if (bar.Key == bucket)
                bar.Value.Color = Colors.White;
            else
                bar.Value.Color = _chordColour;
        }

        foreach (var bar in _graphNoteBars) {
            if (bar.Key == bucket)
                bar.Value.Color = Colors.White;
            else
                bar.Value.Color = _noteColour;
        }
	}
}
