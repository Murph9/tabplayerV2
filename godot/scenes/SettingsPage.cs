using Godot;
using murph9.TabPlayer.scenes.Services;
using System.Linq;
using System;
using System.Collections.Generic;

namespace murph9.TabPlayer.scenes;

public partial class SettingsPage : CenterContainer
{
	[Signal]
	public delegate void ClosedEventHandler();
	
	private Settings _settings;

	public override void _Ready() {
		_settings = SettingsService.Settings();

		LayoutMode = 3;
		AnchorsPreset = 15;

		var vboxContainer = new VBoxContainer() {
		};
		AddChild(vboxContainer);
		
		var hBoxContainer = new HBoxContainer();
		hBoxContainer.AddChild(new Label() {
			Text = "Settings         ", // its for cheap spacing
			LabelSettings = new LabelSettings() {
				FontSize = 24
			}
		});
		var exitButton = new Button() {
			Text = "Save and Close"
		};
		exitButton.Pressed += () => {
			EmitSignal(SignalName.Closed);
		};
		hBoxContainer.AddChild(exitButton);
		
		vboxContainer.AddChild(hBoxContainer);
		vboxContainer.AddChild(new Label() {
			Text = "Set String Colours (low to high):"
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
}
