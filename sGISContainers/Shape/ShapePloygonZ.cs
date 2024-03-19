using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signtelecom.ESRI.Shapefile.Record
{
    class ShapePloygonZ : Shape
    {
        private int Xaddr { get; set; }
        private int Yaddr { get; set; }
        private int Zaddr { get; set; }

        private RectangleD _box;
        private int _nParts;
        private int _nPoints;
        private List<int> _parts = new List<int>();

        private List<Point3D> _points = new List<Point3D>();


        public RectangleD Box { get { return _box; } }
        public int NumParts { get { return _nParts; } }
        public int NumPoints { get { return _nPoints; } }
        public List<int> Parts { get { return _parts; } }
        public List<Point3D> Points { get { return _points; } }

        private double _xmin { get; set; }
        private double _ymin { get; set; }
        private double _xmax { get; set; }
        private double _ymax { get; set; }
        private double _zmin { get; set; }
        private double _zmax { get; set; }
        private double _mmin { get; set; }
        private double _mmax { get; set; }

        private enum DataLocation
        {
                                    // Position     Field       Value       Type    Number      Order
            ShapeType   = 0x0000,   // Byte 0       ShapeType   15          Integer 1           Little
            Box         = 0x0004,   // Byte 4       Box         Box         Double  4           Little
            NumParts    = 0x0024,   // Byte 36      NumParts    NumParts    Integer 1           Little
            NumPoints   = 0x0028,   // Byte 40      NumPoints   NumPoints   Integer 1           Little
            Parts       = 0x002C    // Byte 44      Parts       Parts       Integer NumParts    Little
                                    // Byte X       Points      Points      Point   NumPoints   Little
                                    // Byte Y       Zmin        Zmin        Double  1           Little
                                    // Byte Y+8     Zmax        Zmax        Double  1           Little
                                    // Byte Y+16    Zarray      Zarray      Double  NumPoints   Little
                                    // Byte Z*      Mmin        Mmin        Double  1           Little
                                    // Byte Z+8*    Mmax        Mmax        Double  1           Little
                                    // Byte Z+16*   Marray      Marray      Double  NumPoints   Little

            // Note: X = 44 + (4 * NumParts), Y = X + (16 * NumPoints), Z = Y + 16 + (8 * NumPoints)
        }

        public ShapePloygonZ(List<byte> MetaData, bool UnusedZ = true, bool UnusedM = true)
        {
            Data = MetaData.ToArray();

            Type = (ShapeType)EndianBitConverter.ToInt32(Data, (int)DataLocation.ShapeType, EndianOrder.Little);

            _xmin = EndianBitConverter.ToDouble(Data, (int)DataLocation.Box + SizeofDouble * 0, EndianOrder.Little);
            _ymin = EndianBitConverter.ToDouble(Data, (int)DataLocation.Box + SizeofDouble * 1, EndianOrder.Little);
            _xmax = EndianBitConverter.ToDouble(Data, (int)DataLocation.Box + SizeofDouble * 2, EndianOrder.Little);
            _ymax = EndianBitConverter.ToDouble(Data, (int)DataLocation.Box + SizeofDouble * 3, EndianOrder.Little);
            _box = new RectangleD(_xmin, _ymin, _xmax - _xmin, _ymax - _ymin);

            _nParts = EndianBitConverter.ToInt32(Data, (int)DataLocation.NumParts, EndianOrder.Little);
            _nPoints = EndianBitConverter.ToInt32(Data, (int)DataLocation.NumPoints, EndianOrder.Little);

            Xaddr = (int)DataLocation.Parts + (4 * NumParts);
            Yaddr = Xaddr + (16 * NumPoints);
            Zaddr = Yaddr + 16 + (8 * NumPoints);

            int DataEnd = UnusedZ ? Yaddr : (UnusedM ? Zaddr : Zaddr + SizeofDouble * 3);

            Data = MetaData.GetRange(0, DataEnd).ToArray();

            if (!UnusedZ)
            {
                _zmin = EndianBitConverter.ToDouble(Data, Yaddr, EndianOrder.Little);
                _zmax = EndianBitConverter.ToDouble(Data, Yaddr + SizeofDouble, EndianOrder.Little);

                if (!UnusedM)
                {
                    _mmin = EndianBitConverter.ToDouble(Data, Zaddr, EndianOrder.Little);
                    _mmax = EndianBitConverter.ToDouble(Data, Zaddr + SizeofDouble, EndianOrder.Little);
                }
            }

            for (int ii = 0; ii < NumParts; ii++)
                _parts.Add(EndianBitConverter.ToInt32(Data, (int)DataLocation.Parts + SizeofInt * ii, EndianOrder.Little));

            for (int ii = 0; ii < NumPoints; ii++)
            {
                double _x = EndianBitConverter.ToDouble(Data, Xaddr + SizeofDouble * (2 * ii), EndianOrder.Little);
                double _y = EndianBitConverter.ToDouble(Data, Xaddr + SizeofDouble * (2 * ii + 1), EndianOrder.Little);
                double _z = 0.0f;
                double _m = 0.0f;


                if (!UnusedZ)
                {
                    _z = EndianBitConverter.ToDouble(Data, Yaddr + SizeofDouble * (2 + ii), EndianOrder.Little);

                    if (!UnusedM)
                    {
                        _m = EndianBitConverter.ToDouble(Data, Zaddr + SizeofDouble * (2 + ii), EndianOrder.Little);
                    }
                }

                _points.Add(new Point3D(_x, _y, _z));
            }

            MetaData.RemoveRange(0, DataEnd);
        }

    }
}
