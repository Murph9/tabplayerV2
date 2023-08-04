using Godot;
using murph9.TabPlayer.Songs.Models;

public class NoteGenerator {
    public static Node3D GetBasicNote(SingleNote note, InstrumentConfig config, NoteBlock noteBlock) {
        var notePosZ = DisplayConst.CalcInFretPosZ(note.FretNum);
        var notePos = new Vector3(noteBlock.Time * config.NoteSpeed, note.StringNum * DisplayConst.STRING_DISTANCE_APART, notePosZ);

        var colour = note.StringNum != 255 ? DisplayConst.GetColorFromString(note.StringNum) : Colors.HotPink;
        
        if (note.FretNum == 0) {
            // open note
            var lineStartZ = DisplayConst.CalcFretPosZ(noteBlock.FretWindowStart-1);
            var across = new Vector3(0, 0, DisplayConst.CalcFretWidthZ(noteBlock.FretWindowStart, noteBlock.FretWindowLength));
            var lineStart = new Vector3(noteBlock.Time * config.NoteSpeed, note.StringNum * DisplayConst.STRING_DISTANCE_APART, -lineStartZ);

            return MeshGenerator.BoxLine(colour, lineStart, lineStart + across);
        }
        
        // normal note
        return MeshGenerator.Box(colour, notePos);
    }

    public static IEnumerable<Node3D> GetNote(SingleNote note, InstrumentConfig config, NoteBlock noteBlock) {
        var notePosZ = DisplayConst.CalcInFretPosZ(note.FretNum);
        var notePos = new Vector3(noteBlock.Time * config.NoteSpeed, note.StringNum * DisplayConst.STRING_DISTANCE_APART, notePosZ);

        var colour = note.StringNum != 255 ? DisplayConst.GetColorFromString(note.StringNum) : Colors.HotPink;
        if (!note.Type.Contains(NoteType.CHILD)) {
            if (note.FretNum == 0) {
                // open note
                var lineStartZ = DisplayConst.CalcFretPosZ(noteBlock.FretWindowStart - 1);
                var across = new Vector3(0, 0, DisplayConst.CalcFretWidthZ(noteBlock.FretWindowStart, noteBlock.FretWindowLength));
            
                var start = new Vector3(noteBlock.Time * config.NoteSpeed, note.StringNum * DisplayConst.STRING_DISTANCE_APART, lineStartZ);
                yield return MeshGenerator.BoxLine(colour, start, start + across);
            } else {
                // normal note
                yield return MeshGenerator.Box(colour, notePos);
            }
        }

        var noteText = string.Join(string.Empty, NoteHelper.GetSymbol(note));
        if (noteText.Any()) {
            if (note.FretNum == 0) {
                notePos = new Vector3(notePos.X, notePos.Y, DisplayConst.CalcMiddleWindowZ(noteBlock.FretWindowStart, noteBlock.FretWindowLength));
            }

            var text = MeshGenerator.TextVertical(noteText, notePos - new Vector3(0.6f, 0, 0));
            // TODO text.Colour = Color4.Black;
            yield return text;
        }
    }

    public static IEnumerable<Node3D> CreateNoteLine(NoteBlock noteBlock, SingleNote note, InstrumentConfig config) {
        var notePosZ = DisplayConst.CalcInFretPosZ(note.FretNum);
        var notePos = new Vector3(noteBlock.Time * config.NoteSpeed, note.StringNum * DisplayConst.STRING_DISTANCE_APART, notePosZ);

        if (note.FretNum == 0) {
            notePos = new Vector3(notePos.X, notePos.Y, DisplayConst.CalcMiddleWindowZ(noteBlock.FretWindowStart, noteBlock.FretWindowLength));
        }

        if (!note.Type.Contains(NoteType.SUSTAIN)) {
            yield break;
        }
        
        if (note.Length == 0) {
            Console.WriteLine("Found sustain without a length: " + noteBlock.Time);
            yield break;
        }

        if (note.Type.Contains(NoteType.BEND) && note.Bends.Any()) {
            var lastPos = notePos;
            if (Math.Abs(note.Bends.First().Time - noteBlock.Time) > 1e-3) {
                lastPos = notePos; // TODO wrong
            }
            
            foreach (var b in note.Bends) {
                var endPos = new Vector3(b.Time*config.NoteSpeed, notePos.Y + DisplayConst.STRING_DISTANCE_APART*b.Step, notePos.Z);
                yield return MeshGenerator.BoxLine(DisplayConst.GetColorFromString(note.StringNum), lastPos, endPos);
                lastPos = endPos;
            }

        } else if (note.Type.Contains(NoteType.SLIDE)) {
            if (note.Slide == null || !note.Slide.HasValue)
                throw new Exception("A slide without a slide"); // TODO should really have not passed validation
            var endPos = new Vector3((noteBlock.Time + note.Length) * config.NoteSpeed, notePos.Y, DisplayConst.CalcInFretPosZ(note.Slide.Value.ToFret));
            yield return MeshGenerator.BoxLine(DisplayConst.GetColorFromString(note.StringNum), notePos, endPos);

        } else {
            yield return MeshGenerator.BoxLine(DisplayConst.GetColorFromString(note.StringNum), notePos, notePos + new Vector3(config.NoteSpeed*note.Length, 0, 0));
        }
    }
}