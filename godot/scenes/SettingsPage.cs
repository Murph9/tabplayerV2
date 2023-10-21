using Godot;
using murph9.TabPlayer.scenes.Services;
using System.Linq;
using System.Collections.Generic;

namespace murph9.TabPlayer.scenes;

public partial class SettingsPage : VBoxContainer, ITransistionScene
{
	[Signal]
	public delegate void ClosedEventHandler();
	
	private Settings _settings;
	private TweenHelper _tween;

	public override void _Ready() {
		_settings = SettingsService.Settings();
	
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
		
		AddChild(hBoxContainer);
		AddChild(new Label() {
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
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
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
			AddChild(box);
		}

		AddChild(new Label() {
			Text = "Other Settings"
		});
		var otherBox = new VBoxContainer() {
			Name = "OtherVBoxContainer"
		};
		var lowIsLowCheck = new CheckBox() {
			Text = "Low String at the bottom",
			ButtonPressed = _settings.LowStringIsLow
		};
		lowIsLowCheck.Pressed += () => {
			_settings = _settings with { LowStringIsLow = !_settings.LowStringIsLow };
			SettingsService.UpdateSettings(_settings);
		};
		otherBox.AddChild(lowIsLowCheck);
		AddChild(otherBox);

		var cameraBox = new VBoxContainer() {
			Name = "CameraVBoxContainer"
		};
		var cameraAimLabel = new Label() { Text = "Change Camera Aim Speed: " + _settings.CameraAimSpeed };
		cameraBox.AddChild(cameraAimLabel);
		var cameraButtonBox = new HBoxContainer();
		var cameraAimUpButton = new Button() {
			Text = "Increase"
		};
		cameraAimUpButton.Pressed += () => SetCameraAimSpeed(cameraAimLabel, 1);
		cameraButtonBox.AddChild(cameraAimUpButton);
		var cameraAimDownButton = new Button() {
			Text = "Decrease"
		};
		cameraAimDownButton.Pressed += () => SetCameraAimSpeed(cameraAimLabel, -1);
		cameraButtonBox.AddChild(cameraAimDownButton);
		cameraBox.AddChild(cameraButtonBox);
		
		AddChild(cameraBox);

		_tween = new TweenHelper(GetTree(), this, "position", new Vector2(-500, Position.Y), Position);
		Position = new Vector2(-500, Position.Y);
	}

	private void SetCameraAimSpeed(Label l, int dx) {
		var newSpeed = _settings.CameraAimSpeed + dx;
		if (newSpeed > 30 || newSpeed < 1)
			return;
		
		_settings = _settings with { CameraAimSpeed = newSpeed };
		l.Text = "Change Camera Aim Speed: " + _settings.CameraAimSpeed;
		SettingsService.UpdateSettings(_settings);
	}

	public override void _Process(double delta) { }

    public void AnimateIn() => _tween.ToFinal();

    public void AnimateOut() => _tween.ToInitial();
}
