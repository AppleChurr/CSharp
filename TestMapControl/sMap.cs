using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static sMap.sAzureMap;

namespace sMap
{
    public class sTileMap
    {
        #region enum
        /// <summary>
        /// 타일의 크기를 정의하는 열거형입니다.
        /// </summary>
        public enum TILESIZE : int
        {
            Low = 256,
            High = 512
        }
        #endregion

        #region private
        // 지도의 초기 크기를 정의합니다.
        private readonly Size InitSize = new Size(896, 896);

        // 지도의 최소 및 최대 확대 수준을 정의합니다.
        private readonly int MinMapLevel = 0;
        private readonly int MaxMapLevel = 20;

        // 지도의 포커스 영역을 나타내는 사각형입니다.
        private Rectangle m_focusRect = new Rectangle(256, 256, 512, 512);

        #endregion

        #region public
        /// <summary>
        /// 현재 사용 중인 타일의 크기입니다.
        /// </summary>
        public int TileSize = (int)TILESIZE.Low;


        private readonly object nWTilesLock = new object();
        private int m_nWTiles = 4;
        /// <summary>
        /// 지도의 가로 방향 타일 수입니다.
        /// </summary>
        public int NumTileWidth { 
            get { lock(nWTilesLock){ return m_nWTiles; } }
            set { lock(nWTilesLock){ m_nWTiles = value; } }
        }

        private readonly object nHTilesLock = new object();
        private int m_nHTiles = 4;
        /// <summary>
        /// 지도의 세로 방향 타일 수입니다.
        /// </summary>
        public int NumTileHeight {
            get { lock (nHTilesLock) { return m_nHTiles; } }
            set { lock (nHTilesLock) { m_nHTiles = value; } }
        }

        /// <summary>
        /// 지도의 포커스 영역을 나타내는 사각형입니다.
        /// </summary>
        public Rectangle FocusRect { get { return m_focusRect; } set { m_focusRect = value; } }

        /// <summary>
        /// 지도의 포커스 영역의 X 좌표입니다.
        /// </summary>
        public int FocusX { get { return X * TileSize + m_focusRect.X; } }

        /// <summary>
        /// 지도의 포커스 영역의 Y 좌표입니다.
        /// </summary>
        public int FocusY { get { return Y * TileSize + m_focusRect.Y; } }

        /// <summary>
        /// 지도의 전체 너비입니다.
        /// </summary>
        public int Width { get { return TileSize * NumTileWidth; } }

        /// <summary>
        /// 지도의 전체 높이입니다.
        /// </summary>
        public int Height { get { return TileSize * NumTileHeight; } }

        /// <summary>
        /// 지도의 크기를 나타내는 Size 객체입니다.
        /// </summary>
        public Size Size { get { return new Size(new Point(Width, Height)); } }

        private readonly object xLock = new object();
        private int m_x = 107;
        /// <summary>
        /// 지도의 X축 타일 인덱스입니다.
        /// </summary>
        public int X
        {
            get { lock (xLock) { return m_x; } }
            set { lock (xLock) { m_x = value; } }
        }

        private readonly object yLock = new object();
        private int m_y = 48;
        /// <summary>
        /// 지도의 Y축 타일 인덱스입니다.
        /// </summary>
        public int Y
        {
            get { lock (yLock) { return m_y; } }
            set { lock (yLock) { m_y = value; } }
        }

        private readonly object mLevelLock = new object();
        private int m_mLvlNow = 7;
        /// <summary>
        /// 현재 지도의 확대 수준입니다.
        /// </summary>
        public int MLvlNow
        {
            get { lock (mLevelLock) { return m_mLvlNow; } }
            set { lock (mLevelLock) { m_mLvlNow = value; } }
        }

        private int m_mLvlMin = 5;
        /// <summary>
        /// 지도의 최소 확대 수준입니다.
        /// </summary>
        public int MLvlMin
        {
            get { lock (mLevelLock) { return m_mLvlMin; } }
            set { lock (mLevelLock) { m_mLvlMin = value; } }
        }

        private int m_mLvlMax = 9;
        /// <summary>
        /// 지도의 최대 확대 수준입니다.
        /// </summary>
        public int MLvlMax
        {
            get { lock (mLevelLock) { return m_mLvlMax; } }
            set { lock (mLevelLock) { m_mLvlMax = value; } }
        }
        #endregion

        public string MainTilePath = "";
        public string SubTilePath = "";
        public string TileFmt = "";
        public bool TileLocalPath = true; // true : local, false : web

        private readonly object mapLock = new object();
        private Bitmap m_Map;
        public Bitmap TileMap { 
            get { lock (mapLock) { return (Bitmap)m_Map.Clone(); } } 
            set { lock (mapLock) { m_Map = (Bitmap)value.Clone(); } }
        }

        public sTileMap()
        {
            // Bitmap 객체 초기화
            TileMap = GetBitmap();

            // 비동기 태스크 시작
            Task.Run(() => DrawTileMapsAsync());
        }



        private async Task DrawTileMapsAsync()
        {
            while (true)
            {
                await Task.Delay(11);

                if (MainTilePath == "" || TileFmt == "")
                    continue;

                int nWTiles = NumTileWidth;
                int nHTiles = NumTileHeight;

                List<int> Xaxis = new List<int>();
                List<int> Yaxis = new List<int>();

                for (int ii = 0; ii < nWTiles; ii++)
                    Xaxis.Add(X + ii);

                for (int ii = 0; ii < nHTiles; ii++)
                    Yaxis.Add(Y + ii);

                try
                {
                    using (Bitmap map = GetBitmap())
                    {
                        using (var g = Graphics.FromImage(map))
                        using (var _bg = BufferedGraphicsManager.Current.Allocate(g, new Rectangle(0, 0, map.Width, map.Height)))
                        {
                            Graphics _graphics = _bg.Graphics;

                            _graphics.CompositingMode = CompositingMode.SourceOver;
                            _graphics.CompositingQuality = CompositingQuality.HighSpeed;
                            _graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                            _graphics.SmoothingMode = SmoothingMode.None;
                            _graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

                            int _x = 0, _y = 0;

                            for (_y = 0; _y < nHTiles; _y++)
                                for (_x = 0; _x < nWTiles; _x++)
                                {
                                    if (TileLocalPath)
                                    {
                                        string impath = string.Format(MainTilePath + "{0}\\{1}\\{2}." + TileFmt, MLvlNow, Xaxis[_x], Yaxis[_y]).ToString();
                                        if (!File.Exists(impath))
                                            impath = string.Format(MainTilePath + "none.png");

                                        using (var bmp = new Bitmap(impath))
                                        {
                                            _graphics.DrawImage(bmp, GetTileRect(_x, _y));
#if DEBUG
                                            _graphics.DrawRectangle(Pens.Magenta, GetTileRect(_x, _y));
#endif
                                        }
                                    }
                                }

                            _bg.Render(g);
                        }

                        lock (mapLock)
                        {
                            TileMap = map;
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


        #region mathod

        /// <summary>
        /// 지도의 포커스 영역을 설정합니다.
        /// </summary>
        /// <param name="dx">X축 이동 거리입니다.</param>
        /// <param name="dy">Y축 이동 거리입니다.</param>
        /// <param name="clientSize">클라이언트 영역의 크기입니다.</param>
        public void SetFocusedRect(int dx, int dy, Size clientSize)
        {
            m_focusRect = new Rectangle(TileSize + dx, TileSize + dy, clientSize.Width, clientSize.Height);

            int nWTiles = (int)(((double)clientSize.Width / (double)TileSize) + 3.0f);
            int nHTiles = (int)(((double)clientSize.Height / (double)TileSize) + 3.0f);

            Console.WriteLine($"Num Tile Changed >> {nWTiles}, {nHTiles}");

            if (nWTiles == 3 && nHTiles == 3)
            {
                nWTiles = (int)(((double)InitSize.Width / (double)TileSize) + 3.0f);
                nHTiles = (int)(((double)InitSize.Height / (double)TileSize) + 3.0f);
            }

            NumTileWidth = nWTiles;
            NumTileHeight = nHTiles;
        }

        /// <summary>
        /// 지도의 포커스를 이동시키고 해당 방향을 반환합니다.
        /// </summary>
        /// <param name="_dX">X축 이동 거리입니다.</param>
        /// <param name="_dY">Y축 이동 거리입니다.</param>
        /// <returns>이동 방향을 나타내는 FOCUSMOVE 열거형 값입니다.</returns>
        public FOCUSMOVE FocusSet(int _dX, int _dY)
        {
            m_focusRect.X -= _dX;
            m_focusRect.Y -= _dY;

            FOCUSMOVE _focus = FOCUSMOVE.NONE;

            int _stepX = 0;
            int _stepY = 0;

            if (m_focusRect.X <= 0) // 포커스 X가 음수방향으로 이동하는 것은 전체 지도 기준으로 보면, 지도를 오른쪽으로 미는 것
            {
                _stepX = -1 * ( (Math.Abs(m_focusRect.X) / TileSize) + 1);
                _focus |= FOCUSMOVE.RIGHT;
            }
            else if (m_focusRect.X >= 2 * TileSize)
            {
                _stepX = ((m_focusRect.X - 2 * TileSize) / TileSize) + 1;
                _focus |= FOCUSMOVE.LEFT;
            }

            X += _stepX;
            m_focusRect.X -= (TileSize * _stepX);

            if (m_focusRect.Y <= 0)
            {
                _stepY = -1 *( (Math.Abs(m_focusRect.Y) / TileSize) + 1);
                _focus |= FOCUSMOVE.DOWN;
            }
            else if (m_focusRect.Y >= 2 * TileSize)
            {
                _stepY = ((m_focusRect.Y - 2 * TileSize) / TileSize) + 1;
                _focus |= FOCUSMOVE.UP;
            }

            Y += _stepY;
            m_focusRect.Y -= (TileSize * _stepY);

            return _focus;
        }

        /// <summary>
        /// 지정된 타일의 사각형 영역을 반환합니다.
        /// </summary>
        /// <param name="x">타일의 X축 인덱스입니다.</param>
        /// <param name="y">타일의 Y축 인덱스입니다.</param>
        /// <returns>타일의 Rectangle 객체입니다.</returns>
        public Rectangle GetTileRect(int x, int y)
        {
            return new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);
        }

        /// <summary>
        /// 지도의 현재 크기에 맞는 Bitmap 객체를 생성합니다.
        /// </summary>
        /// <returns>새로운 Bitmap 객체입니다.</returns>
        public Bitmap GetBitmap()
        {
           return new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
        }

        /// <summary>
        /// 지도의 확대 또는 축소를 수행합니다.
        /// </summary>
        /// <param name="_point">중심점으로 사용될 Point 객체입니다.</param>
        /// <param name="bZin">확대할 경우 true, 축소할 경우 false입니다.</param>
        public void ZoomControl(Point _point, bool bZin)
        {
            double[] _oldP;
            double[] _newP;

            _oldP = PointToPosition(_point, this);

            Point Tilep = PositionToTilePoint(_oldP, MLvlNow, TileSize);
            string qdk = TileXYToQuadKey(Tilep.X, Tilep.Y, MLvlNow);

            if (bZin)
            {
                Point nTileP = QuadKeyToTilePoint(qdk + "0");

                MLvlNow++;
                if (MLvlNow > MLvlMax)
                    MLvlNow = MLvlMax;
                else
                {
                    X = (nTileP.X - 2);
                    Y = (nTileP.Y - 2);
                }
            }
            else
            {
                Point nTileP = QuadKeyToTilePoint(qdk.Substring(0, qdk.Length - 1));

                MLvlNow--;
                if (MLvlNow < MLvlMin)
                    MLvlNow = MLvlMin;
                else
                {
                    X = (nTileP.X - 3);
                    Y = (nTileP.Y - 3);
                }
            }
            
            _newP = PointToPosition(_point, this);

            Point _oP = PositionToPoint(_oldP, this);
            Point _nP = PositionToPoint(_newP, this);

            FocusSet(_nP.X - _oP.X, _nP.Y - _oP.Y);
        }

        /// <summary>
        /// 특정 위치에 대해 지도의 확대 수준을 조절합니다.
        /// </summary>
        /// <param name="Position">[경도, 위도] 형식의 배열입니다.</param>
        public void ZoomControl(double[] Position)
        {
            double[] GrobalPixel = PositionToGlobalPixel(Position, this);

            X = (int)(GrobalPixel[0] / TileSize); // 폴더 이름이 X축 값 
            Y = (int)(GrobalPixel[1] / TileSize); // 이미지 이름이 Y축 값

            MLvlNow = MLvlMax;

            int dX = (int)GrobalPixel[0] % TileSize;
            int dY = (int)GrobalPixel[1] % TileSize;
            SetFocusedRect(dX, dY, this.Size);

            X -= NumTileWidth / 2;
            Y -= NumTileHeight / 2;
        }

        #endregion
    }


    public static class sAzureMap
    {
        // 위도 및 경도의 최소 및 최대 범위 상수입니다.
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
        /// 화면상의 포인트를 [경도, 위도] 형식으로 변환합니다. 이 메서드는 sTileMap 클래스의 인스턴스를 사용하여 화면상의 포인트를 지리적 위치로 변환합니다.
        /// </summary>
        /// <param name="p">화면상의 포인트를 나타내는 Point 객체입니다.</param>
        /// <param name="tileMap">sTileMap 클래스의 인스턴스입니다.</param>
        /// <returns>[경도, 위도] 형식의 배열입니다.</returns>
        public static double[] PointToPosition(Point p, sTileMap tileMap)
        {
            double[] pixel = { (double)(tileMap.FocusX + p.X), (double)(tileMap.FocusY + p.Y) };

            Console.WriteLine(string.Format("픽셀 위치: {0}, {1}", pixel[0], pixel[1]));

            return GlobalPixelToPosition(pixel, tileMap.MLvlNow, tileMap.TileSize);
        }

        /// <summary>
        /// 지리적 위치를 픽셀 XY 좌표로 변환합니다. 이 메서드는 sTileMap 클래스의 인스턴스를 사용하여 지리적 위치를 픽셀 좌표로 변환합니다.
        /// </summary>
        /// <param name="position">[경도, 위도] 형식의 지리적 위치입니다.</param>
        /// <param name="tileMap">sTileMap 클래스의 인스턴스입니다.</param>
        /// <returns>픽셀 XY 좌표입니다.</returns>
        public static double[] PositionToGlobalPixel(double[] position, sTileMap tileMap)
        {
            var longitude = Clip(position[0], LONG_MIN, LONG_MAX);
            var latitude = Clip(position[1], LAT_MIN, LAT_MAX);

            var x = (longitude + 180) / 360;
            var sinLatitude = Math.Sin(latitude * Math.PI / 180);
            var y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);

            var mapSize = MapSize(tileMap.MLvlNow, tileMap.TileSize);

            return new double[] {
                Clip(x * mapSize + 0.5, 0, mapSize - 1),
                Clip(y * mapSize + 0.5, 0, mapSize - 1)
            };
        }

        /// <summary>
        /// [경도, 위도] 형식의 배열을 Point 객체로 변환합니다. 이 메서드는 sTileMap 클래스의 인스턴스를 사용하여 지리적 위치를 화면상의 포인트로 변환합니다.
        /// </summary>
        /// <param name="position">[경도, 위도] 형식의 지리적 위치입니다.</param>
        /// <param name="tileMap">sTileMap 클래스의 인스턴스입니다.</param>
        /// <returns>화면상의 Point 객체입니다.</returns>
        public static Point PositionToPoint(double[] position, sTileMap tileMap)
        {
            double[] pixel = PositionToGlobalPixel(position, tileMap);

            return new Point((int)pixel[0] - tileMap.FocusX, (int)pixel[1] - tileMap.FocusY);
        }

        /// <summary>
        /// 한 확대 수준에서 다른 확대 수준으로 글로벌 픽셀 값을 변환합니다.
        /// </summary>
        /// <param name="pixel">[x, y] 형식의 픽셀 좌표입니다.</param>  
        /// <param name="oldZoom">입력된 글로벌 픽셀 값의 확대 수준입니다.</param>  
        /// <param name="newZoom">새로운 확대 수준입니다.</param>  
        /// <returns>변환된 픽셀 좌표입니다.</returns>
        public static double[] ScaleGlobalPixel(double[] pixel, double oldZoom, double newZoom)
        {
            var scale = Math.Pow(2, oldZoom - newZoom);

            return new double[] { pixel[0] * scale, pixel[1] * scale };
        }

        /// <summary>
        /// 특정 확대 수준에서 좌표가 속하는 XY 타일 좌표를 계산합니다.
        /// </summary>
        /// <param name="position">[경도, 위도] 형식의 좌표입니다.</param>
        /// <param name="zoom">확대 수준입니다.</param>
        /// <param name="tileSize">타일 피라미드의 타일 크기입니다.</param>
        /// <param name="tileX">타일 X 위치를 받는 출력 파라미터입니다.</param>
        /// <param name="tileY">타일 Y 위치를 받는 출력 파라미터입니다.</param>
        public static void PositionToTileXY(double[] position, int zoom, int tileSize, out int tileX, out int tileY)
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

        /// <summary>
        /// [경도, 위도] 형식의 좌표를 Point 객체로 변환합니다 (타일 좌표).
        /// </summary>
        /// <param name="position">[경도, 위도] 형식의 좌표입니다.</param>
        /// <param name="zoom">확대 수준입니다.</param>
        /// <param name="tileSize">타일 크기입니다.</param>
        /// <returns>타일 좌표를 나타내는 Point 객체입니다.</returns>
        public static Point PositionToTilePoint(double[] position, int zoom, int tileSize)
        {
            int TileX, TileY;

            PositionToTileXY(position, zoom, tileSize, out TileX, out TileY);

            return new Point(TileX, TileY);
        }

        /// <summary>
        /// Point 객체를 타일 XY 좌표로 변환합니다. 이 메서드는 화면상의 특정 포인트를 지도 타일 시스템의 타일 좌표로 변환하는 데 사용됩니다.
        /// </summary>
        /// <param name="p">화면상의 포인트를 나타내는 Point 객체입니다.</param>
        /// <param name="zoom">현재 지도의 확대 수준입니다.</param>
        /// <param name="tileSize">지도 타일의 크기입니다. 이 값은 타일 시스템에 따라 다를 수 있습니다.</param>
        /// <param name="mapX">현재 지도의 X축 타일 인덱스입니다. 이는 지도의 가로 위치를 결정합니다.</param>
        /// <param name="mapY">현재 지도의 Y축 타일 인덱스입니다. 이는 지도의 세로 위치를 결정합니다.</param>
        /// <param name="focuseRect">현재 지도 뷰의 포커스 영역을 나타내는 Rectangle 객체입니다. 이 영역은 지도 내에서 사용자가 보고 있는 부분을 정의합니다.</param>
        /// <returns>주어진 Point에 해당하는 지도 타일 시스템의 타일 좌표를 나타내는 Point 객체입니다.</returns>
        public static Point PointToTilePoint(Point p, int zoom, int tileSize, int mapX, int mapY, Rectangle focuseRect)
        {
            double[] pixel = { (double)(mapX * tileSize + focuseRect.X + p.X), (double)(mapY * tileSize + focuseRect.Y + p.Y) };

            return PositionToTilePoint(GlobalPixelToPosition(pixel, zoom, tileSize), zoom, tileSize);
        }

        /// <summary>
        /// 타일 XY 좌표를 특정 상세 수준의 quadkey로 변환합니다.
        /// </summary>
        /// <param name="tileX">타일 X 좌표입니다.</param>
        /// <param name="tileY">타일 Y 좌표입니다.</param>
        /// <param name="zoom">확대 수준입니다.</param>
        /// <returns>quadkey를 나타내는 문자열입니다.</returns>
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

        /// <summary>
        /// quadkey를 Point 객체로 변환합니다 (타일 좌표).
        /// </summary>
        /// <param name="quadKey">변환할 quadkey입니다.</param>
        /// <returns>타일 좌표를 나타내는 Point 객체입니다.</returns>
        public static Point QuadKeyToTilePoint(string quadKey)
        {
            int TileX, TileY, zoom;

            QuadKeyToTileXY(quadKey, out TileX, out TileY, out zoom);

            return new Point(TileX, TileY);
        }

        /// <summary>
        /// quadkey를 타일 XY 좌표로 변환합니다.
        /// </summary>
        /// <param name="quadKey">타일의 quadkey입니다.</param>
        /// <param name="tileX">타일 X 좌표를 받는 출력 파라미터입니다.</param>
        /// <param name="tileY">타일 Y 좌표를 받는 출력 파라미터입니다.</param>
        /// <param name="zoom">확대 수준을 받는 출력 파라미터입니다.</param>
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

        #endregion
    }
}
