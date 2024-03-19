using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signtelecom.ESRI.Shapefile
{
    public struct Point2D
    {
        /// <summary>
        /// Gets or sets the X value
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y value
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// A simple double precision point
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public struct Point3D
    {
        /// <summary>
        /// Gets or sets the X value
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y value
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the Z value
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        /// A simple double precision point
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        /// <param name="z">Z value</param>
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public struct RectangleD
    {
        /// <summary>
        /// 
        /// </summary>
        public double X { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double Y { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_w"></param>
        /// <param name="_h"></param>
        public RectangleD(double _x, double _y, double _w, double _h)
        {
            X = _x;
            Y = _y;
            Width = _w;
            Height = _h;
        }


    }
}
