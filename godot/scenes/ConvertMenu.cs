using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Convert;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace murph9.TabPlayer.scenes;

public partial class ConvertMenu : Control, ITransistionScene
{
	[Signal]
	public delegate void ClosedEventHandler();
	
	private TweenHelper _tween;
	
	public override void _Ready() {
		var initialPos = new Vector2(-Size.X, Position.Y);
		_tween = new TweenHelper(GetTree(), this, "position", initialPos, new Vector2(Position.X, Position.Y));
		Position = initialPos;
	}

	public override void _Process(double delta) { }

	private void ChoseButton_Pressed() {
		var infoLabel = GetNode<Label>("InfoLabel");
		infoLabel.Text = "";
		
		var a = GetNode<FileDialog>("FileDialog");
		var windowSize = GetWindow().Size;
		a.Size = new Vector2I((int)Math.Round(windowSize.X*0.8f), (int)Math.Round(windowSize.Y*0.8f));
		a.PopupCentered();
	}

	private async void FromDownloadsButton_Pressed() {
		var infoLabel = GetNode<Label>("InfoLabel");
		var downloadsFolder = OS.GetSystemDir(OS.SystemDir.Downloads);

		infoLabel.Text = "Downloading from " + downloadsFolder;
		await ConvertFiles(Directory.GetFiles(downloadsFolder, "*.psarc"));
	}

	public async void Dir_Selected(string dir) => await ConvertFiles(Directory.GetFiles(dir, "*.psarc"));
	public async void File_Selected(string path) => await ConvertFiles(new string[] {path});
	public async void Files_Selected(string[] paths) => await ConvertFiles(paths);

	private async Task ConvertFiles(string[] files) {
		var infoLabel = GetNode<Label>("InfoLabel");

		files = files.Where(x => x.EndsWith(".psarc")).ToArray();
		if (!files.Any()) {
			infoLabel.Text = "No valid .psarc files found";
			return;
		}
		
		var recreate = GetNode<CheckButton>("RecreateRadio").ButtonPressed;
		var copySource = GetNode<CheckButton>("CopySourceRadio").ButtonPressed;

		var completed = new List<string>();
		var failed = new List<string>();
		foreach (var psarc in files) {
			try {
				var outputFolder = await SongConvertManager.ConvertFile(SongConvertManager.SongType.Psarc, psarc, recreate, copySource, (string str) => {infoLabel.Text = str;});
				if (outputFolder != null) {
					var success = SongFileManager.AddSingleSong(outputFolder.FullName);
					if (success)
						completed.Add(psarc);
					else
						failed.Add(psarc);
				} else {
					failed.Add(psarc);
				}
			} catch (Exception e) {
				GD.Print(e, "ogg file not converted: " + psarc);
				infoLabel.Text = "Failed to convert psarc file: " + psarc;
				failed.Add(psarc);
			}
		}
		infoLabel.Text = $"Completed: {completed.Count}, failed: {failed.Count}";
		if (failed.Count > 0) {
			foreach (var fail in failed) {
				infoLabel.Text += "\n Failed: " + fail;
			}
		}
	}

	public void BackButton_Pressed() {
		_tween.ToInitial();
		EmitSignal(SignalName.Closed);
	}

    public void AnimateIn() => _tween.ToFinal();

    public void AnimateOut() => _tween.ToInitial();
}
