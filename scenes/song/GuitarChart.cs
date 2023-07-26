using Godot;

public partial class GuitarChart : Node3D {
    
    public override void _Ready()
	{
        var material = new StandardMaterial3D();
        material.Set("albedo_color", new Color("#D2B48C")); //tan

        var planeMesh = new PlaneMesh() {
            Size = new Vector2(6, 6),
            CenterOffset = new Vector3(2.5f, 0, -3),
            Material = material
        };
        var plane = new MeshInstance3D() {
            Transform = new Transform3D(0, -1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0),
            Mesh = planeMesh
        };
        GetTree().Root.AddChild(plane);

        var camera = new Camera3D() {
            Transform = new Transform3D(0, 0.310809f, -0.950472f, 0, 0.950472f, 0.310809f, 1, 0, 0, -10, 10, 8)
        };
        GetTree().Root.AddChild(camera);

        var light = new OmniLight3D() {
            Transform = new Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, -3, 3.9219f, 8),
            OmniRange = 15f
        };
        GetTree().Root.AddChild(light);

        // set strings
        int index = 0;
        foreach (var colour in new[] { "red", "yellow", "blue", "orange", "green", "purple"}) {
            var stringMaterial = GD.Load<StandardMaterial3D>("res://scenes/materials/" + colour + ".tres");
            
            var stringMesh = new BoxMesh() {
                Size = new Vector3(0.1f, 0.1f, 25),
                Material = stringMaterial
            };
            var stringObj = new MeshInstance3D() {
                Transform = new Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, index, 12.5f),
                Mesh = stringMesh
            };
            GetTree().Root.AddChild(stringObj);

            index++;
        }
        
        // set frets and fret lines
        var fretMaterial = new StandardMaterial3D();
        fretMaterial.Set("albedo_color", new Color("#D2B48C")); //tan
        var fretMesh = new BoxMesh() {
            Size = new Vector3(0.03f, 6, 0.03f),
            Material = fretMaterial
        };
        var pathMaterial = new StandardMaterial3D();
        pathMaterial.Set("albedo_color", new Color("#333333"));
        var pathMesh = new BoxMesh() {
            Size = new Vector3(0.03f, 6, 0.03f),
            Material = pathMaterial
        };
        for(int i = 0; i < 26; i++) {
            var meshObj = new MeshInstance3D() {
                Transform = new Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 2.5f, i),
                Mesh = fretMesh
            };
            GetTree().Root.AddChild(meshObj);

            var pathObj = new MeshInstance3D() {
                Transform = new Transform3D(0, 5, 0, -1, 0, 0, 0, 0, 1, 15, -0.5f, i),
                Mesh = pathMesh
            };
            GetTree().Root.AddChild(pathObj);
        }

        foreach (var i in new []{3,5,7,9,12,15,17,19,21,24}) {
            var label3d = new Label3D() {
                Text = i.ToString(),
                FontSize = 200,
                Shaded = true,
                Transform = new Transform3D(0, 1, 0, 0, 0, 1, 1, 0, 0, -0.251861f, -0.5f, i-0.5f)
            };

            GetTree().Root.AddChild(label3d);
        }
	}

	public override void _Process(double delta)
	{
	}
}
