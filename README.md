TabPlayer using Godot and C#

Yay


## How to create a new c# and attach it project

```
dotnet new classlib -o TabPlayer.SomeImportantStuff
dotnet sln '.\TabPlayer.sln' add .\TabPlayer.SomeImportantStuff\TabPlayer.SomeImportantStuff.csproj
### not needed as godot does "stuff": dotnet add '.\TabPlayer.csproj' reference .\TabPlayer.SomeImportantStuff\TabPlayer.SomeImportantStuff.csproj
```
