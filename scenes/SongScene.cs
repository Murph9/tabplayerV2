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

	public override void _Ready()
	{
		var info = _state.SongInfo;
		setUILabels(info);

		_audioController.Play();
		
		var guitarChart = GD.Load<CSharpScript>("res://scenes/song/GuitarChart.cs");
		GetTree().Root.AddChild(guitarChart.New().As<GuitarChart>());

		var noteGraph = GD.Load<CSharpScript>("res://scenes/song/NoteGraph.cs");
		var noteGraphScene = noteGraph.New().As<NoteGraph>();
		noteGraphScene._init(_state, _audioController);
		GetTree().Root.AddChild(noteGraphScene);

		try {
			var songScene = GD.Load<CSharpScript>("res://scenes/song/SongChart.cs");
			var s = songScene.New().As<SongChart>();
			s._init(_state.Instrument);
			GetTree().Root.AddChild(s);
		} catch (Exception e) {
			GD.Print(e);
		}

		// TODO test
		// var obj = MeshGenerator.Box(Colors.Red, new Vector3(20, 0, 7));
		// GetTree().Root.AddChild(obj);
	}

	public override void _Process(double delta)
	{
		var guiScene = GetNode<Node3D>("/root/guitarSceneRoot");
		var newPos = new Vector3((float)_audioController.SongPosition*_state.Instrument.Config.NoteSpeed, guiScene.Position.Y, guiScene.Position.Z);
		guiScene.Position = newPos;

		var nextNote = NextNoteBlock();

		var noteText = (nextNote == null) ? "No note" : "Next: " + Math.Round(nextNote.Time, 3) + " in " + Math.Round(nextNote.Time - _audioController.SongPosition, 1);

		var debugText = @$"{_audioController.SongPosition.ToMinSec(true)}
Obj #: {GetTree().Root.GetChildCount()}
{Engine.GetFramesPerSecond()}fps | {(delta*1000).ToString("000.0")}ms
{noteText}
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
