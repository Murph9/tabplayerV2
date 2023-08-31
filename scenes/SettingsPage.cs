using Godot;
using murph9.TabPlayer.scenes.Services;
using System.Linq;
using System;

namespace murph9.TabPlayer.scenes;

public partial class SettingsPage : Node
{
	[Signal]
	public delegate void ClosedEventHandler();
	
	private Settings _settings;

	public override void _Ready() {
		_settings = SettingsService.Settings();

		var vboxContainer = GetNode<VBoxContainer>("StringVBoxContainer");

		foreach (var i in Enumerable.Range(0, 6)) {
			var stringChar = DisplayConst.StringLabels[i];
			var box = new HBoxContainer() {
				Name = stringChar + "HBoxContainer"
			};
			box.AddChild(new Label() {
				Text = stringChar + " String"
			});
			var picker = new ColorPickerButton() {
				LayoutMode = 2,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				Color = _settings.StringColours[i],
				EditAlpha = false
			};
			picker.PopupClosed += () => {
				var colours = new List<Color>(_settings.StringColours)
				{
					[i] = picker.Color
				};
				SettingsService.UpdateSettings(_settings with { StringColours = colours });
			};
			box.AddChild(picker);
			vboxContainer.AddChild(box);
		}
	}

	public override void _Process(double delta) { }

	public void BackButton_Pressed() {
		EmitSignal(SignalName.Closed);
	}
}
