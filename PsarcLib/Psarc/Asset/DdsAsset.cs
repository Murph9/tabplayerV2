using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Buffers;
using System.IO;

namespace Rocksmith2014PsarcLib.Psarc.Asset;

public class DdsAsset : PsarcAsset
{
    /// <summary>
    /// Uses ArrayPool to rent byte arrays to Pfim, by default Pfim creates a new byte array each time
    /// </summary>
    private class ArrayPoolAllocator : Pfim.IImageAllocator
    {
        // Use the shared byte array pool
        private readonly ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        
        public byte[] Rent(int size)
        {
            return pool.Rent(size);
        }

        public void Return(byte[] data)
        {
            pool.Return(data);
        }
    }

    private Pfim.IImage _image;

    public override void ReadFrom(MemoryStream stream)
    {
        _image = Pfim.Pfimage.FromStream(stream, new Pfim.PfimConfig());
    }

    public void Save(string file)
    {
        if (_image.Format == Pfim.ImageFormat.Rgba32)
            Save<Bgra32>(file);
        else if (_image.Format == Pfim.ImageFormat.Rgb24)
            Save<Bgr24>(file);
        else
            throw new Exception("Unsupported pixel format (" + _image.Format + ")");
    }

    private void Save<T>(string file) where T : unmanaged, IPixel<T>
    {
        var image = Image.LoadPixelData<T>(_image.Data, _image.Width, _image.Height);
        image.Save(file);
    }
}
