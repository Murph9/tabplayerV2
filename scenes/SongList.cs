using Godot;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
using Newtonsoft.Json;
using System;

public partial class SongList : Control
{
	private SongFile[] _songList;
	private SongFile _selectedSong;

	// TODO don't unload this

	public override void _Ready()
	{
		var songList = SongFileManager.GetSongFileList((str) => {});
		_songList = songList.Data.ToArray();

		var grid = GetNode<GridContainer>("VBoxContainer/ScrollContainer/GridContainer");
		grid.Columns = 11;
		grid.AddChild(new Label());
		grid.AddChild(new Label() { Text = "Song Name" });
		grid.AddChild(new Label() { Text = "Artist" });
		grid.AddChild(new Label() { Text = "Album" });
		grid.AddChild(new Label() { Text = "Year" });
		grid.AddChild(new Label() { Text = "Length" });
		grid.AddChild(new Label() { Text = "Parts" });
		grid.AddChild(new Label() { Text = "Instr" });
		grid.AddChild(new Label() { Text = "Tuning" });
		grid.AddChild(new Label() { Text = "Diff" });
		grid.AddChild(new Label() { Text = "Notes" });
		
		foreach (var (song, index) in _songList.Select((s, i) => (s, i))) {
			var b = new Button() {
				Text = "▶"
			};
			b.Pressed += () => ItemActivated(index);
			grid.AddChild(b);
			grid.AddChild(new Label() {
				Text = song.SongName.FixedWidthString(30)
			});
			grid.AddChild(new Label() {
				Text = song.Artist.FixedWidthString(24)
			});
			grid.AddChild(new Label() {
				Text = song.Album.FixedWidthString(20)
			});
			grid.AddChild(new Label() {
				Text = song.Year.ToString()
			});
			grid.AddChild(new Label() {
				Text = song.Length.ToMinSec()
			});
			grid.AddChild(new Label() {
				Text = song.GetInstrumentChars()
			});
			grid.AddChild(new Label() {
				Text = song.GetMainInstrument()?.Name
			});
			grid.AddChild(new Label() {
				Text = song.GetMainInstrument()?.Tuning
			});
			grid.AddChild(new Label() {
				Text = song.GetMainInstrument()?.GetNoteDensity(song).ToString()
			});
			grid.AddChild(new Label() {
				Text = song.GetMainInstrument()?.NoteCount.ToString()
			});
		}
	}

	public override void _Process(double delta)
	{
	}

	public void ItemActivated(int index) {
		_selectedSong = _songList[index];
		GD.Print(_selectedSong.SongName + " i:" + index);
		
		var menu = GetNode<PopupMenu>("PopupMenu");
		menu.Clear();
		foreach (var i in _selectedSong.Instruments) {
			menu.AddItem(i.Name + " | Tuning:" + i.Tuning + " | Note Count:" + i.NoteCount);
		}
		menu.Title = "Select the instrument to play in: " + _selectedSong.SongName;

		menu.PopupCentered();
	}

	public void Back() {
		GetTree().ChangeSceneToFile("res://StartMenu.tscn");
	}

	private void SelectedItem_LoadSong(int index) {
		var selectedInstrument = _selectedSong.Instruments.ToArray()[index].Name;
		var songState = SongLoader.Load(new DirectoryInfo(Path.Combine(SongFileManager.SONG_FOLDER, _selectedSong.FolderName)), selectedInstrument);

		PackedScene packedScene = ResourceLoader.Load<PackedScene>("res://scenes/SongScene.tscn");
		var scene = packedScene.Instantiate<SongScene>();
		scene._init(songState);
		
		GetTree().Root.AddChild(scene);
		QueueFree();
	}
}
