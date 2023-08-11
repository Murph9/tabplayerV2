using Godot;
using murph9.TabPlayer.Songs;
using System;

namespace murph9.TabPlayer;

public partial class StartMenu : Node
{
	[Signal]
	public delegate void ClosedEventHandler();
	[Signal]
	public delegate void SongListOpenedEventHandler();
	[Signal]
	public delegate void ConvertMenuOpenedEventHandler();
	[Signal]
	public delegate void InfoMenuOpenedEventHandler();

	public override void _Ready() { }

	public override void _Process(double delta) { }

	public void StartButton_Pressed() {
		EmitSignal(SignalName.SongListOpened);
	}

	public void InfoButton_Pressed() {
		EmitSignal(SignalName.InfoMenuOpened);
	}

	public void ConvertButton_Pressed() {
		EmitSignal(SignalName.ConvertMenuOpened);
	}

	public void ReloadButton_Pressed() {
		// TODO cleanup so that there is UI progress
		SongFileManager.GetSongFileList(GD.Print, true);
	}

	public void QuitButton_Pressed() {
		EmitSignal(SignalName.Closed);
	}
}
