using Godot;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
using Newtonsoft.Json;
using System;

public partial class SongList : Control
{
	private SongFile[] _songList;
	private SongFile _selectedSong;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var songList = SongFileManager.GetSongFileList((str) => {});
		_songList = songList.Data.ToArray();

		// TODO how to listen to an event?
		var list = GetNode<ItemList>("VBoxContainer/ScrollContainer/ItemList");
		foreach (var a in _songList) {
			list.AddItem(a.SongName + " | " + a.Artist);
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

	private void SelectedItem_LoadSong(int index) {
		var selectedInstrument = _selectedSong.Instruments.ToArray()[index].Name;
		var songState = SongLoader.Load(new DirectoryInfo(Path.Combine(SongFileManager.SONG_FOLDER, _selectedSong.FolderName)), selectedInstrument);

		PackedScene packedScene = ResourceLoader.Load<PackedScene>("res://scenes/SongScene.tscn");
		var scene = packedScene.Instantiate<SongScene>();
		scene._init(songState);
		
		GetTree().Root.AddChild(scene);
		GetTree().Root.RemoveChild(this);
	}
}
