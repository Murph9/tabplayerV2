using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.scenes.song;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
using System;
using System.IO;

namespace murph9.TabPlayer.scenes;

public interface IAudioStreamPosition {
	public bool SongPlaying { get; }
	public double GetSongPosition();
}

public partial class SongScene : Node, IAudioStreamPosition
{
	private SongState _state;
	private AudioStreamOggVorbis _audioStream;
	private AudioStreamPlayer _player;
	private double? _cachedSongPosition;

	public bool SongPlaying => _player?.Playing == true;

    [Signal]
	public delegate void ClosedEventHandler();

	public void _init(SongState state) {
		_state = state;
		_audioStream = AudioStreamOggVorbis.LoadFromBuffer(state.Audio);
	}

	public void SongFinished() {
		_player.Play();
		_player.Seek(0);
		_player.StreamPaused = true;
		Pause();
	}

	public void Pause() {
		_player.StreamPaused = true;
		
		var pause = GetNode<Window>("PauseWindow");
		var windowSize = GetWindow().Size;
		pause.Size = new Vector2I((int)Math.Round(windowSize.X*0.5f), (int)Math.Round(windowSize.Y*0.5f));
		pause.PopupCentered();
		pause.ChildControlsChanged();
	}

	public void Resume() {
		_player.StreamPaused = false;
		
		var pause = GetNode<Window>("PauseWindow");
		if (pause.Visible)
			pause.Hide();
	}

	public void Quit() {
		_player.Stop();
		Pause(); // prevent errors in final update frame
		
		EmitSignal(SignalName.Closed);
	}

	public void PauseWindow_Input(InputEvent @event) {
		_Input(@event);
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("song_pause"))
		{
			var pause = GetNode<Window>("PauseWindow");
			if (!pause.Visible) {
				Pause();
			} else {
				Resume();
			}
		}

		if (@event.IsActionPressed("song_skip_forward_10")) {
			Skip10Sec();
		} else if (@event.IsActionPressed("song_skip_backward_10")) {
			Back10Sec();
		} else if (@event.IsActionPressed("song_skip_to_next")) {
			SkipToNext();
		} else if (@event.IsActionPressed("song_restart")) {
			RestartSong();
		}
	}

	public void Skip10Sec() => _player.Seek((float)GetSongPosition() + 10f);
	public void Back10Sec() => _player.Seek((float)GetSongPosition() - 10f);
	public void SkipToNext() {
		var nextNote = NextNoteBlock();
		if (nextNote == null)
			return;
		_player.Seek(nextNote.Time - 1.5f);
	}
	public void RestartSong() => _player.Seek(0);

	public override void _Ready()
	{
		var info = _state.SongInfo;
		SetUILabels(info);

		_player = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		_player.Stream = _audioStream;
		_player.Play();
		
		var guitarChartScene = GD.Load<CSharpScript>("res://scenes/song/GuitarChart.cs").New().As<GuitarChart>();
		guitarChartScene._init(_state, this);
		AddChild(guitarChartScene);

		var noteGraphScene = GD.Load<CSharpScript>("res://scenes/song/NoteGraph.cs").New().As<NoteGraph>();
		noteGraphScene._init(_state, this);
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
		_cachedSongPosition = null;
		if (!_player.Playing)
			return;

		var nextNote = NextNoteBlock();

		var noteText = (nextNote == null) ? "No note" : "Next: " + Math.Round(nextNote.Time, 3) + " in " + Math.Round(nextNote.Time - GetSongPosition(), 1);

		var debugText = @$"{noteText}
{Engine.GetFramesPerSecond()}fps | {delta*1000:000.0}ms
{GetSongPosition().ToMinSec(true)}
";
		GetNode<Label>("RunningDetailsLabel").Text = debugText;

		UpdateLyrics(GetNode<RichTextLabel>("HBoxContainer/LyricsLabel"));
	}

	public double GetSongPosition()
    {
		if (_cachedSongPosition.HasValue) {
			return _cachedSongPosition.Value;
		}

		double time = _player.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix();
		time -= AudioServer.GetOutputLatency();
		_cachedSongPosition = time;
		return time;

		// TODO this might have some stuttering which we'll need to fix
    }

	private void SetUILabels(SongInfo info) {
		var infoLabel = GetNode<Label>("SongInfoLabel");
		infoLabel.Text = $"{info.Metadata.Name} ({info.Metadata.Year})\n{info.Metadata.Artist}";

		var detailsLabel = GetNode<Label>("SongDetailsLabel");
		var guitarTuning = "Tuning: " + Instrument.CalcTuningName(_state.Instrument.Config.Tuning, _state.Instrument.Config.CapoFret);
		var chordCount = _state.Instrument.Notes.Where(x => x.Notes.Count() > 1).Count();
		var singleNoteCount = _state.Instrument.Notes.Count - chordCount;
		detailsLabel.Text = $@"Playing: {_state.Instrument.Name}
Notes: {singleNoteCount}
Chords: {chordCount}
{guitarTuning}
First note @ {_state.Instrument.Notes.First().Time.ToMinSec()}
Last note @ {_state.Instrument.Notes.Last().Time.ToMinSec()}";
	}

	private NoteBlock NextNoteBlock() {
		var songPos = GetSongPosition();
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
		var songPos = GetSongPosition();
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

	private static LyricLine[] GetCurLines(SongState state, double songPos) {
		var lyrics = state.SongInfo.Lyrics;
		if (lyrics == null || !lyrics.Lines.Any())
			return Array.Empty<LyricLine>();

		if (lyrics.Lines.First().StartTime > songPos) {
			// its the first one time
			return lyrics.Lines.Take(2).ToArray();
		}

		foreach (var s in lyrics.Lines) {
			if (s.EndTime < songPos)
				continue;
			return lyrics.Lines.SkipWhile(x => x != s).Take(2).ToArray();
		}

		return Array.Empty<LyricLine>();
	}
}
