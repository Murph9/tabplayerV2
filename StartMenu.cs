using Godot;
using System;
using TabPlayer.SomeImportantStuff;

namespace TabPlayer {
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
	}
}
