using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.scenes.song;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace murph9.TabPlayer.scenes;

public interface IAudioStreamPosition {
    public bool SongPlaying { get; }
    public double GetSongPosition();
}

public partial class SongScene : Node, IAudioStreamPosition {

    private const float SONG_SPEED_DIFF = 0.98f;

    private SongState _state;
    private AudioStreamOggVorbis _audioStream;
    private AudioStreamPlayer _player;
    private double? _cachedSongPosition;

    private NoteMiniGraph _noteGraphScene;
    private SongChart _songChartScene;


    public bool SongPlaying => _player?.Playing == true;

    private double _aPosition;
    private double _bPosition;

    [Signal]
    public delegate void ClosedEventHandler();

    public void _init(SongState state) {
        _state = state;
        _audioStream = AudioStreamOggVorbis.LoadFromBuffer(state.Audio);

        var instrumentList = GetNode<MenuButton>("DetailsVBoxContainer/VBoxContainer/InstrumentMenuButton");
        instrumentList.Text = state.Instrument.Name;

        var instrumentPopup = instrumentList.GetPopup();
        foreach (var item in _state.SongInfo.Instruments) {
            instrumentPopup.AddItem(item.Name);
        }
        instrumentPopup.SetItemChecked(_state.SongInfo.Instruments.ToList().IndexOf(state.Instrument), true);
        instrumentPopup.IdPressed += InstrumentChanged;
    }

    private void InstrumentChanged(long id) {
        var instrumentList = GetNode<MenuButton>("DetailsVBoxContainer/VBoxContainer/InstrumentMenuButton");
        instrumentList.Text = _state.InstrumentName = _state.SongInfo.Instruments.Skip((int)id).First().Name;

        // load the instrument
        LoadInstrumentFromState();
    }

    private void SongFinished() {
        _player.Play();
        _player.Seek(0);
        _player.StreamPaused = true;
        Pause();
    }

    private void PauseButton_Pressed() {
        if (_player.StreamPaused) {
            Resume();
        } else {
            Pause();
        }
    }

    private void Pause() {
        _player.StreamPaused = true;
    }

    private void Resume() {
        _player.StreamPaused = false;
    }

    private void Quit() {
        Pause(); // prevent errors in final update frame
        _player.Stop();

        EmitSignal(SignalName.Closed);
    }

    public override void _Input(InputEvent @event) {
        if (@event.IsActionPressed("ui_cancel")) {
            Quit();
        } else if (@event.IsActionPressed("song_pause")) {
            PauseButton_Pressed();
        } else if (@event.IsActionPressed("song_skip_forward_10")) {
            Skip10Sec();
        } else if (@event.IsActionPressed("song_skip_backward_10")) {
            Back10Sec();
        } else if (@event.IsActionPressed("song_skip_to_next")) {
            SkipToNext();
        } else if (@event.IsActionPressed("song_restart")) {
            RestartSong();
        } else if (@event.IsActionPressed("song_speed_down")) {
            SlowDownPlayback();
        } else if (@event.IsActionPressed("song_speed_up")) {
            SpeedUpPlayback();
        } else if (@event.IsActionPressed("song_set_loop_start")) {
            PickA();
        } else if (@event.IsActionPressed("song_set_loop_end")) {
            PickB();
        } else if (@event.IsActionPressed("song_reset_loop")) {
            ClearLoopTimes();
        } else if (@event.IsActionPressed("song_reset_speed")) {
            ResetSongSpeed();
        }
    }

    private void Skip10Sec() => _player.Seek((float)GetSongPosition() + 10f);
    private void Back10Sec() => _player.Seek((float)GetSongPosition() - 10f);
    private void SkipToNext() {
        var nextNote = NextNoteBlock();
        if (nextNote == null)
            return;
        _player.Seek(nextNote.Time - 1.5f);
    }
    private void RestartSong() => _player.Seek(0);

    private void SlowDownPlayback() => AdjustPitch(SONG_SPEED_DIFF);
    private void SpeedUpPlayback() => AdjustPitch(1f / SONG_SPEED_DIFF);
    private void AdjustPitch(float amount) {
        if (_player.PitchScale < 0.5f && amount < 1)
            return;
        if (_player.PitchScale > 1.7f && amount > 1)
            return;

        SetSongSpeed(_player.PitchScale * amount);
    }
    private void ResetSongSpeed() => SetSongSpeed(1);
    private void SetSongSpeed(float fraction) {
        _player.PitchScale = fraction;

        // PitchShift causes issues so we only enable it if we have to, see why: https://github.com/godotengine/godot/issues/20198
        if (fraction == 1) {
            _player.Bus = "Master";
        } else {
            _player.Bus = "SongPlayback";
        }

        var busId = AudioServer.GetBusIndex("SongPlayback");
        var effect = AudioServer.GetBusEffect(busId, 0) as AudioEffectPitchShift;
        effect.PitchScale = 1 / _player.PitchScale;
    }

    private void PickA() {
        _aPosition = GetSongPosition();
    }
    private void PickB() {
        _bPosition = GetSongPosition();
    }
    private void ClearLoopTimes() {
        _aPosition = default;
        _bPosition = default;
    }

    private async void MoveSongPosition() {
        var posLineEdit = GetNode<LineEdit>("GridContainer/PositionSetLineEdit");
        // parse from the 2 supported formats:
        // number directly
        _ = float.TryParse(posLineEdit.Text, out float pos);

        // and 'XXm XXs XXXms' which is already set
        if (pos == default) {
            var result = SongPositionRegex().Match(posLineEdit.Text);
            if (result.Success) {
                var results = result.Groups;
                pos = float.Parse(results[1].Value) * 60 + float.Parse(results[2].Value) + float.Parse(results[3].Value) / 1000f;
            }
        }

        if (pos == default)
            return;

        // validate that its in the song
        if (pos < 0)
            return;
        if (pos > _player.Stream.GetLength())
            return;

        _player.StreamPaused = false;
        _player.Seek(pos);

        // wait 2 frames to trigger this re-pause so it shows the position update
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        CallDeferred(MethodName.Pause);
    }

    public override void _Ready() {
        var info = _state.SongInfo;
        SetUILabels(info);

        _player = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _player.Stream = _audioStream;
        _player.Play();

        var guitarChartScene = GD.Load<CSharpScript>("res://scenes/song/GuitarChart.cs").New().As<GuitarChart>();
        guitarChartScene._init(_state, this);
        AddChild(guitarChartScene);

        LoadInstrumentFromState();
    }

    private void LoadInstrumentFromState() {
        if (_noteGraphScene != null) {
            RemoveChild(_noteGraphScene);
        }

        _noteGraphScene = GD.Load<CSharpScript>("res://scenes/song/NoteMiniGraph.cs").New().As<NoteMiniGraph>();
        _noteGraphScene._init(_state, this);
        AddChild(_noteGraphScene);

        try {
            if (_songChartScene != null) {
                RemoveChild(_songChartScene);
            }
            _songChartScene = GD.Load<CSharpScript>("res://scenes/song/SongChart.cs").New().As<SongChart>();
            _songChartScene._init(_state.Instrument);
            AddChild(_songChartScene);
        } catch (Exception e) {
            GD.Print(e);
        }
    }

    public override void _Process(double delta) {
        _cachedSongPosition = null;

        var songPosition = GetSongPosition();

        // update the AB positions
        var aLabel = GetNode<Label>("GridContainer/ABLabelStart");
        aLabel.Text = _aPosition == default ? "" : _aPosition.ToMinSec(true);
        var bLabel = GetNode<Label>("GridContainer/ABLabelEnd");
        bLabel.Text = _bPosition == default ? "" : _bPosition.ToMinSec(true);

        if (_aPosition != default && _bPosition != default) {
            if (_aPosition < songPosition && delta + songPosition > _bPosition) {
                _player.Seek((float)_aPosition);
            }
        }

        if (!_player.Playing)
            return;

        var nextNote = NextNoteBlock();

        string noteText = "No note";
        if (nextNote != null) {
            var nextNoteLabel = GetNode<Label>("GridContainer/SkipToNextLabel2");
            nextNoteLabel.Text = "at " + nextNote.Time.ToMinSec(false);

            noteText = "Next Note: " + nextNote.Time.ToMinSec(false) + " in " + Math.Round(nextNote.Time - songPosition, 1);
        }

        GetNode<Label>("RunningDetailsLabel").Text = @$"{Engine.GetFramesPerSecond()}fps | {delta * 1000:000.0}ms
{songPosition.ToMinSec(true)}";

        var detailsLabel = GetNode<Label>("DetailsVBoxContainer/SongDetailsLabel");
        // TODO move this to the LoadInstrumentFromState by taking noteText out
        detailsLabel.Text = $@"---------
Tuning: {Instrument.CalcTuningName(_state.Instrument.Config.Tuning, _state.Instrument.Config.CapoFret)}
Notes: {_state.Instrument.SingleNoteCount()}
Chords: {_state.Instrument.ChordCount()}
First note @ {_state.Instrument.Notes.First().Time.ToMinSec()}
Last note @ {_state.Instrument.Notes.Last().Time.ToMinSec()}
---------
{noteText}
";


        UpdateLyrics(songPosition, GetNode<RichTextLabel>("HBoxContainer/LyricsLabel"));

        // update the set position box
        var posLineEdit = GetNode<LineEdit>("GridContainer/PositionSetLineEdit");
        posLineEdit.Text = songPosition.ToMinSec(true);

        var songSpeedLabel = GetNode<Label>("GridContainer/SongSpeedLabel");
        songSpeedLabel.Text = Math.Round(_player.PitchScale * 100, 1) + "%";
    }

    public double GetSongPosition() {
        if (_cachedSongPosition.HasValue) {
            return _cachedSongPosition.Value;
        }

        double time = _player.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix();
        time -= AudioServer.GetOutputLatency();
        _cachedSongPosition = time;
        return time;
    }

    private void SetUILabels(SongInfo info) {
        var infoLabel = GetNode<Label>("DetailsVBoxContainer/SongInfoLabel");
        infoLabel.Text = $"{info.Metadata.Name} ({info.Metadata.Year})\n{info.Metadata.Artist}";
    }

    private NoteBlock NextNoteBlock() {
        var songPos = GetSongPosition();
        foreach (var b in _state.Instrument.Notes) {
            if (b.Time > songPos)
                return b;
        }
        return null;
    }

    private void UpdateLyrics(double songPos, RichTextLabel label) {
        label.Clear();
        label.PushFontSize(40);

        // TODO don't move to the next lyrics until the next set starts
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
        if (lyrics == null || lyrics.Lines.Count == 0)
            return [];

        if (lyrics.Lines.First().StartTime > songPos) {
            // its the first one time
            return lyrics.Lines.Take(2).ToArray();
        }

        foreach (var s in lyrics.Lines) {
            if (s.EndTime < songPos)
                continue;
            return lyrics.Lines.SkipWhile(x => x != s).Take(2).ToArray();
        }

        return [];
    }

    [GeneratedRegex(@"^.*(\d+)\s*m\s+(\d+)\s*s\s+(\d+)\s*ms.*$")]
    private static partial Regex SongPositionRegex();
}
