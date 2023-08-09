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

	public override void _Ready()
	{
		
	}

	public override void _Process(double delta)
	{
	}

	public void StartButton_Pressed() {
		EmitSignal(SignalName.SongListOpened);
	}

	public void InfoButton_Pressed() {
		GetTree().ChangeSceneToFile("res://scenes/InfoPage.tscn");
	}

	public void ConvertButton_Pressed() {
		GetTree().ChangeSceneToFile("res://scenes/ConvertMenu.tscn");
	}

	public void ReloadButton_Pressed() {
		SongFileManager.GetSongFileList(GD.Print, true);
	}

	public void QuitButton_Pressed() {
		EmitSignal(SignalName.Closed);
	}
}
