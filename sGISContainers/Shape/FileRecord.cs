using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Signtelecom.ESRI.Shapefile.Record;

namespace Signtelecom.ESRI.Shapefile
{
    public class FileRecord
    {
        private FileRecordHeader Header;
        public int Index { get { return Header.RecordNumber; } }

        private ShapeType _type;

        private object _shapeData;
        public object Shape { get{ return _shapeData; } }


        public FileRecord(ShapeType _type, List<byte> _bytes, bool UnusedZ = true, bool UnusedM = true)
        {
            Header = new FileRecordHeader(_bytes);

            this._type = _type;

            switch (this._type)
            {
                case ShapeType.PointZ:
                    _shapeData = new ShapePointZ(_bytes, UnusedZ, UnusedM);
                    break;

                case ShapeType.PolyLineZ:
                    _shapeData = new ShapePolyLineZ(_bytes, UnusedZ, UnusedM);
                    break;

                case ShapeType.PolygonZ:
                    _shapeData = new ShapePloygonZ(_bytes, UnusedZ, UnusedM);
                    break;
            }

            
        }

        public new Type GetType()
        {
            switch(this._type)
            {
                case ShapeType.PointZ:
                    return typeof(ShapePointZ);

                case ShapeType.PolyLineZ:
                    return typeof(ShapePolyLineZ);

                case ShapeType.PolygonZ:
                    return typeof(ShapePloygonZ);

                default:
                    return typeof(Shape);
            }
        }

    }
}



namespace Signtelecom.ESRI.Shapefile.Record
{
    class FileRecordHeader
    {
        private const int HeaderLength = 8;

        public int RecordNumber { get; set; }
        public int ContentLength { get; set; }

        private byte[] Header;


        private enum FileHeaderLocation
        {
                                        // Position Field           Value           Type        Order
            RecordNumber    = 0x0000,   // Byte 0   RecordNumber    Record Number   Integer     Big
            ContentLength   = 0x0004,   // Byte 4   Content Length  Content Length  Integer     Big
        }

        public FileRecordHeader(List<byte> bytes)
        {
            Header = bytes.GetRange(0, HeaderLength).ToArray();
            
            RecordNumber = EndianBitConverter.ToInt32(Header, (int)FileHeaderLocation.RecordNumber, EndianOrder.Big);
            ContentLength = EndianBitConverter.ToInt32(Header, (int)FileHeaderLocation.ContentLength, EndianOrder.Big);

#if DEBUG
            //Console.WriteLine("\t--");
            //Console.WriteLine("\tRecordNumber : " + RecordNumber + ", ContentLength : " + ContentLength);
#endif

            bytes.RemoveRange(0, HeaderLength);
        }
    }


}
