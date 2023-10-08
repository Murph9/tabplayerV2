using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
using System.IO;
using System.Linq;

namespace murph9.TabPlayer.scenes;

public partial class SongDisplay : VBoxContainer
{
	[Signal]
	public delegate void SongSelectedEventHandler(string folder, string instrument);

	private string _folderName;

	public override void _Ready() {}

	public override void _Process(double delta) { }

	public void SongChanged(string folderName) {
		_folderName = folderName;
		LoadSong();
	}

	private void LoadSong() {

		var songInfo = SongLoader.Load(_folderName, null).SongInfo;

		var image = Image.LoadFromFile(Path.Combine(SongFileManager.SONG_FOLDER, _folderName, SongFileManager.ALBUM_ART_NAME));
		GetNode<TextureRect>("AlbumArtTextureRect").Texture = ImageTexture.CreateFromImage(image);

		GetNode<Label>("ArtistLabel").Text = "Artist: " + songInfo.Metadata.Artist;
		GetNode<Label>("SongNameLabel").Text = "Name: " + songInfo.Metadata.Name;
		GetNode<Label>("AlbumLabel").Text = "Album: " + songInfo.Metadata.Album;
		GetNode<Label>("YearLabel").Text = "Year: " + songInfo.Metadata.Year.ToString();
		GetNode<Label>("OtherLabel").Text = "Length: " + songInfo.Metadata.SongLength.ToMinSec();

		var grid = GetNode<GridContainer>("InstrumentGridContainer");
		grid.GetChildren().ToList().ForEach(grid.RemoveChild); // remove all children from the grid

		grid.AddChild(new Label());
		grid.AddChild(new Label() { Text = "Tuning"});
		grid.AddChild(new Label() { Text = "Note Counts"});
		grid.AddChild(new Label() { Text = "Note Density"});
		
		grid.Columns = grid.GetChildren().Count;

		var insturmentsOrdered = songInfo.Instruments.OrderBy(x => 
			{
				if (SongInfo.INSTRUMENT_ORDER.ContainsKey(x.Name))
					return SongInfo.INSTRUMENT_ORDER[x.Name];
				// handle all other entries as in given order
				return 999;
			}).ToList();

		foreach (var i in insturmentsOrdered) {
			var button = new Button() { Text = "Play " + i.Name.Capitalize() };
			button.Pressed += () => {
				EmitSignal(SignalName.SongSelected, _folderName, i.Name);
			};
			grid.AddChild(button);
			grid.AddChild(new Label() { Text = Instrument.CalcTuningName(i.Config.Tuning, i.Config.CapoFret) });
			grid.AddChild(new Label() { Text = $"{i.TotalNoteCount()}, c: {i.ChordCount()}, n: {i.SingleNoteCount()}" });
			grid.AddChild(new Label() { Text = i.GetNoteDensity(songInfo).ToFixedPlaces(2) });
		}
	}
}
