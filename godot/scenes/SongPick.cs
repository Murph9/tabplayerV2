using Godot;
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

	private SongFile _tempSongForConfirmDialog;
	private string _tempInstrumentForConfirmDialog;

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
		
		if (string.IsNullOrEmpty(_songList.TuningFilter)) {
			EmitSignal(SignalName.OpenedSong, song.FolderName, instrument);
			return;
		}

		var pickedInstrument = song.Instruments.First(x => x.Name == instrument);
		if (Instrument.CalcTuningName(pickedInstrument.Tuning) != _songList.TuningFilter) {
			// show a dialog that says that the instrument's tuning isn't the same as the filter instead
			var dialog = GetNode<ConfirmationDialog>("TuningConfirmationDialog");
			dialog.DialogText = $"Instrument tuning ({Instrument.CalcTuningName(pickedInstrument.Tuning)}) is different to song filter ({_songList.TuningFilter})\nAre you sure?";
			dialog.Show();
			_tempSongForConfirmDialog = song;
			_tempInstrumentForConfirmDialog = instrument;
		} else {
			EmitSignal(SignalName.OpenedSong, song.FolderName, instrument);
		}
	}

	public void Back() {
		EmitSignal(SignalName.Closed);
	}

	private void ConfirmedInstrumentTuningIsDiff() {
		EmitSignal(SignalName.OpenedSong, _tempSongForConfirmDialog.FolderName, _tempInstrumentForConfirmDialog);
	}
}
