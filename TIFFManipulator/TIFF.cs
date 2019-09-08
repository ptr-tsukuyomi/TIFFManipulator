using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TIFFManipulator
{
    public class ImageFileHeader
    {
        public bool IsLittleEndian;
        public UInt32 OffsetOf1stIFD;

        public static async Task<ImageFileHeader> LoadFromStream(Stream stream)
        {
            var ifh = new ImageFileHeader();

            var TIFFBOM = new byte[2];
            await stream.ReadAsync(TIFFBOM, 0, 2);
            var BOMValue = System.Text.Encoding.ASCII.GetString(TIFFBOM);

            ifh.IsLittleEndian = BOMValue == "II";

            var binaryReader = new BinaryTools.BinaryReader(stream, ifh.IsLittleEndian);
            await binaryReader.ReadUInt16Async(); // dispose 42

            ifh.OffsetOf1stIFD = await binaryReader.ReadUInt32Async();

            return ifh;
        }
    }

    public class ImageFileDirectory
    {
        public UInt16 Count
        {
            get
            {
                return (ushort)IFDEntries.Count();
            }
        }
        public List<IFDEntry> IFDEntries = new List<IFDEntry>();
        public UInt32 OffsetOfNextIFD;

        public static async Task<ImageFileDirectory> LoadFromStream(Stream stream, bool isLitteleEndian)
        {
            var ifd = new ImageFileDirectory();
            var binaryReader = new BinaryTools.BinaryReader(stream, isLitteleEndian);

            var count = await binaryReader.ReadUInt16Async();
            for(int i = 0; i < count; ++i)
            {
                var ifdEntry = await IFDEntry.LoadFromStream(stream, isLitteleEndian);
                ifd.IFDEntries.Add(ifdEntry);
            }

            ifd.OffsetOfNextIFD = await binaryReader.ReadUInt32Async();

            return ifd;
        }
    }


    public class TIFF
    {
        private ImageFileHeader _ifh;
        public List<ImageFileDirectory> ImageFileDirectories = new List<ImageFileDirectory>();

        public static async Task<TIFF> LoadFromStream(Stream stream)
        {
            var tiff = new TIFF();
            tiff._ifh = await ImageFileHeader.LoadFromStream(stream);

            if (tiff._ifh.OffsetOf1stIFD == 0) return tiff;
            stream.Position = tiff._ifh.OffsetOf1stIFD;

            while (true)
            {
                var ifd = await ImageFileDirectory.LoadFromStream(stream, tiff._ifh.IsLittleEndian);
                tiff.ImageFileDirectories.Add(ifd);

                if (ifd.OffsetOfNextIFD == 0) break;

                stream.Position = ifd.OffsetOfNextIFD;
            }

            return tiff;
        }
    }
}
