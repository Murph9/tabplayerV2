using Godot;

namespace murph9.TabPlayer.scenes;

public partial class InfoPage : Node
{
	[Signal]
	public delegate void ClosedEventHandler();
	
	public override void _Ready() { }

	public override void _Process(double delta) { }

	public void BackButton_Pressed() {
		EmitSignal(SignalName.Closed);
	}
}
