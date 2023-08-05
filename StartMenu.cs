using Godot;
using murph9.TabPlayer.Songs;
using System;

namespace murph9.TabPlayer;

public partial class StartMenu : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void StartButton_Pressed() {
		GetTree().ChangeSceneToFile("res://scenes/SongList.tscn");
	}

	public void InfoButton_Pressed() {
		GetTree().ChangeSceneToFile("res://scenes/InfoPage.tscn");
	}

	public void ConvertButton_Pressed() {
		GetTree().ChangeSceneToFile("res://scenes/ConvertMenu.tscn");
	}

	public void ReloadButton_Pressed() {
		SongFileManager.GetSongFileList((str) => GD.Print(str), true);
	}

	public void QuitButton_Pressed() {
		GetTree().Quit();
	}
}
