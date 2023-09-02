using Godot;
using murph9.TabPlayer.Songs;
using System;

namespace murph9.TabPlayer.scenes;

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
	[Signal]
	public delegate void SongListChangedEventHandler();
	[Signal]
	public delegate void SettingsOpenedEventHandler();

	private string _progressText;

	public override void _Ready() { }

	public override void _Process(double delta) {
		GetNode<Label>("ReloadProgressLabel").Text = _progressText;
	}

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
		var startButton = GetNode<Button>("%StartButton");
		startButton.Disabled = true;
		var convertButton = GetNode<Button>("%ConvertButton");
		convertButton.Disabled = true;
		var reloadButton = GetNode<Button>("%ReloadButton");
		reloadButton.Disabled = true;

		Task.Run(() => {
			SongFileManager.UpdateSongList((str) => _progressText = str);

			CallDeferred("emit_signal", SignalName.SongListChanged);
			reloadButton.SetDeferred("disabled", false);
			convertButton.SetDeferred("disabled", false);
			startButton.SetDeferred("disabled", false);
		});
	}

	public void QuitButton_Pressed() {
		EmitSignal(SignalName.Closed);
	}

	public void SettingsButton_Pressed() {
		EmitSignal(SignalName.SettingsOpened);
	}
}
