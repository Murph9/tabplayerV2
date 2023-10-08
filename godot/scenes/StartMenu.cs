using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.Songs;
using System.Threading.Tasks;

namespace murph9.TabPlayer.scenes;

public partial class StartMenu : Node, ITransistionScene
{
	[Signal]
	public delegate void ClosedEventHandler();
	[Signal]
	public delegate void SongPickOpenedEventHandler();
	[Signal]
	public delegate void ConvertMenuOpenedEventHandler();
	[Signal]
	public delegate void InfoMenuOpenedEventHandler();
	[Signal]
	public delegate void SongListFileChangedEventHandler();
	[Signal]
	public delegate void SettingsOpenedEventHandler();


	private string _progressText;
	private TweenHelper _tween;

	public override void _Ready() {
		var obj = GetNode<VBoxContainer>("VBoxContainer");
		var screen = GetViewport().GetVisibleRect().Size;
		obj.Position = new Vector2(obj.Position.X, screen.Y - obj.Size.Y - 80);
		_tween = new TweenHelper(GetTree(), GetNode<VBoxContainer>("VBoxContainer"), "position", obj.Position, new Vector2(80, obj.Position.Y));

		AnimateIn();
	}

	public override void _Process(double delta) {
		GetNode<Label>("ReloadProgressLabel").Text = _progressText;
	}

	private void StartButton_Pressed() {
		AnimateOut();
		EmitSignal(SignalName.SongPickOpened);
	}

	private void InfoButton_Pressed() {
		AnimateOut();
		EmitSignal(SignalName.InfoMenuOpened);
	}

	private void ConvertButton_Pressed() {
		AnimateOut();
		EmitSignal(SignalName.ConvertMenuOpened);
	}

	private void ReloadButton_Pressed() {
		var startButton = GetNode<Button>("%StartButton");
		startButton.Disabled = true;
		var convertButton = GetNode<Button>("%ConvertButton");
		convertButton.Disabled = true;
		var reloadButton = GetNode<Button>("%ReloadButton");
		reloadButton.Disabled = true;

		Task.Run(() => {
			SongFileManager.UpdateSongList((str) => _progressText = str);

			CallDeferred("emit_signal", SignalName.SongListFileChanged);
			reloadButton.SetDeferred("disabled", false);
			convertButton.SetDeferred("disabled", false);
			startButton.SetDeferred("disabled", false);
		});
	}

	private void QuitButton_Pressed() {
		EmitSignal(SignalName.Closed);
	}

	private void SettingsButton_Pressed() {
		AnimateOut();
		EmitSignal(SignalName.SettingsOpened);
	}

	public void AnimateIn() => _tween.ToFinal();
	public void AnimateOut() => _tween.ToInitial();
}
