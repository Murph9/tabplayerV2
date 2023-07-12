using Rocksmith2014PsarcLib.ReaderExtensions;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Rocksmith2014PsarcLib.Psarc.Models.Sng
{
    public struct Anchor
    {
        public float StartBeatTime;
        public float EndBeatTime;
        public float Unk3_FirstNoteTime;
        public float Unk4_LastNoteTime;
        public byte FretId;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Padding;

        public int Width;
        public int PhraseIterationId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AnchorExtension
    {
        public float BeatTime;
        public byte FretId;
        public int Unk2_0;
        public short Unk3_0;
        public byte Unk4_0;
    }

    public struct Fingerprint
    {
        public int ChordId;
        public float StartTime;
        public float EndTime;
        public float Unk3_FirstNoteTime;
        public float Unk4_LastNoteTime;
    }

    public struct Note : IBinarySerializable
    {
        public uint NoteMask;
        public uint NoteFlags;
        public uint Hash;
        public float Time;
        public byte StringIndex;
        public byte FretId;
        public byte AnchorFretId;
        public byte AnchorWidth;
        public int ChordId;
        public int ChordNotesId;
        public int PhraseId;
        public int PhraseIterationId;
        public short[] FingerPrintId;
        public short NextIterNote;
        public short PrevIterNote;
        public short ParentPrevNote;
        public byte SlideTo;
        public byte SlideUnpitchTo;
        public byte LeftHand;
        public byte Tap;
        public byte PickDirection;
        public byte Slap;
        public byte Pluck;
        public short Vibrato;
        public float Sustain;
        public float MaxBend;
        public BendData32[] BendData;

        public NoteMaskFlag Flags;

        public IBinarySerializable Read(BinaryReader reader)
        {
            NoteMask = reader.ReadUInt32();
            Flags = (NoteMaskFlag)this.NoteMask;

            NoteFlags = reader.ReadUInt32();
            Hash = reader.ReadUInt32();
            Time = reader.ReadSingle();
            StringIndex = reader.ReadByte();
            FretId = reader.ReadByte();
            AnchorFretId = reader.ReadByte();
            AnchorWidth = reader.ReadByte();
            ChordId = reader.ReadInt32();
            ChordNotesId = reader.ReadInt32();
            PhraseId = reader.ReadInt32();
            PhraseIterationId = reader.ReadInt32();

            FingerPrintId = reader.ReadShortArray(2);

            NextIterNote = reader.ReadInt16();
            PrevIterNote = reader.ReadInt16();
            ParentPrevNote = reader.ReadInt16();
            SlideTo = reader.ReadByte();
            SlideUnpitchTo = reader.ReadByte();
            LeftHand = reader.ReadByte();
            Tap = reader.ReadByte();
            PickDirection = reader.ReadByte();
            Slap = reader.ReadByte();
            Pluck = reader.ReadByte();
            Vibrato = reader.ReadInt16();
            Sustain = reader.ReadSingle();
            MaxBend = reader.ReadSingle();
            BendData = reader.ReadStructArray<BendData32>();

            return this;
        }

        [Flags]
        public enum NoteMaskFlag : uint {
            // https://github.com/rscustom/rocksmith-custom-song-toolkit/blob/master/RocksmithToolkitLib/Sng/Constants.cs
            UNDEFINED         = 0x00,
            MISSING           = 0x01,
            CHORD             = 0x02,
            OPEN              = 0x04,
            FRETHANDMUTE      = 0x08,
            TREMOLO           = 0x10,
            HARMONIC          = 0x20,
            PALMMUTE          = 0x40,
            SLAP              = 0x80,
            PLUCK             = 0x0100,
            POP               = 0x0100,
            HAMMERON          = 0x0200,
            PULLOFF           = 0x0400,
            SLIDE             = 0x0800,
            BEND              = 0x1000,
            SUSTAIN           = 0x2000,
            TAP               = 0x4000,
            PINCHHARMONIC     = 0x8000,
            VIBRATO           = 0x010000,
            MUTE              = 0x020000,
            IGNORE            = 0x040000,   // ignore=1
            LEFTHAND          = 0x00080000,
            RIGHTHAND         = 0x00100000,
            HIGHDENSITY       = 0x200000,
            SLIDEUNPITCHEDTO  = 0x400000,
            SINGLE            = 0x00800000, // single note
            CHORDNOTES        = 0x01000000, // has chordnotes exported
            DOUBLESTOP        = 0x02000000,
            ACCENT            = 0x04000000,
            PARENT            = 0x08000000, // linkNext=1
            CHILD             = 0x10000000, // note after linkNext=1
            ARPEGGIO          = 0x20000000,
            MISSING2          = 0x40000000,
            STRUM             = 0x80000000, // handShape defined at chord time
        }
    }

    public struct Arrangement : IBinarySerializable
    {
        public int Difficulty;
        public Anchor[] Anchors;
        public AnchorExtension[] AnchorExtensions;
        public Fingerprint[] Fingerprints1;
        public Fingerprint[] Fingerprints2;
        public Note[] Notes;

        public int PhraseCount;
        public float[] AverageNotesPerIteration;
        public int PhraseIterationCount1;
        public int[] NotesInIteration1;
        public int PhraseIterationCount2;
        public int[] NotesInIteration2;

        public IBinarySerializable Read(BinaryReader reader)
        {
            Difficulty = reader.ReadInt32();

            Anchors = reader.ReadStructArray<Anchor>();
            AnchorExtensions = reader.ReadStructArray<AnchorExtension>();
            Fingerprints1 = reader.ReadStructArray<Fingerprint>();
            Fingerprints2 = reader.ReadStructArray<Fingerprint>();
            Notes = reader.ReadStructArray<Note>();

            PhraseCount = reader.ReadInt32();
            AverageNotesPerIteration = reader.ReadFloatArray(PhraseCount);

            PhraseIterationCount1 = reader.ReadInt32();
            NotesInIteration1 = reader.ReadIntArray(PhraseIterationCount1);

            PhraseIterationCount2 = reader.ReadInt32();
            NotesInIteration2 = reader.ReadIntArray(PhraseIterationCount2);

            return this;
        }
    }
}
