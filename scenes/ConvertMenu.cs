using Godot;
using murph9.TabPlayer.Songs.Convert;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class ConvertMenu : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void GoButton_Pressed() {
		var infoLabel = GetNode<Label>("VBoxContainer/InfoLabel");
		infoLabel.Text = "";
		
		var a = GetNode<FileDialog>("FileDialog");
		var windowSize = GetWindow().Size;
		a.Size = new Vector2I((int)Math.Round(windowSize.X*0.8f), (int)Math.Round(windowSize.Y*0.8f));
		a.PopupCentered();
		a.Show();
	}

	public async void Dir_Selected(string dir) => await ConvertFiles(Directory.GetFiles(dir, "*.psarc"));
	public async void File_Selected(string path) => await ConvertFiles(new string[] {path});
	public async void Files_Selected(String[] paths) => await ConvertFiles(paths);

	private async Task ConvertFiles(string[] files) {
		var infoLabel = GetNode<Label>("VBoxContainer/InfoLabel");

		files = files.Where(x => x.EndsWith(".psarc")).ToArray();
		if (!files.Any()) {
			infoLabel.Text = "No valid .psarc files found";
			return;
		}
		
		foreach (var psarc in files) {
			try {
				// TODO config (maybe from the page this time?)
				await SongConvertManager.ConvertFile(SongConvertManager.SongType.Psarc, psarc, false, false, (string str) => {infoLabel.Text = str;});
			} catch (Exception e) {
				GD.Print(e, "ogg file not converted: " + psarc);
				infoLabel.Text = "Failed to convert psarc file: " + psarc;
				Thread.Sleep(300); // so you can read it i guess
			}
		}
		infoLabel.Text = $"Completed {files.Length} file(s)";
	}

	public void BackButton_Pressed() {
		GetTree().ChangeSceneToFile("res://StartMenu.tscn");
	}
}
