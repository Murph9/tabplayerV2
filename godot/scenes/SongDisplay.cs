using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.Songs;
using System;
using System.IO;

namespace murph9.TabPlayer.scenes;

public partial class SongDisplay : VBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SongChanged(string folderName) => LoadSong(folderName);

	private void LoadSong(string folderName) {
		var songInfo = SongLoader.Load(folderName, "lead").SongInfo;

		// var image = Image.LoadFromFile(@"C:\some file path");
		// GetNode<TextureRect>("AlbumArtTextureRect").Texture = ImageTexture.CreateFromImage(image);

		GetNode<Label>("ArtistLabel").Text = "Artist: " + songInfo.Metadata.Artist;
		GetNode<Label>("SongNameLabel").Text = "Name: " + songInfo.Metadata.Name;
		GetNode<Label>("AlbumLabel").Text = "Album: " + songInfo.Metadata.Album;
		GetNode<Label>("GenreLabel").Text = "Genre: " + songInfo.Metadata.Genre;
		GetNode<Label>("YearLabel").Text = "Year:" + songInfo.Metadata.Year.ToString();
		GetNode<Label>("OtherLabel").Text = "Length: " + songInfo.Metadata.SongLength.ToMinSec();
	}
}
