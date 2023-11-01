using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using murph9.TabPlayer.Songs.Models;

namespace murph9.TabPlayer.scenes.Services;

public class NoteGenerator {
    public static Node3D GetBasicNote(SingleNote note, InstrumentConfig config, float time, int fretWindowStart, int fretWindowLength) {
        var notePosZ = DisplayConst.CalcInFretPosZ(note.FretNum);
        var notePos = new Vector3(time * config.NoteSpeed, DisplayConst.CalcNoteHeightY(note.StringNum), notePosZ);

        var colour = note.StringNum != 255 ? SettingsService.GetColorFromStringNum(note.StringNum) : Colors.HotPink;
        
        if (note.FretNum == 0) {
            // open note
            var lineStartZ = DisplayConst.CalcFretPosZ(fretWindowStart - 1);
            var across = new Vector3(0, 0, DisplayConst.CalcFretWidthZ(fretWindowStart, fretWindowLength));
            var lineStart = new Vector3(time * config.NoteSpeed, DisplayConst.CalcNoteHeightY(note.StringNum), lineStartZ);

            return MeshGenerator.BoxLine(colour, lineStart, lineStart + across);
        }
        
        // normal note
        return MeshGenerator.Box(colour, notePos);
    }

    public static IEnumerable<Node3D> GetNote(SingleNote note, InstrumentConfig config, NoteBlock noteBlock) {
        var notePosZ = DisplayConst.CalcInFretPosZ(note.FretNum);
        var notePos = new Vector3(noteBlock.Time * config.NoteSpeed, DisplayConst.CalcNoteHeightY(note.StringNum), notePosZ);

        var colour = note.StringNum != 255 ? SettingsService.GetColorFromStringNum(note.StringNum) : Colors.HotPink;
        if (!note.Type.Contains(NoteType.CHILD)) {
            if (note.FretNum == 0) {
                // open note
                var lineStartZ = DisplayConst.CalcFretPosZ(noteBlock.FretWindowStart - 1);
                var across = new Vector3(0, 0, DisplayConst.CalcFretWidthZ(noteBlock.FretWindowStart, noteBlock.FretWindowLength));
            
                var start = new Vector3(noteBlock.Time * config.NoteSpeed, DisplayConst.CalcNoteHeightY(note.StringNum), lineStartZ);
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
        var notePos = new Vector3(noteBlock.Time * config.NoteSpeed, DisplayConst.CalcNoteHeightY(note.StringNum), DisplayConst.CalcInFretPosZ(note.FretNum));

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

        var noteColour = SettingsService.GetColorFromStringNum(note.StringNum);
        var finalLinePos = notePos + new Vector3(config.NoteSpeed*note.Length, 0, 0);
        if (note.Type.Contains(NoteType.BEND) && note.Bends.Any()) {
            var lastPos = notePos;
            if (Math.Abs(note.Bends.First().Time - noteBlock.Time) > 1e-3) {
                lastPos = notePos; // TODO probably wrong
            }
            
            foreach (var b in note.Bends) {
                var endPos = new Vector3(b.Time*config.NoteSpeed, notePos.Y + DisplayConst.STRING_DISTANCE_APART*b.Step, notePos.Z);
                yield return MeshGenerator.BoxLine(noteColour, lastPos, endPos);
                lastPos = endPos;
            }

            if (noteBlock.Time + note.Length > note.Bends.Last().Time) {
                var endPos2 = new Vector3((noteBlock.Time + note.Length)*config.NoteSpeed, notePos.Y, notePos.Z);
                yield return MeshGenerator.BoxLine(noteColour, lastPos, endPos2);
            }
            finalLinePos = lastPos;

        } else if (note.Type.Contains(NoteType.SLIDE) || note.Type.Contains(NoteType.SLIDEUNPITCHEDTO)) {
            if (note.Slide == null || !note.Slide.HasValue)
                throw new Exception("A slide without a slide"); // TODO should really have not passed validation
            var endPos = new Vector3((noteBlock.Time + note.Length) * config.NoteSpeed, notePos.Y, DisplayConst.CalcInFretPosZ(note.Slide.Value.ToFret));
            yield return MeshGenerator.BoxLine(noteColour, notePos, endPos);

            finalLinePos = endPos;

        } else {
            if (!note.Type.Contains(NoteType.VIBRATO)) {
                yield return MeshGenerator.BoxLine(noteColour, notePos, notePos + new Vector3(config.NoteSpeed*note.Length, 0, 0));
            } else {
                // wibble and wobble the line (on both axis for visual help)
                int wibbleWobble = 0;
                var curX = notePos.X;
                var lastPos = notePos;
                while (curX < finalLinePos.X) {
                    curX += 1.5f;
                    var curPos = new Vector3(curX, notePos.Y + (float)Math.Sin(wibbleWobble * Math.PI / 2f) * 0.4f, notePos.Z + (float)Math.Cos(wibbleWobble * Math.PI / 2f) * 0.2f);
                    yield return MeshGenerator.BoxLine(noteColour, lastPos, curPos);
                    wibbleWobble += 1;
                    lastPos = curPos;
                }
            }
        }

        if (note.Type.Contains(NoteType.TREMOLO)) {
            // place notes along the line to display that we play it fast
            var curPos = notePos;
            while (curPos.X < finalLinePos.X) {
                var b = MeshGenerator.Box(noteColour, curPos);
                b.Scale *= 0.45f;
                yield return b;
                curPos += new Vector3(1.25f, 0, 0).Project(finalLinePos - notePos);
            }
        }
    }
}
