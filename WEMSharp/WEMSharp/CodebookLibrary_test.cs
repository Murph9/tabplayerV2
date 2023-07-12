/*using WEMSharp;

namespace CodebookLibrary.Test
{
    public class CodebookLibrary
    {
        private char[] _codebookData;
        private long[] _codebookOffsets;
        private int codebookCount;

        public CodebookLibrary()
        {
            _codebookData = null;
            _codebookOffsets = null;
            codebookCount = 0;
        }

        public CodebookLibrary(string filename)
        {
            _codebookData = null;
            _codebookOffsets = null;
            codebookCount = 0;

            using (var isStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                long fileSize = isStream.Length;

                isStream.Seek(fileSize - 4, SeekOrigin.Begin);
                int offsetOffset = Read32LE(isStream);
                codebookCount = (int)((fileSize - offsetOffset) / 4);

                _codebookData = new char[offsetOffset];
                _codebookOffsets = new long[codebookCount];

                isStream.Seek(0, SeekOrigin.Begin);
                for (int i = 0; i < offsetOffset; i++)
                {
                    _codebookData[i] = (char)isStream.ReadByte();
                }

                for (int i = 0; i < codebookCount; i++)
                {
                    _codebookOffsets[i] = Read32LE(isStream);
                }
            }
        }

        public void Rebuild(int i, OggStream bos)
        {
            string cb = GetCodebook(i);
            ulong cbSize;

            {
                long signedCbSize = GetCodebookSize(i);

                if (cb == null || signedCbSize == -1)
                {
                    throw new Exception("Invalid id: " + i);
                }

                cbSize = (ulong)signedCbSize;
            }

            using var asb = new ArrayStreamBuf(cb.ToCharArray(), (long)cbSize);
            using var isStream = new StreamReader(asb);
            var bis = new BitStream(isStream);
            Rebuild(bis, cbSize, bos);
        }

        private void Copy(BitStream bis, BitOggStream bos)
        {
            uint id;
            uint dimensions;
            uint entries;

            bis.ReadBits(out id, 24);
            bis.ReadBits(out dimensions, 16);
            bis.ReadBits(out entries, 24);

            if (0x564342 != id)
            {
                throw new Exception("Invalid codebook identifier");
            }

            bos.WriteBits(id, 24);
            bos.WriteBits(dimensions, 16);
            bos.WriteBits(entries, 24);

            uint ordered;
            bis.ReadBits(out ordered, 1);
            bos.WriteBits(ordered, 1);

            if (ordered == 1)
            {
                uint initialLength;
                bis.ReadBits(out initialLength, 5);
                bos.WriteBits(initialLength, 5);

                uint currentEntry = 0;
                while (currentEntry < entries)
                {
                    uint number;
                    bis.ReadBits(out number, Ilog(entries - currentEntry));
                    bos.WriteBits(number, Ilog(entries - currentEntry));
                    currentEntry += (uint)number;
                }

                if (currentEntry > entries)
                {
                    throw new Exception("Current entry out of range");
                }
            }
            else
            {
                uint sparse;
                bis.ReadBits(out sparse, 1);
                bos.WriteBits(sparse, 1);

                for (uint i = 0; i < entries; i++)
                {
                    bool presentBool = true;

                    if (sparse == 1)
                    {
                        uint present;
                        bis.ReadBits(out present, 1);
                        bos.WriteBits(present, 1);

                        presentBool = present != 0;
                    }

                    if (presentBool)
                    {
                        uint codewordLength;
                        bis.ReadBits(out codewordLength, 5);
                        bos.WriteBits(codewordLength, 5);
                    }
                }
            }

            uint lookupType;
            bis.ReadBits(out lookupType, 4);
            bos.WriteBits(lookupType, 4);

            if (lookupType == 0)
            {
                // No lookup table
            }
            else if (lookupType == 1)
            {
                uint min, max;
                uint valueLength;
                uint sequenceFlag;

                bis.ReadBits(out min, 32);
                bis.ReadBits(out max, 32);
                bis.ReadBits(out valueLength, 4);
                bis.ReadBits(out sequenceFlag, 1);

                bos.WriteBits(min, 32);
                bos.WriteBits(max, 32);
                bos.WriteBits(valueLength, 4);
                bos.WriteBits(sequenceFlag, 1);

                uint quantvals = _book_maptype1_quantvals(entries, dimensions);
                for (uint i = 0; i < quantvals; i++)
                {
                    uint val;
                    bis.ReadBits(out val, (int)valueLength + 1);
                    bos.WriteBits(val, (int)valueLength + 1);
                }
            }
            else if (lookupType == 2)
            {
                throw new Exception("Didn't expect lookup type 2");
            }
            else
            {
                throw new Exception("Invalid lookup type");
            }
        }

        private void Rebuild(BitStream bis, ulong cbSize, BitOggStream bos)
        {
            uint dimensions;
            uint entries;

            bis.ReadBits(out dimensions, 4);
            bis.ReadBits(out entries, 14);

            bos.WriteBits(0x564342, 24);
            bos.WriteBits(dimensions, 16);
            bos.WriteBits(entries, 24);

            uint ordered;
            bis.ReadBits(out ordered, 1);
            bos.WriteBits(ordered, 1);

            if (ordered == 1)
            {
                uint initialLength;
                bis.ReadBits(out initialLength, 5);
                bos.WriteBits(initialLength, 5);

                uint currentEntry = 0;
                while (currentEntry < entries)
                {
                    uint number;
                    bis.ReadBits(out number, Ilog(entries - currentEntry));
                    bos.WriteBits(number, Ilog(entries - currentEntry));
                    currentEntry += (uint)number;
                }

                if (currentEntry > entries)
                {
                    throw new Exception("Current entry out of range");
                }
            }
            else
            {
                uint codewordLengthLength;
                uint sparse;
                bis.ReadBits(out codewordLengthLength, 3);
                bis.ReadBits(out sparse, 1);

                bos.WriteBits(sparse, 1);

                for (uint i = 0; i < entries; i++)
                {
                    bool presentBool = true;

                    if (sparse == 1)
                    {
                        uint present;
                        bis.ReadBits(out present, 1);
                        bos.WriteBits(present, 1);

                        presentBool = present != 0;
                    }

                    if (presentBool)
                    {
                        uint codewordLength;
                        bis.ReadBits(out codewordLength, (int)codewordLengthLength);

                        bos.WriteBits(codewordLength, 5);
                    }
                }
            }

            uint lookupType;
            bis.ReadBits(out lookupType, 1);
            bos.WriteBits(lookupType, 4);

            if (lookupType == 0)
            {
                // No lookup table
            }
            else if (lookupType == 1)
            {
                uint min, max;
                uint valueLength;
                uint sequenceFlag;

                bis.ReadBits(out min, 32);
                bis.ReadBits(out max, 32);
                bis.ReadBits(out valueLength, 4);
                bis.ReadBits(out sequenceFlag, 1);

                bos.WriteBits(min, 32);
                bos.WriteBits(max, 32);
                bos.WriteBits(valueLength, 4);
                bos.WriteBits(sequenceFlag, 1);

                uint quantvals = _book_maptype1_quantvals(entries, dimensions);
                for (uint i = 0; i < quantvals; i++)
                {
                    uint val;
                    bis.ReadBits(out val, (int)valueLength + 1);
                    bos.WriteBits(val, (int)valueLength + 1);
                }
            }
            else if (lookupType == 2)
            {
                throw new Exception("Didn't expect lookup type 2");
            }
            else
            {
                throw new Exception("Invalid lookup type");
            }

            if (cbSize != 0 && bis.GetTotalBitsRead() / 8 + 1 != (int)cbSize)
            {
                throw new Exception($"Size mismatch: {cbSize}, {bis.GetTotalBitsRead() / 8 + 1}");
            }
        }

        private string GetCodebook(int i)
        {
            // Implementation not provided in the given code snippet
            return null;
        }

        private long GetCodebookSize(int i)
        {
            // Implementation not provided in the given code snippet
            return -1;
        }

        private int Read32LE(Stream stream)
        {
            int value = 0;
            for (int i = 0; i < 4; i++)
            {
                value |= stream.ReadByte() << (i * 8);
            }
            return value;
        }

        private int Ilog(ulong v)
        {
            int r = 0;
            while (v != 0)
            {
                r++;
                v >>= 1;
            }
            return r;
        }

        private uint _book_maptype1_quantvals(ulong entries, ulong dimensions)
        {
            // Implementation not provided in the given code snippet
            return 0;
        }
    }
}
*/