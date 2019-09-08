using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System;   

namespace TIFFManipulatorTest
{
    [TestClass]
    public class TIFFManipulatorLoadTest
    {
        [TestMethod]
        public void LoadTIFF()
        {
            Task.Run(async () =>
            {
                using (var stream = System.IO.File.OpenRead("inui.tif"))
                {
                    var tiff = await TIFFManipulator.TIFF.LoadFromStream(stream);

                    Console.WriteLine("IsLittleEndian: {0}", tiff.IsLittleEndian);
                    Console.WriteLine("IFDs: {0}", tiff.ImageFileDirectories.Count);

                    foreach(var ifd in tiff.ImageFileDirectories)
                    {
                        Console.WriteLine("IFD entries: {0}", ifd.Count);
                        foreach(var e in ifd.IFDEntries)
                        {
                            Console.WriteLine("\tTag: {0}, Type: {1}, Count: {2}", e.Tag, e.FieldType.ToString(), e.Count);

                            switch (e)
                            {
                                case TIFFManipulator.ASCIIEntry a:
                                    foreach(var str in a.Values)
                                    {
                                        Console.WriteLine("\t\t{0}", str);
                                    }
                                    break;
                                case TIFFManipulator.UNDEFINEDEntry u:
                                    Console.Write("\t\t");
                                    foreach (var b in u.Value)
                                    {
                                        Console.Write("{0:X2} ", b);
                                    }
                                    Console.WriteLine();
                                    break;
                                case TIFFManipulator.IntegerEntry i:
                                    foreach(var v in i.IntegerValues)
                                    {
                                        Console.WriteLine("\t\t{0}", v);
                                    }
                                    break;
                                case TIFFManipulator.NumberEntry n:
                                    foreach(var v in n.RealNumberValues)
                                    {
                                        Console.WriteLine("\t\t{0}", v);
                                    }
                                    break;
                                default:
                                    Assert.Fail("Must be unreachable here!");
                                    break;
                            }
                        }
                    }
                }
            }).Wait();
        }
    }
}
