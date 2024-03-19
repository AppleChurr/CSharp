using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signtelecom.ESRI.Shapefile
{
    public enum ShapeType
    {
        Null =          0x0000, // 00
        Point =         0x0001, // 01
        PolyLine =      0x0003, // 03
        Polygon =       0x0005, // 05
        MultiPoint =    0x0008, // 08
        PointZ =        0x000B, // 11
        PolyLineZ =     0x000D, // 13
        PolygonZ =      0x000F, // 15
        MultiPointZ =   0x0012, // 18
        PointM =        0x0015, // 21
        PolyLineM =     0x0017, // 23
        PolygonM =      0x0019, // 25
        MultiPointM =   0x001C, // 28
        MultiPatch =    0x001F  // 31
    }

    public class oESRIShape : IDisposable
    {
        /// <summary>
        /// The length of a Shapefile header in bytes
        /// </summary>
        public const int HeaderLength = 100;

        public FileHeader Header { get; set; }

        public List<FileRecord> Record { get; set; }

        public string FileName { get; set; }

        public oESRIShape(string FilePath)
        {

            if (!File.Exists(FilePath))
                throw new Exception(FilePath + " is not exists");

            FileName = FilePath.Substring(FilePath.LastIndexOf('\\') + 1, FilePath.Length - (FilePath.LastIndexOf('\\') + 1));

            List<byte> FileBytes = File.ReadAllBytes(FilePath).ToList();

            if (FileBytes == null || FileBytes.Count <= 0)
                throw new ArgumentNullException("FileBytes");

            Header = new FileHeader(FileBytes.GetRange(0, HeaderLength).ToArray());
            FileBytes.RemoveRange(0, HeaderLength);

#if DEBUG
            Console.WriteLine(Header.FileLength);
            Console.WriteLine(Header.Type);

            Console.WriteLine(Header.Xmin + " ~ " + Header.Xmax);
            Console.WriteLine(Header.Ymin + " ~ " + Header.Ymax);
            Console.WriteLine(Header.Zmin + " ~ " + Header.Zmax);
            Console.WriteLine(Header.Mmin + " ~ " + Header.Mmax);

            Console.WriteLine("");
#endif

            Record = new List<FileRecord>();

            while (FileBytes.Count != 0)
            {
                if (FileName == "GCP.shp" || FileName == "Point107.shp")
                    Record.Add(new FileRecord(Header.Type, FileBytes, Header.isUnusedZ, !Header.isUnusedM));
                else
                    Record.Add(new FileRecord(Header.Type, FileBytes, Header.isUnusedZ, Header.isUnusedM));
                
            }
            
        }

        ~oESRIShape()
        {
            Dispose();
        }
        public void Dispose()
        {
           
        }
    }
}

/* ------------------------------------------------------------------------
 * (c)copyright 2009-2019 Robert Ellison and contributors - https://github.com/abfo/shapefile
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

namespace Signtelecom.ESRI.Shapefile
{
    /// <summary>
    /// The order of bytes provided to EndianBitConverter
    /// </summary>
    public enum EndianOrder
    {
        /// <summary>
        /// Value is stored as big-endian
        /// </summary>
        Big,

        /// <summary>
        /// Value is stored as little-endian
        /// </summary>
        Little
    }

    /// <summary>
    /// BitConverter methods that allow a different source byte order (only a subset of BitConverter)
    /// </summary>
    public static class EndianBitConverter
    {
        /// <summary>
        /// Returns an integer from four bytes of a byte array
        /// </summary>
        /// <param name="value">bytes to convert</param>
        /// <param name="startIndex">start index in value</param>
        /// <param name="order">byte order of value</param>
        /// <returns>the integer</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        /// <exception cref="ArgumentException">Thrown if startIndex is invalid</exception>
        public static int ToInt32(byte[] value, int startIndex, EndianOrder order)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((startIndex + sizeof(int)) > value.Length)
            {
                throw new ArgumentException("startIndex invalid (not enough space in value to extract an integer", "startIndex");
            }

            if (BitConverter.IsLittleEndian && (order == EndianOrder.Big))
            {
                byte[] toConvert = new byte[sizeof(int)];
                Array.Copy(value, startIndex, toConvert, 0, sizeof(int));
                Array.Reverse(toConvert);
                return BitConverter.ToInt32(toConvert, 0);
            }
            else
            {
                return BitConverter.ToInt32(value, startIndex);
            }
        }

        /// <summary>
        /// Returns a double from eight bytes of a byte array
        /// </summary>
        /// <param name="value">bytes to convert</param>
        /// <param name="startIndex">start index in value</param>
        /// <param name="order">byte order of value</param>
        /// <returns>the double</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        /// <exception cref="ArgumentException">Thrown if startIndex is invalid</exception>
        public static double ToDouble(byte[] value, int startIndex, EndianOrder order)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((startIndex + sizeof(double)) > value.Length)
            {
                throw new ArgumentException("startIndex invalid (not enough space in value to extract a double", "startIndex");
            }

            if (BitConverter.IsLittleEndian && (order == EndianOrder.Big))
            {
                byte[] toConvert = new byte[sizeof(double)];
                Array.Copy(value, startIndex, toConvert, 0, sizeof(double));
                Array.Reverse(toConvert);
                return BitConverter.ToDouble(toConvert, 0);
            }
            else
            {
                return BitConverter.ToDouble(value, startIndex);
            }
        }
    }
}

