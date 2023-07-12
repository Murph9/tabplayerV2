using Godot;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
using System;
using System.IO;

public partial class SongScene : Node
{
	private SongState _state;
	private AudioController _audioController;

	public void init(SongInfo info, Instrument i) {
		// TODO this needs to be given a SongInfo but for now we hard code load one
		var d = new DirectoryInfo(@"C:\Users\murph\AppData\Local\murph9.TabPlayer\3-Doors-Down_Kryptonite");
		this._state = SongLoader.Load(d, "combo");

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
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var info = _state.SongInfo;
		var a = GetNode<Label>("SongInfoLabel");
		a.Text = $"{info.Metadata.Name} ({info.Metadata.Year})\n{info.Metadata.Artist}";

		_audioController.Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var a = GetNode<Label>("RunningDetailsLabel");
		a.Text = _audioController.SongPosition +"";
	}
}
