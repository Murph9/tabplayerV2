using Godot;
using murph9.TabPlayer.Songs.Models;

public partial class SongChart : Node3D {
    
    private NoteBlock _lastChord;
    private Instrument _instrument;

    public void Init(Instrument instrument) {
        _instrument = instrument;
    }

    public override void _Ready()
	{
        var itemList = _init(_instrument);
        var root = GetTree().Root;
        foreach (var item in itemList) {
            root.AddChild(item);
        }
	}

	public override void _Process(double delta)
	{
	}

    private IEnumerable<Node3D> _init(Instrument instrument) {
        var fretNumLastPlacedMap = new Dictionary<int, float>();
        
        foreach (var noteBlock in instrument.Notes) {
            if (noteBlock.Notes.Count() > 1) {
                foreach (var x in Chord(noteBlock, instrument.Config)) yield return x;
                _lastChord = noteBlock;
            } else {
                foreach (var x in SingleNote(noteBlock, instrument.Config)) yield return x;
                _lastChord = null;
            }
            
            foreach (var note in noteBlock.Notes.Reverse()) { // reverse because we want the numbers to above the notes (and notes block otherwise)
                if (note.FretNum == 0 || note.FretNum > 30)
                    continue;

                if (!fretNumLastPlacedMap.ContainsKey(note.FretNum))
                    fretNumLastPlacedMap[note.FretNum] = -10;
                
                // put some numbers on the track where notes are for reading
                if (Math.Abs(noteBlock.Time - fretNumLastPlacedMap[note.FretNum]) > 0.6f) {
                    fretNumLastPlacedMap[note.FretNum] = noteBlock.Time;
                    var zPos = DisplayConst.CalcInFretPosZ(note.FretNum);
                    yield return MeshGenerator.TextVertical(note.FretNum.ToString(), new Vector3(noteBlock.Time * instrument.Config.NoteSpeed, DisplayConst.STRING_DISTANCE_APART*note.StringNum + 1, zPos));
                }
            }
        }

        foreach (var x in GenerateNoteBlockFrets(instrument)) yield return x;
    }

    private IEnumerable<Node3D> Chord(NoteBlock noteBlock, InstrumentConfig config) {
        if (noteBlock.Notes.Count() < 2)
            throw new ArgumentException(nameof(noteBlock), "must have mote than one note");
        
        var list = new List<Node3D>();
        
        var lineStartZ = DisplayConst.CalcFretPosZ(noteBlock.FretWindowStart-1);
        var across = new Vector3(0, 0, DisplayConst.CalcFretWidthZ(noteBlock.FretWindowStart, noteBlock.FretWindowLength));
        var bottomLeftPos = new Vector3(noteBlock.Time * config.NoteSpeed, DisplayConst.TRACK_BOTTOM_WORLD + 0.01f, lineStartZ);
        var chordDirUp = new Vector3(0,1,0) * 6 * DisplayConst.STRING_DISTANCE_APART;

        // generate the notes only if the chord is new (compared with the last one)
        if (noteBlock.ChordFlags.Contains(NoteBlockFlags.MUTE)) {
            var lineA = MeshGenerator.BoxLine(Colors.LightGray, bottomLeftPos, bottomLeftPos + across + chordDirUp*0.5f);
            list.Add(lineA);
            var lineB = MeshGenerator.BoxLine(Colors.LightGray, bottomLeftPos + across, bottomLeftPos + chordDirUp*0.5f);
            list.Add(lineB);
        }
        else if (!noteBlock.IsSameChordAs(_lastChord)) {
            foreach (var note in noteBlock.Notes) {
                list.AddRange(NoteGenerator.GetNote(note, config, noteBlock));
                list.AddRange(NoteGenerator.CreateNoteLine(noteBlock, note, config));
            }
            
            // display label of the first chord in sequence
            if (!string.IsNullOrWhiteSpace(noteBlock.Label)) {
                var pos = new Vector3(noteBlock.Time * config.NoteSpeed, 7 * DisplayConst.STRING_DISTANCE_APART, DisplayConst.CalcInFretPosZ(noteBlock.FretWindowStart));
                list.Add(MeshGenerator.TextVertical(noteBlock.Label, pos));
            }
        }
        
        list.Add(MeshGenerator.BoxLine(Colors.LightGray, bottomLeftPos + chordDirUp, bottomLeftPos));
        list.Add(MeshGenerator.BoxLine(Colors.LightGray, bottomLeftPos, bottomLeftPos + across));
        list.Add(MeshGenerator.BoxLine(Colors.LightGray, bottomLeftPos + across, bottomLeftPos + across + chordDirUp));
        return list;
    }

    private IEnumerable<Node3D> SingleNote(NoteBlock noteBlock, InstrumentConfig config) {
        if (noteBlock.Notes.Count() != 1)
            throw new ArgumentException(nameof(noteBlock), "must have only 1 note for this method");
        
        var note = noteBlock.Notes.First();

        foreach (var o in NoteGenerator.GetNote(note, config, noteBlock)) yield return o;
        foreach (var o in NoteGenerator.CreateNoteLine(noteBlock, note, config)) yield return o;

        if (note.FretNum != 0) {
            var notePos = new Vector3(noteBlock.Time * config.NoteSpeed, note.StringNum * DisplayConst.STRING_DISTANCE_APART, DisplayConst.CalcInFretPosZ(note.FretNum));

            // vertical line for timing
            var dir = new Vector3(0, notePos.Y - DisplayConst.TRACK_BOTTOM_WORLD, 0);

            var start = new Vector3(notePos.X, DisplayConst.TRACK_BOTTOM_WORLD, notePos.Z);
            yield return MeshGenerator.BoxLine(DisplayConst.GetColorFromString(note.StringNum), start, start + dir);

            // and a horizontal one for position
            var pos = new Vector3(notePos.X, DisplayConst.TRACK_BOTTOM_WORLD+0.01f, DisplayConst.CalcFretPosZ(note.FretNum-1));
            var across = new Vector3(0, 0, DisplayConst.CalcFretWidthZ(note.FretNum));
            yield return MeshGenerator.BoxLine(DisplayConst.GetColorFromString(note.StringNum), pos, pos + across);
        }
    }
    
    private IEnumerable<Node3D> GenerateNoteBlockFrets(Instrument instrument) {
        float startOfCurSection = -10;
        int curStart = -1;
        int curLength = -1;
        foreach (var note in instrument.Notes) {
            if (curStart != note.FretWindowStart || curLength != note.FretWindowLength) {
                if (curStart > 0) {
                    while (startOfCurSection < note.Time) { // don't let them get too big
                        var length = Math.Min(note.Time - startOfCurSection, 10);
                        yield return CreateWindowPiece(curStart, curLength, startOfCurSection + length, startOfCurSection, instrument.Config);
                        startOfCurSection += length;
                    }
                }
                curStart = note.FretWindowStart;
                curLength = note.FretWindowLength;
                startOfCurSection = note.Time;
            }
        }
        // finallize the last note's one
        if (curStart > 0) {
            var lastNote = instrument.Notes.Last();
            while (startOfCurSection < lastNote.Time + 1) { // don't let them get too big
                var length = Math.Min(lastNote.Time + 1 - startOfCurSection, 10);
                yield return CreateWindowPiece(curStart, curLength, startOfCurSection + length, startOfCurSection, instrument.Config);
                startOfCurSection += length;
            }
        }
    }
    
    private static Node3D CreateWindowPiece(int fret, int length, float endTime, float startTime, InstrumentConfig config) {
        var across = DisplayConst.CalcFretWidthZ(fret, length);
        var pos = new Vector3((endTime + startTime)/2*config.NoteSpeed - 0.5f, DisplayConst.TRACK_BOTTOM_WORLD-0.01f, DisplayConst.CalcFretPosZ(fret - 1) + across/2); //avg
        return MeshGenerator.Plane(Colors.DarkSlateBlue, pos, new Vector2(config.NoteSpeed*(endTime - startTime), across));
    }
}
