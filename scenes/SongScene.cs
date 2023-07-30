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
		var a = GetNode<Label>("SongInfoLabel");
		a.Text = $"{info.Metadata.Name} ({info.Metadata.Year})\n{info.Metadata.Artist}";

		_audioController.Play();

		// var packedScene = ResourceLoader.Load<PackedScene>("res://scenes/song/GuitarChart.tscn");
		
		var guitarChart = GD.Load<CSharpScript>("res://scenes/song/GuitarChart.cs");
		GetTree().Root.AddChild(guitarChart.New().As<GuitarChart>());
/*
		try {
			var songScene = GD.Load<CSharpScript>("res://scenes/song/SongChart.cs");
			var s = songScene.New().As<SongChart>();
			s.Init(_state.Instrument);
			GetTree().Root.AddChild(s);
		} catch (Exception e) {
			GD.Print(e);
		}
*/
		// TODO test
		// var obj = MeshGenerator.Box(new Color("red"), new Vector3(20, 0, 7));
		// GetTree().Root.AddChild(obj);
	}

	public override void _Process(double delta)
	{
		var guiScene = GetNode<Node3D>("/root/guitarSceneRoot");
		var newPos = new Vector3((float)_audioController.SongPosition*_state.Instrument.Config.NoteSpeed, guiScene.Position.Y, guiScene.Position.Z);
		guiScene.Position = newPos;

		var a = GetNode<Label>("RunningDetailsLabel");
		a.Text = GetTree().Root.GetCamera3D().GlobalTransform.Origin+"";
	}
}
