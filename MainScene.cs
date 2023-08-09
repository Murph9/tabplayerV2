using Godot;
using murph9.TabPlayer.scenes;
using murph9.TabPlayer.Songs;
using System;

namespace murph9.TabPlayer;

public partial class MainScene : Node
{
	// The Scene manager of the game, this has no game in it right?

	private StartMenu _startMenu;
	private SongList _songList;

	public override void _Ready()
	{
		_startMenu = GD.Load<PackedScene>("res://StartMenu.tscn").Instantiate<StartMenu>();
		AddChild(_startMenu);

		_startMenu.Closed += () => {
			GD.Print("Start Menu closed");
			GetTree().Quit();
		};
		_startMenu.SongListOpened += () => {
			RemoveChild(_startMenu);
			AddChild(_songList);
		};

		_songList = GD.Load<PackedScene>("res://scenes/SongList.tscn").Instantiate<SongList>();
		_songList.Closed += () => {
			RemoveChild(_songList);
			AddChild(_startMenu);
		};
		_songList.OpenedSong += (string dir, string instrument) => {
			RemoveChild(_songList);
			var packedScene = ResourceLoader.Load<PackedScene>("res://scenes/SongScene.tscn");
			var scene = packedScene.Instantiate<SongScene>();
			scene._init(SongLoader.Load(new DirectoryInfo(dir), instrument));
			AddChild(scene);
			scene.Closed += () => {
				scene.QueueFree();
				AddChild(_songList);
			};
		};
	}

	public override void _Process(double delta) {}
}
