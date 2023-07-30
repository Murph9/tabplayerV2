using Godot;

public class MeshGenerator {
    
    public static MeshInstance3D BoxLine(Color c, Vector3 start, Vector3 end) {
        var mat = new StandardMaterial3D() {
            AlbedoColor = c
        };
        var length = (end - start).Length();
        
        var mesh = new BoxMesh() {
            Size = new Vector3(length, 0.1f, 0.1f),
            Material = mat
        };
        var meshObj = new MeshInstance3D() {
            Transform = new Transform3D(new Basis(new Quaternion(new Vector3(1,0,0), (end-start).Normalized())), start.Lerp(end, 0.5f)),
            Mesh = mesh
        };

        return meshObj;
    }

    public static Node3D TextVertical(string text, Vector3 pos) {
        return new Label3D() {
            Text = text,
            FontSize = 200,
            Shaded = true,
            Transform = new Transform3D(new Basis(0, 0, -1, 0, 1, 0, 1, 0, 0), pos),
        };
    }

    public static Node3D Box(Color c, Vector3 pos) {
        var mat = new StandardMaterial3D() {
            AlbedoColor = c
        };

        var mesh = new BoxMesh() {
            Size = new Vector3(1, 1, 1),
            Material = mat
        };
        var meshObj = new MeshInstance3D() {
            Transform = new Transform3D(Basis.Identity, pos),
            Mesh = mesh
        };

        return meshObj;
    }

    public static Node3D Plane(Color c, Vector3 center, Vector2 size) {
        var mat = new StandardMaterial3D() {
            AlbedoColor = c
        };

        var mesh = new PlaneMesh() {
            Size = size,
            Material = mat
        };
        var meshObj = new MeshInstance3D() {
            Transform = new Transform3D(Basis.Identity, center),
            Mesh = mesh
        };

        return meshObj;
    }
}
