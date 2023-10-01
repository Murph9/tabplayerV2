using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
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
		_songDisplay.SongSelected += (string s) =>
        {
            SongFileList songList = SongFileManager.GetSongFileList();
			ChooseSong(songList.Data.First(x => s == x.FolderName));
		};
		
		_songList = GD.Load<PackedScene>("res://scenes/SongList.tscn").Instantiate<SongList>();
		_songList.SetDisplay(_songDisplay);

		_songList.SongSelected += _songDisplay.SongChanged;
	}

	public override void _Ready() {
		_songList.LayoutMode = 3;
		
		var vbox = GetNode<VBoxContainer>("VBoxContainer");
		vbox.AddChild(_songList);
	}

	public override void _Process(double delta)
	{
	}

	private void ChooseSong(SongFile song) {
		GD.Print("Selected: " + song.SongName);
		
		var panel = GetNode<PopupPanel>("PopupPanel");
		panel.GetChildren().ToList().ForEach(panel.RemoveChild);

		var layout = new VBoxContainer();
		panel.AddChild(layout);

		layout.AddChild(new Label() {
			Text = $"Select an instrument for song:\n{song.SongName}\n{song.Artist} ({song.Year})\n{song.Length.ToMinSec()}"
		});

		foreach (var i in song.Instruments) {
			var b = new Button() {
				Text = $"{i.Name} Tuning: {Instrument.CalcTuningName(i.Tuning, i.CapoFret)} | Notes: {i.NoteCount} @ {i.GetNoteDensity(song).ToFixedPlaces(2, false)}"
			};
			b.Pressed += () => SelectedItem_LoadSong(song, i.Name);
			layout.AddChild(b);
		}

		panel.PopupCentered();
	}

	private void SelectedItem_LoadSong(SongFile song, string instrument) {
		EmitSignal(SignalName.OpenedSong, song.FolderName, instrument);
	}

	public void Back() {
		EmitSignal(SignalName.Closed);
	}
}
