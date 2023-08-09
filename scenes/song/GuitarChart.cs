using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;

namespace murph9.TabPlayer.scenes.song;

public partial class GuitarChart : Node3D {
    private const float CAM_MOVE_SPEED = 0.2f;

    private SongState _songState;
    private AudioController _audioController;
    
    private NoteBlock _lastNoteBlock;
    private Node3D _lastNoteBlockNode;

    public void _init(SongState songState, AudioController audio) {
        _songState = songState;
        _audioController = audio;
    }

    public override void _Ready()
	{
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
        int index = 0;
        const int STRING_LENGTH = 50;
        foreach (var colour in new[] { Colors.Red, Colors.Yellow, Colors.Blue, Colors.Orange, Colors.Green, Colors.Purple}) {
            var stringMaterial = new StandardMaterial3D() {
                AlbedoColor = colour
            };
            
            var stringMesh = new BoxMesh() {
                Size = new Vector3(0.08f, 0.08f, STRING_LENGTH),
                Material = stringMaterial
            };
            var stringObj = new MeshInstance3D() {
                Transform = new Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, index, STRING_LENGTH/2f),
                Mesh = stringMesh
            };
            AddChild(stringObj);

            index++;
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

        foreach (var i in new []{3,5,7,9,12,15,17,19,21,24}) {
            var label3d = new Label3D() {
                Text = i.ToString(),
                FontSize = 200,
                Shaded = true,
                Transform = new Transform3D(0, 1, 0, 0, 0, 1, 1, 0, 0, -0.25f, -0.5f, DisplayConst.CalcInFretPosZ(i))
            };

            AddChild(label3d);
        }
	}

	public override void _Process(double delta)
	{
		if (!_audioController.Active)
			return;

        var block = NextNoteBlock();
        if (block != null) {
            var cam = GetTree().Root.GetCamera3D();
            
            var wantPos = DisplayConst.CalcMiddleWindowZ(block.FretWindowStart, block.FretWindowLength);
            var newZ = cam.Position.Z*(1-delta*CAM_MOVE_SPEED) + wantPos*delta*CAM_MOVE_SPEED;
            cam.Position = new Vector3(cam.Position.X, cam.Position.Y, (float)newZ);
        }

		var newPos = new Vector3((float)_audioController.SongPosition * _songState.Instrument.Config.NoteSpeed, Position.Y, Position.Z);
		Position = newPos;

        if (block?.Time != _lastNoteBlock?.Time) { // diff notes i hope
            _lastNoteBlock = block;

            _lastNoteBlockNode?.QueueFree();
            if (block == null) return;

            _lastNoteBlockNode = new Node3D();
            AddChild(_lastNoteBlockNode);

            var config = _songState.Instrument.Config;
            foreach (var n in block.Notes) {
                // .2 so we can see the open notes
                var note = NoteGenerator.GetBasicNote(n, config, 0.2f/config.NoteSpeed, _lastNoteBlock.FretWindowStart, _lastNoteBlock.FretWindowLength);
                // TODO scale with open notes working
                _lastNoteBlockNode.AddChild(note);
            }
        }
	}

    private NoteBlock NextNoteBlock() {
		var songPos = _audioController.SongPosition;
		foreach (var b in _songState.Instrument.Notes) {
			if (b.Time > songPos)
				return b;
		}
		return null;
	}
}
