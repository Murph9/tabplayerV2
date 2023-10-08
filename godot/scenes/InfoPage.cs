using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.Songs;

namespace murph9.TabPlayer.scenes;

public partial class InfoPage : VBoxContainer, ITransistionScene
{
	[Signal]
	public delegate void ClosedEventHandler();

	private TweenHelper _tween;
	
	public override void _Ready() {
		_tween = new TweenHelper(GetTree(), this, "position", new Vector2(-500, Position.Y), Position);
		Position = new Vector2(-500, Position.Y);
	}

	public override void _Process(double delta) { }

	private void BackButton_Pressed() {
		EmitSignal(SignalName.Closed);
	}

	private void ProjectSourceButton_Pressed() {
		OS.ShellOpen("https://github.com/Murph9/tabplayerV2");
	}

	private void OpenConfigFolder_Pressed() {
		OS.ShellOpen("file://" + SongFileManager.SONG_FOLDER);
	}

    public void AnimateIn() => _tween.ToFinal();
    public void AnimateOut() => _tween.ToInitial();
}
