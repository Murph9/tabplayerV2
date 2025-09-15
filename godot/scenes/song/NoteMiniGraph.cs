using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace murph9.TabPlayer.scenes.song;

public partial class NoteMiniGraph : Node2D {

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

        _notePlotImage = ImageTexture.CreateFromImage(image);
    }

    public override void _Draw() {
        base._Draw();

        DrawTexture(_notePlotImage, Vector2.Zero);

        var windowSize = GetViewport().GetVisibleRect();
        var posX = ((1 - SCREEN_SPREAD) + SCREEN_SPREAD * (float)_audio.GetSongPosition() / (_songState.Instrument.LastNoteTime + 4)) * (int)windowSize.End.X;
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
}
