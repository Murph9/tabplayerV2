using Godot;

public partial class GuitarChart : Node3D {
    
    public override void _Ready()
	{
        var sceneRoot = new Node3D() {
            Name = "guitarSceneRoot"
        };
        GetTree().Root.AddChild(sceneRoot);

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
        sceneRoot.AddChild(plane);

        var camera = new Camera3D() {
            Transform = new Transform3D(0, 0.310809f, -0.950472f, 0, 0.950472f, 0.310809f, 1, 0, 0, -10, 10, 8)
        };
        sceneRoot.AddChild(camera);

        var light = new DirectionalLight3D() {
            Transform = new Transform3D(-0.177838f, 0.752991f, -0.633544f, -0.317607f, 0.565433f, 0.761191f, 0.931397f, 0.336587f, 0.1386f, 0, 0, 0)
        };
        sceneRoot.AddChild(light);

        // set strings
        int index = 0;
        const int STRING_LENGTH = 50;
        foreach (var colour in new[] { "red", "yellow", "blue", "orange", "green", "purple"}) {
            var stringMaterial = GD.Load<StandardMaterial3D>("res://scenes/materials/" + colour + ".tres");
            
            var stringMesh = new BoxMesh() {
                Size = new Vector3(0.08f, 0.08f, STRING_LENGTH),
                Material = stringMaterial
            };
            var stringObj = new MeshInstance3D() {
                Transform = new Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, index, STRING_LENGTH/2f),
                Mesh = stringMesh
            };
            sceneRoot.AddChild(stringObj);

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
            sceneRoot.AddChild(fretObj);

            var pathObj = new MeshInstance3D() {
                Transform = new Transform3D(0, 5, 0, -1, 0, 0, 0, 0, 1, 15, DisplayConst.TRACK_BOTTOM_WORLD, DisplayConst.CalcFretPosZ(i)),
                Mesh = pathMesh
            };
            sceneRoot.AddChild(pathObj);
        }

        foreach (var i in new []{3,5,7,9,12,15,17,19,21,24}) {
            var label3d = new Label3D() {
                Text = i.ToString(),
                FontSize = 200,
                Shaded = true,
                Transform = new Transform3D(0, 1, 0, 0, 0, 1, 1, 0, 0, -0.25f, -0.5f, DisplayConst.CalcInFretPosZ(i))
            };

            sceneRoot.AddChild(label3d);
        }
	}

	public override void _Process(double delta)
	{
	}
}
