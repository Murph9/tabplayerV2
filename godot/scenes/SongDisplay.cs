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
	public delegate void SongSelectedEventHandler(string folder);

	private string _folderName;

	public override void _Ready() {}

	public override void _Process(double delta) { }

	private void Play_Selected() => EmitSignal(SignalName.SongSelected, _folderName);

	public void SongChanged(string folderName) {
		_folderName = folderName;
		GetNode<Button>("PlayButton").Visible = true; // please don't press it before we are ready
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

		grid.AddChild(new Label() { Text = "Instrument Name"});
		grid.AddChild(new Label() { Text = "Tuning"});
		grid.AddChild(new Label() { Text = "Note Counts"});
		grid.AddChild(new Label() { Text = "Note Density"});

		grid.Columns = grid.GetChildren().Count;

		foreach (var i in songInfo.Instruments) {
			grid.AddChild(new Label() { Text = i.Name });
			grid.AddChild(new Label() { Text = Instrument.CalcTuningName(i.Config.Tuning, i.Config.CapoFret) });
			grid.AddChild(new Label() { Text = $"{i.TotalNoteCount()}, c: {i.ChordCount()}, n: {i.SingleNoteCount()}" });
			grid.AddChild(new Label() { Text = i.GetNoteDensity(songInfo).ToString() });
		}
	}
}
