using Godot;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
using System;

public partial class SongList : Control
{
	private SongFile[] _songList;

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

	public void StartButton_Pressed() {
		PackedScene packedScene = ResourceLoader.Load<PackedScene>("res://scenes/SongScene.tscn");
		var scene = packedScene.Instantiate<SongScene>();
		scene.init(null, null); // TODO get from Table
		
		GetTree().Root.AddChild(scene);
		GetTree().Root.RemoveChild(this);
	}

	public void ItemActivated(int index) {
		GD.Print(_songList[index].SongName + " i:" + index);
	}
}
