using System;
using System.IO;
using System.Linq;

namespace TIFFManipulator
{

    public abstract class IFDEntry
    {
        public enum FieldTypeEnum
        {
            BYTE = 1,
            ASCII,
            SHORT,
            LONG,
            RATIONAL,
            SBYTE,
            UNDEFINED,
            SSHORT,
            SLONG,
            SRATIONAL,
            FLOAT,
            DOUBLE
        }

        public UInt16 Tag;
        public FieldTypeEnum FieldType;
        public UInt16 Count;

        public UInt32 Offset;

        protected static int GetValuesLength(FieldTypeEnum fielType, UInt16 count)
        {
            int unit = 0;
            
        }

        public static async IFDEntry LoadFromStream(Stream stream, bool isLittleEndian)
        {
            var binaryReader = new BinaryTools.BinaryReader(stream, isLittleEndian);

            var tag = await binaryReader.ReadUInt16Async();
            var fieldType = (FieldTypeEnum)await binaryReader.ReadUInt16Async();
            var count = await binaryReader.ReadUInt16Async();
        }
    }

    public class BYTEEntry : IFDEntry
    {
        public Byte[] Values;
    }
    public class SHORTEntry : IFDEntry
    {
        public UInt16[] Values;
    }
    public class LONGEntry : IFDEntry
    {
        public UInt32[] Values;
    }
    public class ASCIIEntry : IFDEntry
    {
        public string[] Values;
    }
    public class RATIONALEntry: IFDEntry
    {
        /// <summary>
        /// Fraction
        /// </summary>
        public UInt16[] First;
        /// <summary>
        /// Denominator
        /// </summary>
        public UInt16[] Second;
        public double[] Calculated => First?.Zip(Second, (a, b) => (double)a / b).ToArray();
    }
    public class SBYTEEntry : IFDEntry
    {
        public SByte[] Values;
    }
    public class UNDEFINEDEntry: IFDEntry
    {
        public byte[] Value;
    }
    public class SSHORTEntry: IFDEntry
    {
        public Int16[] Values;
    }
    public class SLONGEntry: IFDEntry
    {
        public Int32[] Values;
    }
    public class SRATIONALEntry: IFDEntry
    {
        /// <summary>
        /// Fraction
        /// </summary>
        public Int16[] First;
        /// <summary>
        /// Denominator
        /// </summary>
        public Int16[] Second;
        public double[] Calculated => First?.Zip(Second, (a, b) => (double)a / b).ToArray();
    }

    public class FLOATEntry
    {
        public float[] Values;
    }
    public class DOUBLEEntry
    {
        public double[] Values;
    }

    public class TIFF
    {

    }
}
