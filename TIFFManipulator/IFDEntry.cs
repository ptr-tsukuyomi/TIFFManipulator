using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        protected static System.Collections.Generic.Dictionary<FieldTypeEnum, Type> CorrespondingTypes = new System.Collections.Generic.Dictionary<FieldTypeEnum, Type>()
        {
            { FieldTypeEnum.BYTE, typeof(byte) },
            { FieldTypeEnum.SHORT, typeof(ushort) },
            {FieldTypeEnum.LONG, typeof(ulong) },
            {FieldTypeEnum.SBYTE, typeof(sbyte) },
            {FieldTypeEnum.SSHORT, typeof(short) },
            {FieldTypeEnum.SLONG, typeof(long) },
            {FieldTypeEnum.FLOAT, typeof(float) },
            {FieldTypeEnum.DOUBLE, typeof(double) }
        };

        protected static System.Collections.Generic.Dictionary<FieldTypeEnum, int> ValueLengths = new System.Collections.Generic.Dictionary<FieldTypeEnum, int>()
        {
            { FieldTypeEnum.BYTE, 1 },
            { FieldTypeEnum.SHORT, 2 },
            {FieldTypeEnum.LONG, 4 },
            {FieldTypeEnum.SBYTE, 1 },
            {FieldTypeEnum.SSHORT, 2 },
            {FieldTypeEnum.SLONG, 4 },
            {FieldTypeEnum.FLOAT, 4 },
            {FieldTypeEnum.DOUBLE, 8 }
        };


        public UInt16 Tag;
        public FieldTypeEnum FieldType;
        public UInt16 Count;

        public UInt32 ValueOffset;

        protected static int GetValuesLength(FieldTypeEnum fieldType, UInt16 count)
        {
            if (ValueLengths.ContainsKey(fieldType))
            {
                return ValueLengths[fieldType] * count;
            }
            switch (fieldType)
            {
                case FieldTypeEnum.ASCII:
                    return count;
                case FieldTypeEnum.RATIONAL:
                    return ValueLengths[FieldTypeEnum.LONG] * 2 * count;
                case FieldTypeEnum.SRATIONAL:
                    return ValueLengths[FieldTypeEnum.SLONG] * 2 * count;
                case FieldTypeEnum.UNDEFINED:
                    return count;
                default:
                    return 0;
            }
        }

        public static async Task<IFDEntry> LoadFromStream(Stream stream, bool isLittleEndian)
        {
            var binaryReader = new BinaryTools.BinaryReader(stream, isLittleEndian);

            var tag = await binaryReader.ReadUInt16Async();
            var fieldType = (FieldTypeEnum)await binaryReader.ReadUInt16Async();
            var count = await binaryReader.ReadUInt16Async();
            var offset = 0U;

            var valueLength = GetValuesLength(fieldType, count);
            if (valueLength > 4)
            {
                offset = await binaryReader.ReadUInt32Async();
                stream.Position = offset;
            }

            IFDEntry ifdEntry;
            switch (fieldType)
            {
                case FieldTypeEnum.BYTE:
                    {
                        ifdEntry = new BYTEEntry();
                        var array = new byte[count];

                        for (int i = 0; i < count; ++i)
                        {
                            array[i] = await binaryReader.ReadByteAsync();
                        }
                        (ifdEntry as BYTEEntry).Values = array;
                    }
                    break;
                case FieldTypeEnum.SHORT:
                    {
                        ifdEntry = new SHORTEntry();
                        var array = new ushort[count];
                        for (int i = 0; i < count; ++i)
                        {
                            array[i] = await binaryReader.ReadUInt16Async();
                        }
                        (ifdEntry as SHORTEntry).Values = array;
                    }
                    break;
                case FieldTypeEnum.LONG:
                    {
                        ifdEntry = new LONGEntry();
                        var array = new uint[count];
                        for (int i = 0; i < count; ++i)
                        {
                            array[i] = await binaryReader.ReadUInt32Async();
                        }
                        (ifdEntry as LONGEntry).Values = array;
                    }
                    break;
                case FieldTypeEnum.SBYTE:
                    {
                        ifdEntry = new SBYTEEntry();
                        var array = new sbyte[count];
                        for (int i = 0; i < count; ++i)
                        {
                            array[i] = await binaryReader.ReadSByteAsync();
                        }
                        (ifdEntry as SBYTEEntry).Values = array;
                    }
                    break;
                case FieldTypeEnum.SSHORT:
                    {
                        ifdEntry = new SSHORTEntry();
                        var array = new short[count];
                        for (int i = 0; i < count; ++i)
                        {
                            array[i] = await binaryReader.ReadInt16Async();
                        }
                        (ifdEntry as SSHORTEntry).Values = array;
                    }
                    break;
                case FieldTypeEnum.SLONG:
                    {
                        ifdEntry = new SLONGEntry();
                        var array = new int[count];
                        for (int i = 0; i < count; ++i)
                        {
                            array[i] = await binaryReader.ReadInt32Async();
                        }
                        (ifdEntry as SLONGEntry).Values = array;
                    }
                    break;
                case FieldTypeEnum.FLOAT:
                    {
                        ifdEntry = new FLOATEntry();
                        var array = new float[count];
                        for (int i = 0; i < count; ++i)
                        {
                            array[i] = await binaryReader.ReadSingleAsync();
                        }
                        (ifdEntry as FLOATEntry).Values = array;
                    }
                    break;
                case FieldTypeEnum.DOUBLE:
                    {
                        ifdEntry = new DOUBLEEntry();
                        var array = new double[count];
                        for (int i = 0; i < count; ++i)
                        {
                            array[i] = await binaryReader.ReadDoubleAsync();
                        }
                        (ifdEntry as DOUBLEEntry).Values = array;
                    }
                    break;
                case FieldTypeEnum.ASCII:
                    {
                        ifdEntry = new ASCIIEntry();
                        var data = new byte[count];

                        var read = await binaryReader.ReadAsync(data, 0, data.Length);
                        if (read != data.Length) throw new Exception("Can not read enough data.");

                        var all = System.Text.Encoding.ASCII.GetString(data, 0, data.Length);

                        (ifdEntry as ASCIIEntry).Values = all.Split(new char[] { '\0' });
                    }
                    break;
                case FieldTypeEnum.UNDEFINED:
                    {
                        ifdEntry = new UNDEFINEDEntry();
                        var data = new byte[count];

                        var read = await binaryReader.ReadAsync(data, 0, data.Length);
                        if (read != data.Length) throw new Exception("Can not read enough data.");

                        (ifdEntry as UNDEFINEDEntry).Value = data;
                    }
                    break;
                case FieldTypeEnum.RATIONAL:
                    {
                        ifdEntry = new RATIONALEntry();
                        var first = new uint[count];
                        var second = new uint[count];

                        for (int i = 0; i < count; ++i)
                        {
                            first[i] = await binaryReader.ReadUInt32Async();
                            second[i] = await binaryReader.ReadUInt32Async();
                        }

                        (ifdEntry as RATIONALEntry).First = first;
                        (ifdEntry as RATIONALEntry).Second = second;
                    }
                    break;
                case FieldTypeEnum.SRATIONAL:
                    {
                        ifdEntry = new SRATIONALEntry();
                        var first = new int[count];
                        var second = new int[count];

                        for (int i = 0; i < count; ++i)
                        {
                            first[i] = await binaryReader.ReadInt32Async();
                            second[i] = await binaryReader.ReadInt32Async();
                        }

                        (ifdEntry as SRATIONALEntry).First = first;
                        (ifdEntry as SRATIONALEntry).Second = second;
                    }
                    break;
                default:
                    throw new Exception("Specified fieldType is not defined in TIFF 6.0 and not supported.");
            }

            ifdEntry.Tag = tag;
            ifdEntry.FieldType = fieldType;
            ifdEntry.Count = count;
            ifdEntry.ValueOffset = offset;

            return ifdEntry;
        }
    }

    public abstract class NumberEntry : IFDEntry
    {
        public abstract Double[] RealNumberValues
        {
            get;
        }
    }

    public abstract class IntegerEntry : NumberEntry
    {
        public abstract Int64[] IntegerValues
        {
            get;
        }

        public override Double[] RealNumberValues
        {
            get
            {
                return IntegerValues.Select((a) => (Double)a).ToArray();
            }
        }

        protected static Int64[] ConvertToInt64Array<T>(System.Collections.Generic.IEnumerable<T> array)
        {
            return array.Select((a) => Convert.ToInt64(a)).ToArray();
        }
    }

    public class BYTEEntry : IntegerEntry
    {
        public Byte[] Values;

        public override long[] IntegerValues => ConvertToInt64Array(Values);
    }
    public class SHORTEntry : IntegerEntry
    {
        public UInt16[] Values;

        public override long[] IntegerValues => ConvertToInt64Array(Values);
    }
    public class LONGEntry : IntegerEntry
    {
        public UInt32[] Values;

        public override long[] IntegerValues => ConvertToInt64Array(Values);
    }
    public class ASCIIEntry : IFDEntry
    {
        public string[] Values;
    }
    public class RATIONALEntry : NumberEntry
    {
        /// <summary>
        /// Fraction
        /// </summary>
        public UInt32[] First;
        /// <summary>
        /// Denominator
        /// </summary>
        public UInt32[] Second;
        public double[] Calculated => First?.Zip(Second, (a, b) => (double)a / b).ToArray();

        public override double[] RealNumberValues => Calculated;
    }
    public class SBYTEEntry : IntegerEntry
    {
        public SByte[] Values;

        public override long[] IntegerValues => ConvertToInt64Array(Values);
    }
    public class UNDEFINEDEntry : IFDEntry
    {
        public byte[] Value;
    }
    public class SSHORTEntry : IntegerEntry
    {
        public Int16[] Values;

        public override long[] IntegerValues => ConvertToInt64Array(Values);
    }
    public class SLONGEntry : IntegerEntry
    {
        public Int32[] Values;

        public override long[] IntegerValues => ConvertToInt64Array(Values);
    }
    public class SRATIONALEntry : NumberEntry
    {
        /// <summary>
        /// Fraction
        /// </summary>
        public Int32[] First;
        /// <summary>
        /// Denominator
        /// </summary>
        public Int32[] Second;
        public double[] Calculated => First?.Zip(Second, (a, b) => (double)a / b).ToArray();

        public override double[] RealNumberValues => Calculated;
    }

    public class FLOATEntry : NumberEntry
    {
        public float[] Values;

        public override double[] RealNumberValues => Values.Select((a) => (double)a).ToArray();
    }
    public class DOUBLEEntry : NumberEntry
    {
        public double[] Values;

        public override double[] RealNumberValues => Values;
    }
}
