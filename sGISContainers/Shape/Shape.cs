using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signtelecom.ESRI.Shapefile.Record
{
    class Shape
    {
        protected readonly int SizeofInt = 4;
        protected readonly int SizeofDouble = 8;

        protected ShapeType Type { get; set; }
        protected byte[] Data { get; set; }
    }
}
