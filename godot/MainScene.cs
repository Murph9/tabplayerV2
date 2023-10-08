using Godot;
using murph9.TabPlayer.scenes;
using murph9.TabPlayer.Songs;

namespace murph9.TabPlayer;

public partial class MainScene : Node
{
	// The Scene manager of the game, this has no game in it right?

	private StartMenu _startMenu;
	private SongPick _songPick;
	private ConvertMenu _convertMenu;
	private InfoPage _infoPage;
	private SettingsPage _settingsPage;

	public override void _Ready()
	{
		LoadMenus();
		LoadSongPick();
	}
	
	private void LoadMenus() {
		LoadStartMenu();

		_convertMenu = GD.Load<PackedScene>("res://scenes/ConvertMenu.tscn").Instantiate<ConvertMenu>();
		AddChild(_convertMenu);
		_convertMenu.Closed += () => {
			_convertMenu.AnimateOut();
			_startMenu.AnimateIn();
		};

		_infoPage = GD.Load<PackedScene>("res://scenes/InfoPage.tscn").Instantiate<InfoPage>();
		AddChild(_infoPage);
		_infoPage.Closed += () => {
			_infoPage.AnimateOut();
			_startMenu.AnimateIn();
		};

		_settingsPage = GD.Load<CSharpScript>("res://scenes/SettingsPage.cs").New().As<SettingsPage>();
		AddChild(_settingsPage);
		_settingsPage.Closed += () => {
			_settingsPage.AnimateOut();
			_startMenu.AnimateIn();
		};
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
			RemoveChild(_convertMenu);
			RemoveChild(_settingsPage);
			RemoveChild(_infoPage);
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
			_convertMenu.AnimateIn();
			_startMenu.AnimateOut();
		};
		_startMenu.InfoMenuOpened += () => {
			_infoPage.AnimateIn();
			_startMenu.AnimateOut();
        };
		_startMenu.SettingsOpened += () => {
			_settingsPage.AnimateIn();
			_startMenu.AnimateOut();
        };
	}

	private void LoadSongPick() {
		_songPick = GD.Load<PackedScene>("res://scenes/SongPick.tscn").Instantiate<SongPick>();
		_songPick.Closed += () => {
			RemoveChild(_songPick);

			AddChild(_startMenu);
			AddChild(_convertMenu);
			AddChild(_settingsPage);
			AddChild(_infoPage);
			_startMenu.AnimateIn();
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
