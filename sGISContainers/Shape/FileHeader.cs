using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signtelecom.ESRI.Shapefile
{
    public class FileHeader
    {
        private const int ExpectedFileCode = 9994;
        private const int ExpectedVersion = 1000;

        public int FileCode { get; set; }
        public int FileLength { get; set; }

        public int Version { get; set; }

        public ShapeType Type { get; set; }
        
        public double Xmin{ get; set; }
        public double Xmax { get; set; }

        public double Ymin{ get; set; }
        public double Ymax{ get; set; }
        
        public double Zmin{ get; set; }
        public double Zmax{ get; set; }
        public bool isUnusedZ { get; set; }

        public double Mmin{ get; set; }
        public double Mmax { get; set; }
        public bool isUnusedM { get; set; }


        public string DistanceUnit { get { return "Meter"; } }

        public RectangleF Area { get { return new RectangleF(new PointF((float)Xmin, (float)Ymin), new SizeF((float)Xmax - (float)Xmin, (float)Ymax - (float)Ymin)); } }

        private byte[] Header;

        private enum FileHeaderLocation
        {
                                    //Position  Field           Value       Type        Order   
            FileCode =      0x0000, //Byte 0    File Code       9994        Integer     Big
            Unused =        0x0004, //Byte 4    Unused          0           Integer     Big
            FileLength =    0x0018, //Byte 24   File Length     File Length Integer     Big
            Version =       0x001C, //Byte 28   Version         1000        Integer     Little
            ShapeType =     0x0020, //Byte 32   Shape Type      Shape Type  Integer     Little
            BBoxXmin =      0x0024, //Byte 36   Bounding Box    Xmin        Double      Little  
            BBoxYmin =      0x002C, //Byte 44   Bounding Box    Ymin        Double      Little
            BBoxXmax =      0x0034, //Byte 52   Bounding Box    Xmax        Double      Little
            BBoxYmax =      0x003C, //Byte 60   Bounding Box    Ymax        Double      Little
            BBoxZmin =      0x0044, //Byte 68*  Bounding Box    Zmin        Double      Little
            BBoxZmax =      0x004C, //Byte 76*  Bounding Box    Zmax        Double      Little
            BBoxMmin =      0x0054, //Byte 84*  Bounding Box    Mmin        Double      Little
            BBoxMmax =      0x005C  //Byte 92*  Bounding Box    Mmax        Double      Little
        }

        public FileHeader(byte[] data)
        {
            Header = (byte[])data.Clone();

            FileCode = EndianBitConverter.ToInt32(Header, (int)FileHeaderLocation.FileCode, EndianOrder.Big);
            if (FileCode != ExpectedFileCode)
            {
                throw new InvalidOperationException(string.Format("Header File code is {0}, expected {1}", FileCode, ExpectedFileCode));
            }

            Version = EndianBitConverter.ToInt32(Header, (int)FileHeaderLocation.Version, EndianOrder.Little);
            if (Version != ExpectedVersion)
            {
                throw new InvalidOperationException(string.Format("Header version is {0}, expected {1}", Version, ExpectedVersion));
            }

            FileLength = EndianBitConverter.ToInt32(Header, (int)FileHeaderLocation.FileLength, EndianOrder.Big);

            Type = (ShapeType)EndianBitConverter.ToInt32(Header, (int)FileHeaderLocation.ShapeType, EndianOrder.Little);
            Xmin = EndianBitConverter.ToDouble(Header, (int)FileHeaderLocation.BBoxXmin, EndianOrder.Little);
            Ymin = EndianBitConverter.ToDouble(Header, (int)FileHeaderLocation.BBoxYmin, EndianOrder.Little);
            Xmax = EndianBitConverter.ToDouble(Header, (int)FileHeaderLocation.BBoxXmax, EndianOrder.Little);
            Ymax = EndianBitConverter.ToDouble(Header, (int)FileHeaderLocation.BBoxYmax, EndianOrder.Little);
            Zmin = EndianBitConverter.ToDouble(Header, (int)FileHeaderLocation.BBoxZmin, EndianOrder.Little);
            Zmax = EndianBitConverter.ToDouble(Header, (int)FileHeaderLocation.BBoxZmax, EndianOrder.Little);
            Mmin = EndianBitConverter.ToDouble(Header, (int)FileHeaderLocation.BBoxMmin, EndianOrder.Little);
            Mmax = EndianBitConverter.ToDouble(Header, (int)FileHeaderLocation.BBoxMmax, EndianOrder.Little);

            isUnusedZ = (Zmin == 0 && Zmax == 0);
            isUnusedM = (Mmin == 0 && Mmax == 0);
        }
    }
}