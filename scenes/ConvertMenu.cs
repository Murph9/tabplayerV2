using Godot;
using System;

public partial class ConvertMenu : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void GoButton_Pressed() {
		var a = GetNode<FileDialog>("FileDialog");
		a.Visible = true;
	}

	public void Dir_Selected(string dir) {
		Files_Selected(Directory.GetFiles(dir, "*.wem"));
	}

	public void File_Selected(string path) {
		Files_Selected(new string[] {path});
	}
	public void Files_Selected(String[] paths) {
		GD.Print(paths);
		using var fileS = File.Open(@"C:\Users\murph\AppData\Local\murph9.TabPlayer\3-Doors-Down_Kryptonite\768764903.wem", FileMode.Open);
		using var outS = File.Open(@"C:\Users\murph\Desktop\temp.ogg", FileMode.Create);
		var a = new WEMSharp.WEMFile(fileS, WEMSharp.WEMForcePacketFormat.NoForcePacketFormat);
		try {
			a.GenerateOGG(outS, false, false);
		} catch (Exception e) {
			GD.Print(e, "ogg file not converted :(");
		}
	}

	public void BackButton_Pressed() {
		GetTree().ChangeSceneToFile("res://StartMenu.tscn");
	}
}
