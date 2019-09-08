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
                            Console.WriteLine("\tTag: {0}, Type: {1}, Count: {2}", e.Tag, e.FieldType.ToString());
                        }
                    }
                }
            }).Wait();
        }
    }
}
