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
		vboxContainer.AddChild(new Label() {
			Text = "Select Your String Colours (high to low):"
		});

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
				_settings = _settings with { StringColours = colours };
				SettingsService.UpdateSettings(_settings);
			};
			box.AddChild(picker);
			vboxContainer.AddChild(box);
		}

		vboxContainer.AddChild(new Label() {
			Text = "Other Settings"
		});
		var otherBox = new HBoxContainer() {
			Name = "OtherHBoxContainer"
		};
		var lowIsLowCheck = new CheckBox() {
			Text = "Low String at the bottom: ",
			ButtonPressed = _settings.LowStringIsLow
		};
		lowIsLowCheck.Pressed += () => {
			_settings = _settings with { LowStringIsLow = !_settings.LowStringIsLow };
			SettingsService.UpdateSettings(_settings);
		};
		otherBox.AddChild(lowIsLowCheck);
		vboxContainer.AddChild(otherBox);
	}

	public override void _Process(double delta) { }

	public void BackButton_Pressed() {
		EmitSignal(SignalName.Closed);
	}
}
