using Godot;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
using System;

public partial class SongScene : Node
{
	private SongInfo _info;

	public void init(SongInfo info, Instrument i) {

		// TODO this needs to be given a SongInfo but for now we hard code load one
		var d = new DirectoryInfo(@"C:\Users\murph\AppData\Local\murph9.TabPlayer\3-Doors-Down_Kryptonite");
		var a = SongLoader.Load(d, "combo");
		this._info = a.SongInfo;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("please");
		var a = GetNode<Label>("SongInfoLabel");
		a.Text = _info.Metadata.Name;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
