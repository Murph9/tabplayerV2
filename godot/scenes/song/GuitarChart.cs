using System;
using System.Linq;
using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;

namespace murph9.TabPlayer.scenes.song;

public partial class GuitarChart : Node3D {
    private const float CAM_MOVE_SPEED = 0.2f;

    private SongState _songState;
    private IAudioStreamPosition _audio;

    private NoteBlock _lastNoteBlock;
    private Node3D _lastNoteBlockNode;

    public void _init(SongState songState, IAudioStreamPosition audio) {
        _songState = songState;
        _audio = audio;
    }

    public override void _Ready() {
        var material = new StandardMaterial3D() {
            AlbedoColor = Colors.Tan
        };

        var planeMesh = new PlaneMesh() {
            Size = new Vector2(6, 6),
            CenterOffset = new Vector3(2.5f, 0, -3),
            Material = material
        };
        var plane = new MeshInstance3D() {
            Transform = new Transform3D(0, -1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0),
            Mesh = planeMesh
        };
        AddChild(plane);

        var camera = new Camera3D() {
            Fov = 60,
            Transform = new Transform3D(0, 0.310809f, -0.950472f, 0, 0.950472f, 0.310809f, 1, 0, 0, -12, 10, 8),
        };
        AddChild(camera);

        var light = new DirectionalLight3D() {
            Transform = new Transform3D(-0.177838f, 0.752991f, -0.633544f, -0.317607f, 0.565433f, 0.761191f, 0.931397f, 0.336587f, 0.1386f, 0, 0, 0)
        };
        AddChild(light);

        // set strings
        const int STRING_LENGTH = 50;

        foreach (var i in Enumerable.Range(0, 6)) {
            var colour = SettingsService.GetColorFromStringNum(i);
            var stringMaterial = new StandardMaterial3D() {
                AlbedoColor = colour
            };

            var stringMesh = new BoxMesh() {
                Size = new Vector3(0.08f, 0.08f, STRING_LENGTH),
                Material = stringMaterial
            };
            var stringObj = new MeshInstance3D() {
                Transform = new Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, DisplayConst.CalcNoteHeightY(i), STRING_LENGTH / 2f),
                Mesh = stringMesh
            };
            AddChild(stringObj);
        }

        // set frets and fret lines
        var fretMaterial = new StandardMaterial3D() {
            AlbedoColor = Colors.Tan
        };
        var fretMesh = new BoxMesh() {
            Size = new Vector3(0.03f, 5 + Math.Abs(DisplayConst.TRACK_BOTTOM_WORLD * 2), 0.03f),
            Material = fretMaterial
        };
        var pathMaterial = new StandardMaterial3D() {
            AlbedoColor = Colors.DarkGray
        };
        var pathMesh = new BoxMesh() {
            Size = new Vector3(0.03f, 6, 0.03f),
            Material = pathMaterial
        };
        for (int i = 0; i < 25; i++) {
            var fretObj = new MeshInstance3D() {
                Transform = new Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 2.5f, DisplayConst.CalcFretPosZ(i)),
                Mesh = fretMesh
            };
            AddChild(fretObj);

            var pathObj = new MeshInstance3D() {
                Transform = new Transform3D(0, 5, 0, -1, 0, 0, 0, 0, 1, 15, DisplayConst.TRACK_BOTTOM_WORLD, DisplayConst.CalcFretPosZ(i)),
                Mesh = pathMesh
            };
            AddChild(pathObj);
        }

        foreach (var i in new[] { 3, 5, 7, 9, 12, 15, 17, 19, 21, 24 }) {
            var label3d = new Label3D() {
                Text = i.ToString(),
                FontSize = 200,
                Shaded = true,
                Transform = new Transform3D(0, 1, 0, 0, 0, 1, 1, 0, 0, -0.25f, DisplayConst.TRACK_BOTTOM_WORLD, DisplayConst.CalcInFretPosZ(i))
            };

            AddChild(label3d);
        }
    }

    public override void _Process(double delta) {
        if (!_audio.SongPlaying)
            return;

        var block = NextNoteBlock();
        if (block != null) {
            var cam = GetTree().Root.GetCamera3D();
            var camMoveSpeed = SettingsService.Settings().CameraAimSpeed / 50f; //so the setting has more logic numbers
            var wantPos = DisplayConst.CalcMiddleWindowZ(block.FretWindowStart, block.FretWindowLength);
            var newZ = cam.Position.Z * (1 - delta * camMoveSpeed) + wantPos * delta * camMoveSpeed;
            cam.Position = new Vector3(cam.Position.X, cam.Position.Y, (float)newZ);
        }

        var realCamSongPos = _audio.GetSongPosition() - SettingsService.Settings().AudioPositionOffsetMs / 1000f;
        var newPos = new Vector3((float)realCamSongPos * _songState.Instrument.Config.NoteSpeed, Position.Y, Position.Z);
        Position = newPos;

        if (block?.Time != _lastNoteBlock?.Time) { // diff notes i hope
            _lastNoteBlock = block;

            _lastNoteBlockNode?.QueueFree();
            _lastNoteBlockNode = null;
            if (block == null) return;

            _lastNoteBlockNode = new Node3D();
            AddChild(_lastNoteBlockNode); // TODO show sustains

            var config = _songState.Instrument.Config;
            foreach (var n in block.Notes) {
                // .2 so we can see the open notes
                var note = NoteGenerator.GetBasicNote(n, config, 0.2f / config.NoteSpeed, _lastNoteBlock.FretWindowStart, _lastNoteBlock.FretWindowLength);
                // TODO scale with open notes working
                _lastNoteBlockNode.AddChild(note);
            }
        }
    }

    private NoteBlock NextNoteBlock() {
        var songPos = _audio.GetSongPosition();
        foreach (var b in _songState.Instrument.Notes) {
            if (b.Time > songPos)
                return b;
        }
        return null;
    }
}
