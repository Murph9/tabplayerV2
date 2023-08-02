using Godot;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
using System;
using System.IO;

public partial class SongScene : Node
{
	private SongState _state;
	private AudioController _audioController;

	public void _init(SongState state) {
		_state = state;

		_audioController = new AudioController(_state.AudioStream);
	}

	public void Pause() {
		_audioController.Pause();
		
		var popup = GetNode<PopupPanel>("PopupPanel");
		popup.PopupCentered();
	}

	public void Resume() {
		_audioController.Play();
		var popup = GetNode<PopupPanel>("PopupPanel");
		if (popup.Visible)
			popup.Hide();
	}

	public void Quit() {
		_audioController?.Stop();
		_audioController?.Dispose();
		GetTree().ChangeSceneToFile("res://scenes/SongList.tscn");
		
		this.Pause(); // prevent errors in final update frame (or free audio controller later?)
		QueueFree();
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("song_pause"))
		{
			var popup = GetNode<PopupPanel>("PopupPanel");
			if (!popup.Visible) {
				Pause();
			}
		}

		if (@event.IsActionPressed("song_skip_forward_10")) {
			Skip10Sec();
		} else if (@event.IsActionPressed("song_skip_backward_10")) {
			Back10Sec();
		} else if (@event.IsActionPressed("song_skip_to_first")) {
			SkipToFirst();
		} else if (@event.IsActionPressed("song_restart")) {
			RestartSong();
		}
	}

	public void Skip10Sec() => _audioController.Seek(_audioController.SongPosition + 10);
	public void Back10Sec() => _audioController.Seek(_audioController.SongPosition - 10);
	public void SkipToFirst() {
		var first = _state.Instrument.Notes.First();
		_audioController.Seek(first.Time - 1.5f);
	}
	public void RestartSong() => _audioController.Seek(0);


	public override void _Ready()
	{
		var info = _state.SongInfo;
		setUILabels(info);

		_audioController.Play();
		
		var guitarChartScene = GD.Load<CSharpScript>("res://scenes/song/GuitarChart.cs").New().As<GuitarChart>();
		guitarChartScene._init(_state, _audioController);
		AddChild(guitarChartScene);

		var noteGraphScene = GD.Load<CSharpScript>("res://scenes/song/NoteGraph.cs").New().As<NoteGraph>();
		noteGraphScene._init(_state, _audioController);
		AddChild(noteGraphScene);

		try {
			var songScene = GD.Load<CSharpScript>("res://scenes/song/SongChart.cs").New().As<SongChart>();
			songScene._init(_state.Instrument);
			AddChild(songScene);
		} catch (Exception e) {
			GD.Print(e);
		}
	}

	public override void _Process(double delta)
	{
		var nextNote = NextNoteBlock();

		var noteText = (nextNote == null) ? "No note" : "Next: " + Math.Round(nextNote.Time, 3) + " in " + Math.Round(nextNote.Time - _audioController.SongPosition, 1);

		var debugText = @$"{noteText}
{Engine.GetFramesPerSecond()}fps | {(delta*1000).ToString("000.0")}ms
{_audioController.SongPosition.ToMinSec(true)}
";
		GetNode<Label>("RunningDetailsLabel").Text = debugText;

		UpdateLyrics(GetNode<RichTextLabel>("LyricsLabel"));
	}

	private void setUILabels(SongInfo info) {
		var infoLabel = GetNode<Label>("SongInfoLabel");
		infoLabel.Text = $"{info.Metadata.Name} ({info.Metadata.Year})\n{info.Metadata.Artist}";

		var detailsLabel = GetNode<Label>("SongDetailsLabel");
		var guitarTuning = "Tuning: " + _state.Instrument.CalcFullTuningStr();
		var chordCount = _state.Instrument.Notes.Where(x => x.Notes.Count() > 1).Count();
		var singleNoteCount = _state.Instrument.Notes.Count() - chordCount;
		detailsLabel.Text = $@"Playing: {_state.Instrument.Name}
Notes: {singleNoteCount}
Chords: {chordCount}
{guitarTuning}
First note @ {_state.Instrument.Notes.First().Time.ToMinSec()}
Last note @ {_state.Instrument.Notes.Last().Time.ToMinSec()}";
	}

	private NoteBlock NextNoteBlock() {
		var songPos = _audioController.SongPosition;
		foreach (var b in _state.Instrument.Notes) {
			if (b.Time > songPos)
				return b;
		}
		return null;
	}

	private void UpdateLyrics(RichTextLabel label) {
		label.Clear();
		label.PushFontSize(20);
		
		//TODO don't move to the next lyrics until the next set starts
		var songPos = _audioController.SongPosition;
		var curList = GetCurLines(_state, songPos);
		if (curList.Length < 1) {
			label.Text = null;
			return;
		}

		if (curList[0] != null) {
			var (partA, partB) = curList[0].GetParts(songPos);
			label.PushColor(Colors.Red);
			label.AddText(partA);
			label.PushColor(Colors.Yellow);
			label.AddText(partB);
		}

		if (curList.Length > 1) {
			label.AddText("\n" + curList[1]?.ToString());
		}
	}

	private LyricLine[] GetCurLines(SongState state, double songPos) {
		var lyrics = state.SongInfo.Lyrics;
		if (lyrics == null || !lyrics.Lines.Any())
			return new LyricLine[0];

		if (lyrics.Lines.First().StartTime > songPos) {
			// its the first one time
			return lyrics.Lines.Take(2).ToArray();
		}

		foreach (var s in lyrics.Lines) {
			if (s.EndTime < songPos)
				continue;
			return lyrics.Lines.SkipWhile(x => x != s).Take(2).ToArray();
		}

		return new LyricLine[0];
	}
}
