using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
using Newtonsoft.Json;
using System;

namespace murph9.TabPlayer.scenes;

public partial class SongList : Control
{
	class Column {
		public string Name { get; private set; }
		public bool Sorted => Sort != null;
		public Func<SongFile, object> Sort { get; private set; }
		public Button Button { get; set; }
		public Column(string name, Func<SongFile, object> value) {
			Name = name;
			Sort = value;
		}
	}

	class Row {
		public SongFile Song { get; private set; }
		public List<Control> Controls { get; private set; }
		public Row(SongFile song, Action buttonAction) {
			Song = song;

			Controls = new List<Control>();
			var b = new Button() {
				Text = "▶"
			};
			b.Pressed += buttonAction;
			Controls.Add(b);
			Controls.Add(new Label() {
				Text = song.SongName.FixedWidthString(30)
			});
			Controls.Add(new Label() {
				Text = song.Artist.FixedWidthString(24)
			});
			Controls.Add(new Label() {
				Text = song.Album.FixedWidthString(20)
			});
			Controls.Add(new Label() {
				Text = song.Year.ToString()
			});
			Controls.Add(new Label() {
				Text = song.Length.ToMinSec()
			});
			Controls.Add(new Label() {
				Text = song.GetInstrumentChars()
			});

			var mainI = song.GetMainInstrument();
			Controls.Add(new Label() { Text = mainI?.Name });
			Controls.Add(new Label() { Text = Instrument.CalcTuningName(mainI?.Tuning) });
			Controls.Add(new Label() { Text = mainI?.NoteCount.ToString() });
			Controls.Add(new Label() { Text = mainI?.GetNoteDensity(song).ToFixedPlaces(2, false) });
			var otherInstruments = song.Instruments.Where(x => x != song.GetMainInstrument()).Take(2);
			foreach (var otherI in otherInstruments) {
				Controls.Add(new Label() {
					Text = otherI.Name + ": " + Instrument.CalcTuningName(otherI.Tuning, otherI.CapoFret)
				});
			}
			for (int i = 0; i < 2 - otherInstruments.Count(); i++) {
				Controls.Add(new Label());
			}
		}
	}

	private static readonly List<Column> HEADINGS = new() {
		new Column(null, null),
		new Column("Song Name", (s) => s.SongName),
		new Column("Artist", (s) => s.Artist),
		new Column("Album", (s) => s.Album),
		new Column("Year", (s) => s.Year),
		new Column("Length", (s) => s.Length),
		new Column("Parts", (s) => s.GetInstrumentChars()),
		new Column("Main", (s) => s.GetMainInstrument()?.Name),
		new Column("Tuning", (s) => Instrument.CalcTuningName(s.GetMainInstrument()?.Tuning, s.GetMainInstrument()?.CapoFret)),
		new Column("Notes", (s) => s.GetMainInstrument()?.GetNoteDensity(s)),
		new Column("Density", (s) => s.GetMainInstrument()?.NoteCount),
		new Column(null, null),
		new Column(null, null),
	};

	private SongFile _selectedSong;

	private readonly List<Row> _rows;
	private Func<SongFile, object> _sort = (s) => s.SongName;
	private Func<SongFile, bool> _filter = null;
	private string _tuningFilter = null;

	// TODO don't unload this

	public SongList() {
		var songList = SongFileManager.GetSongFileList((str) => {});
		_rows = songList.Data.ToArray().Select(x => new Row(x, () => ItemActivated(x))).ToList();
	}

	public override void _Ready()
	{
		var group = new ButtonGroup();

		var grid = GetNode<GridContainer>("VBoxContainer/ScrollContainer/GridContainer");
		grid.Columns = HEADINGS.Count;
		foreach (var heading in HEADINGS) {
			if (heading.Sorted) {
                var b = new Button
                {
                    Text = heading.Name,
                    ButtonGroup = group,
                    ToggleMode = true
                };
                grid.AddChild(b);
				heading.Button = b;
			} else {
				grid.AddChild(new Label() {
					Text = heading.Name
				});
			}
		}
		group.Pressed += Heading_Pressed;

		var tuningSelect = GetNode<OptionButton>("VBoxContainer/HBoxContainer/TuningOptionButton");
		tuningSelect.AddItem("");
		foreach (var tuning in _rows.Select(x => Instrument.CalcTuningName(x.Song.GetMainInstrument()?.Tuning)).Distinct()) {
			tuningSelect.AddItem(tuning);
		}

		LoadTableRows();
		LoadTableFilter();
	}

	public override void _Process(double delta)
	{
	}

	public void ItemActivated(SongFile selectedSong) {
		_selectedSong = selectedSong;

		GD.Print(_selectedSong.SongName);
		
		var panel = GetNode<PopupPanel>("PopupPanel");
		panel.GetChildren().ToList().ForEach(panel.RemoveChild);

		var layout = new VBoxContainer();
		panel.AddChild(layout);

		layout.AddChild(new Label() {
			Text = $"Select an instrument for: {selectedSong.SongName} by {selectedSong.Artist}"
		});

		foreach (var i in _selectedSong.Instruments) {
			var b = new Button() {
				Text = i.Name + " | Tuning:" + Instrument.CalcTuningName(i.Tuning, i.CapoFret) + " | Note Count:" + i.NoteCount
			};
			b.Pressed += () => SelectedItem_LoadSong(i.Name);
			layout.AddChild(b);
		}

		panel.PopupCentered();
	}

	public void Back() {
		GetTree().ChangeSceneToFile("res://StartMenu.tscn");
	}

	public void TuningSelected(int index) {
		var tuningSelect = GetNode<OptionButton>("VBoxContainer/HBoxContainer/TuningOptionButton");
		var record = tuningSelect.GetItemText(index);
		_tuningFilter = record;
		LoadTableFilter();
	}

	private void SelectedItem_LoadSong(string name) {
		var songState = SongLoader.Load(new DirectoryInfo(Path.Combine(SongFileManager.SONG_FOLDER, _selectedSong.FolderName)), name);

		PackedScene packedScene = ResourceLoader.Load<PackedScene>("res://scenes/SongScene.tscn");
		var scene = packedScene.Instantiate<SongScene>();
		scene._init(songState);
		
		GetTree().Root.AddChild(scene);
		QueueFree();
	}

	private void LoadTableRows() {
		var grid = GetNode<GridContainer>("VBoxContainer/ScrollContainer/GridContainer");
		foreach (var row in _rows.OrderBy(x => _sort(x.Song))) {
			foreach (var control in row.Controls) {
				if (control.GetParent() != null)
					grid.RemoveChild(control);
				grid.AddChild(control);
			}
		}
	}

	private void LoadTableFilter() {
		foreach (var row in _rows) {
			var tuningEnabled = string.IsNullOrEmpty(_tuningFilter) || Instrument.CalcTuningName(row.Song.GetMainInstrument().Tuning) == _tuningFilter;
			var enabled = _filter == null || _filter(row.Song);
			foreach (var control in row.Controls) {
				control.Visible = enabled && tuningEnabled;
			}
		}
	}

	private void Heading_Pressed(BaseButton b) {
        if (b is not Button button) return;
        GD.Print(button.Text);

		var heading = HEADINGS.Single(x => button == x.Button);
		_sort = heading.Sort;

		LoadTableRows();
	}

	private void ResetFilter_Pressed() {
		var textFilter = GetNode<LineEdit>("VBoxContainer/HBoxContainer/FilterLineEdit");
		textFilter.Text = null;
		_filter = null;
		LoadTableFilter();
	}

	private void UpdateFilter(string filter) {
		_filter = x => {
			return x.Artist.Contains(filter, StringComparison.InvariantCultureIgnoreCase)
			|| x.SongName.Contains(filter, StringComparison.InvariantCultureIgnoreCase)
			|| x.Album.Contains(filter, StringComparison.InvariantCultureIgnoreCase);
		};

		LoadTableFilter();
	}

	public void SelectRandom() {
		var validSongs = _rows.Where(x => x.Controls.Any(x => x.Visible)).ToArray();
		if (!validSongs.Any()) 
			return;

		var index = new Random().NextInt64(0, validSongs.Length);
		var song = validSongs[index];
		
		ItemActivated(song.Song);
	}
}
