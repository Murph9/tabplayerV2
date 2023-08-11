using Godot;
using murph9.TabPlayer.Songs.Convert;
using System;
using System.IO;
using System.Linq;
using System.Threading;
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

	public void GoButton_Pressed() {
		var infoLabel = GetNode<Label>("VBoxContainer/InfoLabel");
		infoLabel.Text = "";
		
		var a = GetNode<FileDialog>("FileDialog");
		var windowSize = GetWindow().Size;
		a.Size = new Vector2I((int)Math.Round(windowSize.X*0.8f), (int)Math.Round(windowSize.Y*0.8f));
		a.PopupCentered();
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
		
		var completed = new List<string>();
		foreach (var psarc in files) {
			try {
				var success = await SongConvertManager.ConvertFile(SongConvertManager.SongType.Psarc, psarc, _recreate, (string str) => {infoLabel.Text = str;});
				if (success)
					completed.Add(psarc);
			} catch (Exception e) {
				GD.Print(e, "ogg file not converted: " + psarc);
				infoLabel.Text = "Failed to convert psarc file: " + psarc;
				Thread.Sleep(300); // so you can read it i guess
			}
		}
		if (completed.Count < 1)
			infoLabel.Text = $"Completed none :(";
		else
			infoLabel.Text = $"Completed: {String.Join('\n', completed)}";
	}

	public void BackButton_Pressed() {
		EmitSignal(SignalName.Closed);
	}
}
