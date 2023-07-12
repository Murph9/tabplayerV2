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
		GD.Print(dir);
	}

	public void File_Selected(string path) {
		GD.Print(path);
	}
	public void Files_Selected(String[] paths) {
		GD.Print(paths);
	}

	public void BackButton_Pressed() {
		GetTree().ChangeSceneToFile("res://StartMenu.tscn");
	}
}
