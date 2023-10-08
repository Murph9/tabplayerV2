using Godot;
using murph9.TabPlayer.Songs;
using System.Linq;

namespace murph9.TabPlayer.scenes;

public partial class SongPick : Control
{
	[Signal]
	public delegate void ClosedEventHandler();
	[Signal]
	public delegate void OpenedSongEventHandler(string folder, string instrument);

	private SongList _songList;
	private SongDisplay _songDisplay;

	public SongPick() {
		_songDisplay = GD.Load<PackedScene>("res://scenes/SongDisplay.tscn").Instantiate<SongDisplay>();
		_songDisplay.SongSelected += (string folder, string instrument) =>
        {
            SongFileList songList = SongFileManager.GetSongFileList();
			ChooseSong(songList.Data.First(x => folder == x.FolderName), instrument);
		};
		
		_songList = GD.Load<PackedScene>("res://scenes/SongList.tscn").Instantiate<SongList>();
		_songList.SetDisplay(_songDisplay);

		_songList.SongSelected += _songDisplay.SongChanged;
	}

	public override void _Ready() {
		_songList.LayoutMode = 3;
		
		var vbox = GetNode<VBoxContainer>("MarginContainer/VBoxContainer");
		vbox.AddChild(_songList);
	}

	public override void _Process(double delta) { }

	private void ChooseSong(SongFile song, string instrument) {
		GD.Print($"Selected: {song.SongName} with path {instrument}");
		
		EmitSignal(SignalName.OpenedSong, song.FolderName, instrument);
	}

	public void Back() {
		EmitSignal(SignalName.Closed);
	}
}
