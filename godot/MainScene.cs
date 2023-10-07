using Godot;
using murph9.TabPlayer.scenes;
using murph9.TabPlayer.Songs;

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
			convertMenu.Closed += () => {
				RemoveChild(convertMenu);
				_startMenu.Show();
			};
			AddChild(convertMenu);
		};
		_startMenu.InfoMenuOpened += () => {
			var infoMenu = GD.Load<PackedScene>("res://scenes/InfoPage.tscn").Instantiate<InfoPage>();
			infoMenu.Closed += () => {
				RemoveChild(infoMenu);
				_startMenu.Show();
			};
			AddChild(infoMenu);
        };
		_startMenu.SettingsOpened += () => {
			var settingsMenu = GD.Load<CSharpScript>("res://scenes/SettingsPage.cs").New().As<SettingsPage>();
			settingsMenu.Closed += () => {
				RemoveChild(settingsMenu);
				_startMenu.Show();
			};
			AddChild(settingsMenu);
        };
	}

	private void LoadSongPick() {
		_songPick = GD.Load<PackedScene>("res://scenes/SongPick.tscn").Instantiate<SongPick>();
		_songPick.Closed += () => {
			RemoveChild(_songPick);
			AddChild(_startMenu);
			_startMenu.Show();
		};
		_songPick.OpenedSong += (string folderName, string instrument) => {
			RemoveChild(_songPick);
			var packedScene = ResourceLoader.Load<PackedScene>("res://scenes/SongScene.tscn");
			var scene = packedScene.Instantiate<SongScene>();
			scene._init(SongLoader.Load(folderName, instrument));
			AddChild(scene);
			scene.Closed += () => {
				scene.QueueFree();
				AddChild(_songPick);
			};
		};
	}

	public override void _Process(double delta) {}
}
