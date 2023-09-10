using Godot;
using murph9.TabPlayer.Songs;

namespace murph9.TabPlayer.scenes;

public partial class InfoPage : Node
{
	[Signal]
	public delegate void ClosedEventHandler();
	
	public override void _Ready() { }

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
}
