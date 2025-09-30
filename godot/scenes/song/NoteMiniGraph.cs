using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace murph9.TabPlayer.scenes.song;

public partial class NoteMiniGraph : Node2D {

    private const float BUCKET_SIZE = 2; // seconds
    private const int BUCKET_BOTTOM_OFFSET = 20; // px
    private const int PIXEL_SIZE = 3;
    private const float SCREEN_SPREAD = 0.8f;

    private SongState _songState;
    private IAudioStreamPosition _audio;
    private Line2D _currentPostionLine;

    private Texture2D _notePlotImage;

    public void _init(SongState songState, IAudioStreamPosition audio) {
        _songState = songState;
        _audio = audio;
    }

    public override void _Ready() {
        var stringColours = SettingsService.Settings().StringColours.ToArray();

        var windowSize = GetViewport().GetVisibleRect();
        var image = Image.CreateEmpty((int)windowSize.End.X, (int)windowSize.End.Y, false, Image.Format.Rgba8);

        foreach (var noteBlock in _songState.Instrument.Notes) {
            var posX = ((1 - SCREEN_SPREAD) + SCREEN_SPREAD * noteBlock.Time / (_songState.Instrument.LastNoteTime + 4)) * (int)windowSize.End.X;

            if (noteBlock.IsChord) {
                for (var i = noteBlock.FretWindowStart * PIXEL_SIZE; i < (noteBlock.FretWindowStart + noteBlock.FretWindowLength) * PIXEL_SIZE; i++) {
                    var posY = (int)windowSize.End.Y - i - BUCKET_BOTTOM_OFFSET;
                    DrawNotePixel(image, new Color(Colors.White, 0.3f), new Vector2I((int)posX, posY), 1);
                }
            }

            if (noteBlock.ChordFlags.Contains(NoteBlockFlags.MUTE)) {
                var posY = (int)windowSize.End.Y - noteBlock.FretWindowStart * PIXEL_SIZE - BUCKET_BOTTOM_OFFSET;
                DrawNotePixel(image, Colors.LightGray, new Vector2I((int)posX, posY));
            } else {
                foreach (var note in noteBlock.Notes) {
                    var posY = (int)windowSize.End.Y - note.FretNum * PIXEL_SIZE - BUCKET_BOTTOM_OFFSET;
                    DrawNotePixel(image, stringColours[note.StringNum], new Vector2I((int)posX, posY));
                }
            }
        }

        // TODO align to section starts
        var noteBuckets = _songState.Instrument.Notes.ToLookup(x => Math.Round(x.Time / BUCKET_SIZE));
        for (var i = 0f; i < _songState.Instrument.LastNoteTime; i += BUCKET_SIZE) {
            var bucket = _songState.Instrument.Notes.Where(x => x.Time >= i && x.Time < i + BUCKET_SIZE);
            if (!bucket.Any()) continue;

            var first = bucket.First();

            // get count of note and count of chords
            var chordCount = bucket.Count(x => x.IsChord);
            var noteCount = bucket.Count(x => !x.IsChord);

            var posX = ((1 - SCREEN_SPREAD) + SCREEN_SPREAD * i / (_songState.Instrument.LastNoteTime + 4)) * (int)windowSize.End.X;
            var posY = (int)windowSize.End.Y - BUCKET_BOTTOM_OFFSET + PIXEL_SIZE;

            var posXNext = ((1 - SCREEN_SPREAD) + SCREEN_SPREAD * (i + BUCKET_SIZE) / (_songState.Instrument.LastNoteTime + 4)) * (int)windowSize.End.X;

            DrawRectPixels(image, new Color(Colors.Black, 0.4f), new Vector2I((int)posX, posY), new Vector2I((int)posXNext, posY + bucket.Count()));
        }

        _notePlotImage = ImageTexture.CreateFromImage(image);
    }

    public override void _Draw() {
        base._Draw();

        DrawTexture(_notePlotImage, Vector2.Zero);

        var windowSize = GetViewport().GetVisibleRect();
        var posX = (1 - SCREEN_SPREAD + SCREEN_SPREAD * (float)_audio.GetSongPosition() / (_songState.Instrument.LastNoteTime + 4)) * (int)windowSize.End.X;
        DrawLine(new Vector2(posX, (int)windowSize.End.Y - BUCKET_BOTTOM_OFFSET), new Vector2(posX, (int)windowSize.End.Y - BUCKET_BOTTOM_OFFSET - 24 * PIXEL_SIZE), Colors.White, width: 1);
    }

    public override void _Process(double delta) {
        if (!_audio.SongPlaying)
            return;

        QueueRedraw();
    }

    private static void DrawNotePixel(Image image, Color colour, Vector2I pos, int size = PIXEL_SIZE) {
        for (int i = -size / 2; i < -size / 2 + size; i++) {
            for (int j = -size / 2; j < -size / 2 + size; j++) {
                image.SetPixel(pos.X + i, pos.Y + j, colour);
            }
        }
    }

    private static void DrawRectPixels(Image image, Color colour, Vector2I start, Vector2I end) {
        if (start.X > end.X || start.Y > end.Y) GD.Print("Error with " + start + "," + end + " rect draw");

        for (var i = start.X; i < end.X; i++) {
            for (var j = start.Y; j < end.Y; j++) {
                image.SetPixel(i, j, colour);
            }
        }
    }
}
