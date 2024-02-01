using System;
using System.Drawing;
using System.Text;

namespace sControl.Map
{
    public class sTileMap
    {
        public enum TILESIZE : int
        {
            Low = 256,
            High = 512
        }

        public int TileSize = (int)TILESIZE.Low;

        public int wTile = 4;
        public int hTile = 4;

        public int MAP_X = 108; // 폴더 이름이 X축 값 
        public int MAP_Y = 49; // 이미지 이름이 Y축 값

        public Rectangle FocusedRect = new Rectangle(256, 256, 512, 512);

        public int Width { get { return TileSize * wTile; } }
        public int Height { get { return TileSize * hTile; } }

        public int X { get { return MAP_X; } set { MAP_X = value; } }
        public int Y { get { return MAP_Y; } set { MAP_Y = value; } }

    }


    public static class sAzureMap
    {
        private const double LAT_MIN = -85.05112878;
        private const double LAT_MAX = 85.05112878;
        private const double LONG_MIN = -180.0f;
        private const double LONG_MAX = 180.0f;


        #region Azure Map Ref.

        /// <summary>
        /// 지정된 최소값과 최대값 사이로 숫자를 제한합니다.
        /// </summary>
        /// <param name="n">제한할 숫자입니다.</param>
        /// <param name="minValue">허용 가능한 최소값입니다.</param>
        /// <param name="maxValue">허용 가능한 최대값입니다.</param>
        /// <returns>제한된 값입니다.</returns>
        private static double Clip(double n, double minValue, double maxValue)
        {
            return Math.Min(Math.Max(n, minValue), maxValue);
        }

        /// <summary>
        /// 특정 확대 수준에서 지도의 너비와 높이를 픽셀 단위로 계산합니다.
        /// </summary>
        /// <param name="zoom">계산할 확대 수준입니다.</param>
        /// <param name="tileSize">타일 피라미드의 타일 크기입니다.</param>
        /// <returns>픽셀 단위의 지도 너비와 높이입니다.</returns>
        public static double MapSize(double zoom, int tileSize)
        {
            return Math.Ceiling(tileSize * Math.Pow(2, zoom));
        }

        /// <summary>
        /// 픽셀 좌표를 특정 확대 수준에서 지리 좌표로 변환합니다.
        /// 픽셀 좌표는 지도의 좌측 상단 모서리(위도 90, 경도 -180)를 기준으로 합니다.
        /// </summary>
        /// <param name="pixel">[x, y] 형식의 픽셀 좌표입니다.</param>  
        /// <param name="zoom">확대 수준입니다.</param>
        /// <param name="tileSize">타일 피라미드의 타일 크기입니다.</param>
        /// <returns>[경도, 위도] 형식의 위치 값입니다.</returns>
        public static double[] GlobalPixelToPosition(double[] pixel, int zoom, int tileSize)
        {
            var mapSize = MapSize(zoom, tileSize);

            var x = (Clip(pixel[0], 0, mapSize - 1) / mapSize) - 0.5;
            var y = 0.5 - (Clip(pixel[1], 0, mapSize - 1) / mapSize);

            return new double[] {
                360 * x,    // 경도
                90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI  // 위도
            };
        }

        /// <summary>
        /// Point 객체를 [경도, 위도] 형식으로 변환합니다.
        /// </summary>
        /// <param name="p">변환할 Point 객체입니다.</param>
        /// <param name="zoom">확대 수준입니다.</param>
        /// <param name="tileSize">타일 크기입니다.</param>
        /// <returns>[경도, 위도] 형식의 배열입니다.</returns>
        public static double[] PointToPosition(Point p, int zoom, int tileSize, int mapX, int mapY, Rectangle focuseRect)
        {
            double[] pixel = { (double)(mapX * tileSize + focuseRect.X + p.X), (double)(mapY * tileSize + focuseRect.Y + p.Y) };

            Console.WriteLine(string.Format("픽셀 위치: {0}, {1}", pixel[0], pixel[1]));

            return GlobalPixelToPosition(pixel, zoom, tileSize);
        }

        /// <summary>
        /// 위도/경도 좌표를 특정 상세 수준의 픽셀 XY 좌표로 변환합니다.
        /// position[0]은 경도, position[1]은 위도입니다.
        /// </summary>
        /// <param name="position">[경도, 위도] 형식의 좌표입니다.</param>
        /// <param name="zoom">확대 수준입니다.</param>
        /// <param name="tileSize">타일 피라미드의 타일 크기입니다.</param> 
        /// <returns>전역 픽셀 좌표입니다.</returns>
        public static double[] PositionToGlobalPixel(double[] position, int zoom, int tileSize)
        {
            var longitude = Clip(position[0], LONG_MIN, LONG_MAX);
            var latitude = Clip(position[1], LAT_MIN, LAT_MAX);

            var x = (longitude + 180) / 360;
            var sinLatitude = Math.Sin(latitude * Math.PI / 180);
            var y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);

            var mapSize = MapSize(zoom, tileSize);

            return new double[] {
                Clip(x * mapSize + 0.5, 0, mapSize - 1),
                Clip(y * mapSize + 0.5, 0, mapSize - 1)
            };
        }

        /// <summary>
        /// [경도, 위도] 형식의 배열을 Point 객체로 변환합니다.
        /// </summary>
        /// <param name="position">[경도, 위도] 형식의 배열입니다.</param>
        /// <param name="zoom">확대 수준입니다.</param>
        /// <param name="tileSize">타일 크기입니다.</param>
        /// <returns>Point 객체입니다.</returns>
        public static Point PositionToPoint(double[] position, int zoom, int tileSize, int mapX, int mapY, Rectangle focuseRect)
        {
            double[] pixel = PositionToGlobalPixel(position, zoom, tileSize);

            return new Point((int)pixel[0] - (mapX * tileSize + focuseRect.X), (int)pixel[1] - (mapY * tileSize + focuseRect.Y));
        }

        /// <summary>
        /// Performs a scale transform on a global pixel value from one zoom level to another.
        /// </summary>
        /// <param name="pixel">Pixel coordinates in the format of [x, y].</param>  
        /// <param name="oldZoom">The zoom level in which the input global pixel value is from.</param>  
        /// <returns>A scale pixel coordinate.</returns>
        public static double[] ScaleGlobalPixel(double[] pixel, double oldZoom, double newZoom)
        {
            var scale = Math.Pow(2, oldZoom - newZoom);

            return new double[] { pixel[0] * scale, pixel[1] * scale };
        }

        /// <summary>
        /// Calculates the XY tile coordinates that a coordinate falls into for a specific zoom level.
        /// </summary>
        /// <param name="position">Position coordinate in the format [longitude, latitude]</param>
        /// <param name="zoom">Zoom level</param>
        /// <param name="tileSize">The size of the tiles in the tile pyramid.</param>
        /// <param name="tileX">Output parameter receiving the tile X position.</param>
        /// <param name="tileY">Output parameter receiving the tile Y position.</param>
        public static void PositionToTileXY(double[] position, out int tileX, out int tileY, int zoom, int tileSize)
        {
            var latitude = Clip(position[1], LAT_MIN, LAT_MAX);
            var longitude = Clip(position[0], LONG_MIN, LONG_MAX);

            var x = (longitude + 180) / 360;
            var sinLatitude = Math.Sin(latitude * Math.PI / 180);
            var y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);

            //tileSize needed in calculations as in rare cases the multiplying/rounding/dividing can make the difference of a pixel which can result in a completely different tile. 
            var mapSize = MapSize(zoom, tileSize);
            tileX = (int)Math.Floor(Clip(x * mapSize + 0.5, 0, mapSize - 1) / (double)tileSize);
            tileY = (int)Math.Floor(Clip(y * mapSize + 0.5, 0, mapSize - 1) / (double)tileSize);
        }
        public static Point PositionToTilePoint(double[] position, int zoom, int tileSize)
        {
            int TileX;
            int TileY;

            PositionToTileXY(position, out TileX, out TileY, zoom, tileSize);

            return new Point(TileX, TileY);
        }
        public static Point PointToTilePoint(Point p, int zoom, int tileSize, int mapX, int mapY, Rectangle focuseRect)
        {
            double[] pixel = { (double)(mapX * tileSize + focuseRect.X + p.X), (double)(mapY * tileSize + focuseRect.Y + p.Y) };

            return PositionToTilePoint(GlobalPixelToPosition(pixel, zoom, tileSize), zoom, tileSize);
        }

        /// <summary>
        /// Converts tile XY coordinates into a quadkey at a specified level of detail.
        /// </summary>
        /// <param name="tileX">Tile X coordinate.</param>
        /// <param name="tileY">Tile Y coordinate.</param>
        /// <param name="zoom">Zoom level</param>
        /// <returns>A string containing the quadkey.</returns>
        public static string TileXYToQuadKey(int tileX, int tileY, int zoom)
        {
            var quadKey = new StringBuilder();
            for (int i = zoom; i > 0; i--)
            {
                char digit = '0';
                int mask = 1 << (i - 1);
                if ((tileX & mask) != 0)
                {
                    digit++;
                }
                if ((tileY & mask) != 0)
                {
                    digit++;
                    digit++;
                }
                quadKey.Append(digit);
            }
            return quadKey.ToString();
        }
        public static string TilePointToQuadKey(Point TilePoint, int zoom)
        {
            return TileXYToQuadKey(TilePoint.X, TilePoint.Y, zoom);
        }

        /// <summary>
        /// Converts a quadkey into tile XY coordinates.
        /// </summary>
        /// <param name="quadKey">Quadkey of the tile.</param>
        /// <param name="tileX">Output parameter receiving the tile X coordinate.</param>
        /// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>
        /// <param name="zoom">Output parameter receiving the zoom level.</param>
        public static void QuadKeyToTileXY(string quadKey, out int tileX, out int tileY, out int zoom)
        {
            tileX = tileY = 0;
            zoom = quadKey.Length;
            for (int i = zoom; i > 0; i--)
            {
                int mask = 1 << (i - 1);
                switch (quadKey[zoom - i])
                {
                    case '0':
                        break;

                    case '1':
                        tileX |= mask;
                        break;

                    case '2':
                        tileY |= mask;
                        break;

                    case '3':
                        tileX |= mask;
                        tileY |= mask;
                        break;

                    default:
                        throw new ArgumentException("Invalid QuadKey digit sequence.");
                }
            }
        }
        public static Point QuadKeyToTilePoint(string quadKey)
        {
            int TileX, TileY, zoom;

            QuadKeyToTileXY(quadKey, out TileX, out TileY, out zoom);

            return new Point(TileX, TileY);
        }

        #endregion
    }
}
