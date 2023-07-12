using Godot;
using System;
using murph9.TabPlayer.SomeImportantStuff;

namespace murph9.TabPlayer {
	public partial class StartMenu : Node
	{
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			var result = ServiceTest.GetServiceResult();
			GD.Print(result);
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}

		public void StartButton_Pressed() {
			PackedScene packedScene = ResourceLoader.Load("res://scenes/SongScene.tscn") as PackedScene;
			var scene = packedScene.Instantiate() as SongScene;
			scene.init(null, null);
			
			GetTree().Root.AddChild(scene);

			GetTree().Root.RemoveChild(this);
		}

		public void InfoButton_Pressed() {
			GetTree().ChangeSceneToFile("res://scenes/InfoPage.tscn");
		}

		public void ConvertButton_Pressed() {
			GetTree().ChangeSceneToFile("res://scenes/ConvertMenu.tscn");
		}
	}
}
