using Godot;
using murph9.TabPlayer.scenes;
using murph9.TabPlayer.Songs;
using System.IO;

namespace murph9.TabPlayer;

public partial class MainScene : Node
{
	// The Scene manager of the game, this has no game in it right?

	private StartMenu _startMenu;
	private SongPick _songPick;

	public override void _Ready()
	{
		LoadStartMenu();
		LoadSongPick();
	}
	
	private void LoadStartMenu() {
		_startMenu = GD.Load<PackedScene>("res://scenes/StartMenu.tscn").Instantiate<StartMenu>();
		AddChild(_startMenu);

		_startMenu.Closed += () => {
			GD.Print("Start Menu closed");
			GetTree().Quit();
		};
		_startMenu.SongPickOpened += () => {
			RemoveChild(_startMenu);
			AddChild(_songPick);
		};
		_startMenu.SongListFileChanged += () => {
			var loaded = _songPick.IsVisibleInTree();
			if (loaded) {
				_songPick.QueueFree();
			}
			LoadSongPick();

			if (loaded) {
				AddChild(_songPick);
			}
		};
		_startMenu.ConvertMenuOpened += () => {
			var convertMenu = GD.Load<PackedScene>("res://scenes/ConvertMenu.tscn").Instantiate<ConvertMenu>();
			RemoveChild(_startMenu);
			convertMenu.Closed += () => {
				RemoveChild(convertMenu);
				AddChild(_startMenu);
			};
			AddChild(convertMenu);
		};
		_startMenu.InfoMenuOpened += () => {
			var infoMenu = GD.Load<PackedScene>("res://scenes/InfoPage.tscn").Instantiate<InfoPage>();
			RemoveChild(_startMenu);
			infoMenu.Closed += () => {
				RemoveChild(infoMenu);
				AddChild(_startMenu);
			};
			AddChild(infoMenu);
        };
		_startMenu.SettingsOpened += () => {
			var settingsMenu = GD.Load<CSharpScript>("res://scenes/SettingsPage.cs").New().As<SettingsPage>();
			RemoveChild(_startMenu);
			settingsMenu.Closed += () => {
				RemoveChild(settingsMenu);
				AddChild(_startMenu);
			};
			AddChild(settingsMenu);
        };
	}

	private void LoadSongPick() {
		_songPick = GD.Load<PackedScene>("res://scenes/SongPick.tscn").Instantiate<SongPick>();
		_songPick.Closed += () => {
			RemoveChild(_songPick);
			AddChild(_startMenu);
		};
		_songPick.OpenedSong += (string dir, string instrument) => {
			RemoveChild(_songPick);
			var packedScene = ResourceLoader.Load<PackedScene>("res://scenes/SongScene.tscn");
			var scene = packedScene.Instantiate<SongScene>();
			scene._init(SongLoader.Load(new DirectoryInfo(dir), instrument));
			AddChild(scene);
			scene.Closed += () => {
				scene.QueueFree();
				AddChild(_songPick);
			};
		};
	}

	public override void _Process(double delta) {}
}
