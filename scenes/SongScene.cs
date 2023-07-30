using Godot;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
using System;
using System.IO;

public partial class SongScene : Node
{
	private SongState _state;
	private AudioController _audioController;

	private Mesh _mesh;

	public void init(SongState state) {
		_state = state;

		_audioController = new AudioController(_state.AudioStream);

		var controlsLabel = GetNode<Label>("ControlsLabel");
		controlsLabel.Text = $@"Controls:
------------------------
[Space]  Play/Pause
[R]      Restart
[F]      Go to first note (-2sec)
[W], [S] Seek forward/backward 10 sec
[Q]      Quit and pick a new song
";

		_mesh = new Mesh();
	}

	public override void _Ready()
	{
		var info = _state.SongInfo;
		setUILabels(info);

		_audioController.Play();

		// var packedScene = ResourceLoader.Load<PackedScene>("res://scenes/song/GuitarChart.tscn");
		
		var guitarChart = GD.Load<CSharpScript>("res://scenes/song/GuitarChart.cs");
		GetTree().Root.AddChild(guitarChart.New().As<GuitarChart>());

		try {
			var songScene = GD.Load<CSharpScript>("res://scenes/song/SongChart.cs");
			var s = songScene.New().As<SongChart>();
			s.Init(_state.Instrument);
			GetTree().Root.AddChild(s);
		} catch (Exception e) {
			GD.Print(e);
		}

		// TODO test
		// var obj = MeshGenerator.Box(Colors.Red, new Vector3(20, 0, 7));
		// GetTree().Root.AddChild(obj);
	}

	public override void _Process(double delta)
	{
		var guiScene = GetNode<Node3D>("/root/guitarSceneRoot");
		var newPos = new Vector3((float)_audioController.SongPosition*_state.Instrument.Config.NoteSpeed, guiScene.Position.Y, guiScene.Position.Z);
		guiScene.Position = newPos;

		var a = GetNode<Label>("RunningDetailsLabel");
		a.Text = Engine.GetFramesPerSecond() + "fps @ " + GetTree().Root.GetCamera3D().GlobalTransform.Origin+"\nYo?";

		var nextNote = NextNoteBlock();
		var noteText = (nextNote == null) ? "No note" : "Next: " + Math.Round(nextNote.Time, 3) + " in " + Math.Round(nextNote.Time - _audioController.SongPosition, 1);

		var debugText = @$"{_audioController.SongPosition.ToMinSec(true)}
Obj #: {GetTree().Root.GetChildCount()}
{Engine.GetFramesPerSecond()}fps | {(delta*1000).ToString("000.0")}ms
{noteText}
";
		a.Text = debugText;
	}

	private void setUILabels(SongInfo info) {
		var infoLabel = GetNode<Label>("SongInfoLabel");
		infoLabel.Text = $"{info.Metadata.Name} ({info.Metadata.Year})\n{info.Metadata.Artist}";

		var detailsLabel = GetNode<Label>("SongDetailsLabel");
		var guitarTuning = "Tuning: " + _state.Instrument.CalcFullTuningStr();
		var chordCount = _state.Instrument.Notes.Where(x => x.Notes.Count() > 1).Count();
		var singleNoteCount = _state.Instrument.Notes.Count() - chordCount;
		detailsLabel.Text = $@"Playing: {_state.Instrument.Name}
Notes: {singleNoteCount}
Chords: {chordCount}
{guitarTuning}
First note @ {_state.Instrument.Notes.First().Time.ToMinSec()}
Last note @ {_state.Instrument.Notes.Last().Time.ToMinSec()}";
	}

	private NoteBlock NextNoteBlock() {
		foreach (var b in _state.Instrument.Notes) {
			if (b.Time > _audioController.SongPosition)
				return b;
		}
		return null;
	}
}
