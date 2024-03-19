using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signtelecom.ESRI.Shapefile.Record
{
    class ShapePointZ : Shape
    {
        private double _x;
        private double _y;
        private double _z;
        private double _m;
        private Point3D _point;

        public double X { get { return _x; } }
        public double Y { get { return _y; } }
        public double Z { get { return _z; } }
        public double Measure { get { return _m; } }
        public Point3D PointZ { get { return _point; } }

        private enum DataLocation
        {
                                    //Position  Field       Value   Type    Number  Order
            ShapeType   = 0x0000,   //Byte 0    Shape Type  11      Integer 1       Little
            X           = 0x0004,   //Byte 4    X           X       Double  1       Little
            Y           = 0x000C,   //Byte 12   Y           Y       Double  1       Little
            Z           = 0x0014,   //Byte 20   Z           Z       Double  1       Little
            Measure     = 0x001C,   //Byte 28   Measure     M       Double  1       Little
        }

        public ShapePointZ(List<byte> MetaData, bool UnusedZ = true, bool UnusedM = true)
        {
            int DataEnd = UnusedZ ? (int)DataLocation.Z : (UnusedM ? (int)DataLocation.Measure : (int)DataLocation.Measure + SizeofDouble);

            Data = MetaData.GetRange(0, DataEnd).ToArray();

            Type = (ShapeType)EndianBitConverter.ToInt32(Data, (int)DataLocation.ShapeType, EndianOrder.Little);

            _x = EndianBitConverter.ToDouble(Data, (int)DataLocation.X, EndianOrder.Little);
            _y = EndianBitConverter.ToDouble(Data, (int)DataLocation.Y, EndianOrder.Little);

            _z = UnusedZ ? 0 : EndianBitConverter.ToDouble(Data, (int)DataLocation.Z, EndianOrder.Little);
            _m = UnusedM ? 0 : EndianBitConverter.ToDouble(Data, (int)DataLocation.Measure, EndianOrder.Little);

            _point = new Point3D(_x, _y, _z);

#if DEBUG
            //Console.WriteLine("\tPointZ : (" + X + ", " + Y + ", " + Z + ")");
            //Console.WriteLine("\t--");
#endif

            MetaData.RemoveRange(0, DataEnd);
        }
    }
}
