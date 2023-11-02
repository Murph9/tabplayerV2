using Godot;
using murph9.TabPlayer.scenes.Services;
using murph9.TabPlayer.Songs;
using murph9.TabPlayer.Songs.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace murph9.TabPlayer.scenes;

public partial class SongList : VBoxContainer
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
		private static readonly StyleBox ROW_HIGHLIGHTED = new StyleBoxFlat
		{
			BgColor = Colors.Gray
		};

		private readonly Action<SongFile> _callback;
		public SongFile Song { get; }
		public List<Control> Controls { get; }
        private bool _selected;
		public bool Selected {
			get { return _selected; }
			set {
				_selected = value;
				if (_selected) {
                    Controls.ForEach(x => x.AddThemeStyleboxOverride("normal", ROW_HIGHLIGHTED));
				} else {
					Controls.ForEach(x => x.RemoveThemeStyleboxOverride("normal"));
				}
			}
		}

        public Row(SongFile song, Action<SongFile> buttonAction) {
			Song = song;
			_callback = buttonAction;

			Controls = new List<Control>
            {
                CreateLabel(song.SongName.FixedWidthString(30)),
                CreateLabel(song.Artist.FixedWidthString(24)),
                CreateLabel(song.Album.FixedWidthString(20)),
                CreateLabel(song.Year.ToString()),
                CreateLabel(song.Length.ToMinSec()),
                CreateLabel(song.GetInstrumentChars())
            };
		}

		private Label CreateLabel(string text) {
			var l = new Label() {
				Text = text,
				MouseFilter = MouseFilterEnum.Stop
			};
			l.GuiInput += RowSelectedForReal;
			return l;
		}

        private void RowSelectedForReal(InputEvent @event)
        {
            if (@event is InputEventMouseButton e)
				if (e.ButtonIndex == MouseButton.Left && e.Pressed)
					_callback(Song);
        }
	}
	
	private static readonly List<Column> HEADINGS = new() {
		new Column("Song Name", (s) => s.SongName),
		new Column("Artist", (s) => s.Artist),
		new Column("Album", (s) => s.Album),
		new Column("Year", (s) => s.Year),
		new Column("Length", (s) => s.Length),
		new Column("Parts", (s) => s.GetInstrumentChars())
	};

	private readonly List<Row> _rows;
	private Func<SongFile, object> _sort = (s) => s.SongName;
	private Func<SongFile, bool> _filter = null;
	private string _tuningFilter = null;
	public string TuningFilter => _tuningFilter;

	private SongDisplay _songDisplay;

	[Signal]
	public delegate void SongSelectedEventHandler(string folder);

	public SongList() {
		var songList = SongFileManager.GetSongFileList();
		_rows = songList.Data.ToArray().Select(x => new Row(x, RowSelected)).ToList();
	}

	public void SetDisplay(SongDisplay songDisplay) {
		_songDisplay = songDisplay;
	}

	private void RowSelected(SongFile selectedSong) {
		var selectedRow = _rows.FirstOrDefault(x => x.Song == selectedSong);
		selectedRow.Selected = true;
		foreach (var r in _rows) {
			if (r != selectedRow && r.Selected) {
				r.Selected = false;
			}
		}
		EmitSignal(SignalName.SongSelected, selectedSong.FolderName);
	}

	public override void _Ready()
	{
		var split = GetNode<VBoxContainer>("HSplitContainer/VBoxContainerDetails");
		split.AddChild(_songDisplay);

		var group = new ButtonGroup();

		var grid = GetNode<GridContainer>("%GridContainer");
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

		var tuningSelect = GetNode<OptionButton>("HBoxContainer/TuningOptionButton");
		tuningSelect.AddItem("");
		foreach (var tuning in _rows.Select(x => Instrument.CalcTuningName(x.Song.GetMainInstrument()?.Tuning)).Distinct()) {
			tuningSelect.AddItem(tuning);
		}

		LoadTableRows();
		LoadTableFilter();

		if (!_rows.Any()) {
			var label = new Label() {
				Text = "No songs found :(, please convert some at the main menu"
			};
			grid.AddSibling(label);
			grid.Visible = false;
		}
	}

	public override void _Process(double delta) { }

	private void TuningSelected(int index) {
		var tuningSelect = GetNode<OptionButton>("HBoxContainer/TuningOptionButton");
		var record = tuningSelect.GetItemText(index);
		_tuningFilter = record;
		LoadTableFilter();
	}

	private void LoadTableRows() {
		var grid = GetNode<GridContainer>("%GridContainer");
		foreach (var row in _rows.OrderBy(x => _sort(x.Song))) {
			foreach (var control in row.Controls) {
				if (control.GetParent() != null)
					grid.RemoveChild(control);
				grid.AddChild(control);
			}
		}
	}

	private void LoadTableFilter() {
		var capoShown = GetNode<CheckBox>("HBoxContainer/CapoCheckBox").ButtonPressed;

		int countShown = 0;
		foreach (var row in _rows) {
			var instrument = row.Song.GetMainInstrument();
			var tuningEnabled = (string.IsNullOrEmpty(_tuningFilter) || Instrument.CalcTuningName(instrument.Tuning) == _tuningFilter)
				&& (instrument.CapoFret == 0 || (capoShown && instrument.CapoFret != 0));
			
			var enabled = _filter == null || _filter(row.Song);
			foreach (var control in row.Controls) {
				control.Visible = enabled && tuningEnabled;
			}
			if (enabled) {
				countShown++;
			}
		}
		
		var songCountLabel = GetNode<Label>("HBoxContainer/SongsLoadedLabel");
		songCountLabel.Text = countShown + " songs shown";
	}

	private void Heading_Pressed(BaseButton b) {
        if (b is not Button button) return;

		var heading = HEADINGS.Single(x => button == x.Button);
		_sort = heading.Sort;

		LoadTableRows();
	}

	private void ResetFilter_Pressed() {
		var textFilter = GetNode<LineEdit>("HBoxContainer/FilterLineEdit");
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
		
		RowSelected(song.Song);
		// TODO scroll
	}

	private void ShowCapo_Pressed() {
		LoadTableFilter();
	}
}
