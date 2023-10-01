using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.Songs;
using System.IO;

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
		GetNode<Label>("GenreLabel").Text = "Genre: " + songInfo.Metadata.Genre;
		GetNode<Label>("YearLabel").Text = "Year:" + songInfo.Metadata.Year.ToString();
		GetNode<Label>("OtherLabel").Text = "Length: " + songInfo.Metadata.SongLength.ToMinSec();
	}
}
