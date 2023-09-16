using Godot;
using murph9.TabPlayer.Songs.Convert;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace murph9.TabPlayer.scenes;

public partial class ConvertMenu : Node2D
{
	private bool _recreate;

	[Signal]
	public delegate void ClosedEventHandler();
	
	public override void _Ready() { }

	public override void _Process(double delta) { }

	public void Recreate_Toggled(bool state) => _recreate = state;

	private void ChoseButton_Pressed() {
		var infoLabel = GetNode<Label>("VBoxContainer/InfoLabel");
		infoLabel.Text = "";
		
		var a = GetNode<FileDialog>("FileDialog");
		var windowSize = GetWindow().Size;
		a.Size = new Vector2I((int)Math.Round(windowSize.X*0.8f), (int)Math.Round(windowSize.Y*0.8f));
		a.PopupCentered();
	}

	private async void FromDownloadsButton_Pressed() {
		var infoLabel = GetNode<Label>("VBoxContainer/InfoLabel");
		var downloadsFolder = OS.GetSystemDir(OS.SystemDir.Downloads);

		infoLabel.Text = "Downloading from " + downloadsFolder;
		await ConvertFiles(Directory.GetFiles(downloadsFolder, "*.psarc"));
	}

	public async void Dir_Selected(string dir) => await ConvertFiles(Directory.GetFiles(dir, "*.psarc"));
	public async void File_Selected(string path) => await ConvertFiles(new string[] {path});
	public async void Files_Selected(string[] paths) => await ConvertFiles(paths);

	private async Task ConvertFiles(string[] files) {
		var infoLabel = GetNode<Label>("VBoxContainer/InfoLabel");

		files = files.Where(x => x.EndsWith(".psarc")).ToArray();
		if (!files.Any()) {
			infoLabel.Text = "No valid .psarc files found";
			return;
		}
		
		var completed = new List<string>();
		var failed = new List<string>();
		foreach (var psarc in files) {
			try {
				var success = await SongConvertManager.ConvertFile(SongConvertManager.SongType.Psarc, psarc, _recreate, (string str) => {infoLabel.Text = str;});
				if (success)
					completed.Add(psarc);
				else
					failed.Add(psarc);
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
		EmitSignal(SignalName.Closed);
	}
}
