# TabPlayer using Godot and C#

This application lets you play rocksmith cldc and others by importing them in app forever.

It allows stopping, rewinding and skipping through songs much faster than rocksmith.
And allows you to play along with the notes as you would TAB or sheet music.

This doesn't listen to a plugged in guitar or tell you notes hit

See the MiiChannel song, note the strings and note preview at: https://www.murph9.com/mygames

## How to play

1. Download the latest release from [Github Releases](https://github.com/Murph9/tabplayerV2/releases)

1. Download some songs from various sources like [CustomsForge](https://customsforge.com/index.php) (requires a free account)

1. Go to the convert page and select the downloaded songs
1. Run the reload song list feature
1. Play Songs

## Included C# Dependencies

-   [PsarcLib](https://github.com/kokolihapihvi/Rocksmith2014PsarcLib)
-   [revorbstd](https://github.com/overtools/revorbstd)
-   [WEMSharp](https://github.com/neon-nyan/WEMSharp)

Most have slight modifications to work in the c# .net 7 environment for godot.

### Modifying and Useful Notes

#### How to create a new c# and attach the project

```
dotnet new classlib -o TabPlayer.SomeImportantStuff
dotnet sln '.\TabPlayer.sln' add .\TabPlayer.SomeImportantStuff\TabPlayer.SomeImportantStuff.csproj
### not needed as godot does "stuff": dotnet add '.\TabPlayer.csproj' reference .\TabPlayer.SomeImportantStuff\TabPlayer.SomeImportantStuff.csproj
```
