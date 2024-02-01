using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

using static sMap.sAzureMap;

namespace sMap
{
    public enum FOCUSMOVE
    {
        NONE = 0x00,
        INIT = 0x01,
        UP = 0x10,
        DOWN = 0x20,
        RIGHT = 0x40,
        LEFT = 0x80,
    }


    public class MapHome
    {
        public int XTile = -1;
        public int YTile = -1;
        public int ZoomLevel = -1;
        public int dx = -1;
        public int dy = -1;

        public MapHome(int tx, int ty, int zlvl, int dx, int dy)
        {
            XTile = tx;
            YTile = ty;
            ZoomLevel = zlvl;
            this.dx = dx;
            this.dy = dy;
        }
    }

    public partial class sMapControl : Panel
    {
        private ImageAttributes ImgAttribute = new ImageAttributes();

        private static string MainTilePath = "";
        private static string SubTilePath = "";
        private static bool TileLocalPath = true; // true : local, false : web
        
        private static string TileFmt = "";

        
        private static MapHome Home = null;

        //private static Bitmap m_Map;


        private sTileMap _TileMap = new sTileMap();

        #region Draw Task

        public event EventHandler OnMapDrawn;

        // 이 메서드는 지도를 비동기적으로 그리는 역할을 합니다.
        private async Task DrawWorker_DoWorkAsync()
        {
            try
            {
                // 무한 루프를 사용하여 반복적으로 그리기 작업을 수행합니다.
                while (true)
                {
                    // 33밀리초마다 대기합니다. 이는 약 30 FPS의 프레임 속도와 유사합니다.
                    await Task.Delay(33);

                    // 컨트롤이 표시되지 않거나 유효하지 않은 경우 다음 반복으로 건너뜁니다.
                    if (!this.Visible || !this.IsHandleCreated)
                        continue;

                    // UI 스레드가 아닌 경우 BeginInvoke를 사용하여 UI 스레드에서 작업을 수행합니다.
                    if (this.InvokeRequired)
                    {
                        this.BeginInvoke(new MethodInvoker(delegate
                        {
                            // 그래픽 리소스를 사용하여 지도를 그립니다.
                            using (var thisGraphics = this.CreateGraphics())
                            using (var bufferGraphics = BufferedGraphicsManager.Current.Allocate(thisGraphics, this.ClientRectangle))
                            using (Graphics drawGraphics = bufferGraphics.Graphics)
                            {
                                // 성능을 위해 그리기 설정을 조정합니다.
                                drawGraphics.CompositingMode = CompositingMode.SourceCopy;
                                drawGraphics.CompositingQuality = CompositingQuality.HighSpeed;
                                drawGraphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                                drawGraphics.SmoothingMode = SmoothingMode.None;
                                drawGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;

                                // 지도 이미지를 그립니다.
                                drawGraphics.DrawImage(_TileMap.TileMap, this.ClientRectangle, _TileMap.FocusRect, GraphicsUnit.Pixel);

                                // 추가 그리기 옵션을 설정합니다.
                                drawGraphics.CompositingMode = CompositingMode.SourceOver;
                                drawGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                // 추가 그리기 작업이 있으면 실행합니다.
                                OnMapDrawn?.Invoke(drawGraphics, null);

                                // 버퍼에서 화면으로 그립니다.
                                bufferGraphics.Render(thisGraphics);
                            }
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                // 예외 발생 시 콘솔에 메시지를 출력합니다.
                Console.WriteLine(ex.Message);
            }
        }

        #endregion

        /// <summary>
        /// mapPath : MapPath\\ 
        /// tileFmt : png
        /// </summary>
        /// <param name="mapPath"></param>
        /// <param name="tileFmt"></param>
        public sMapControl(string mapPath, string tileFmt = "png")
        {
            InitializeComponent();

            MainTilePath = mapPath;
            TileLocalPath = MainTilePath.ToLower().Substring(0, 4) == "http" ? false : true;
            TileFmt = tileFmt;

            float[][] matrixItems ={
               new float[] {1, 0, 0, 0, 0},
               new float[] {0, 1, 0, 0, 0},
               new float[] {0, 0, 1, 0, 0},
               new float[] {0, 0, 0, 0.5f, 0},
               new float[] {0, 0, 0, 0, 1}};
            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);
            ImgAttribute.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            DrawTileMaps(FOCUSMOVE.INIT);

            GoHome();

            Task.Run(() => DrawWorker_DoWorkAsync());

            _TileMap.MainTilePath = MainTilePath;
            _TileMap.TileLocalPath = TileLocalPath;
            _TileMap.TileFmt = tileFmt;
        }

        public string TileMapPath
        {
            get { return MainTilePath; }
            set
            {
                if (value != MainTilePath)
                {
                    SubTilePath = "";
                    MainTilePath = value;
                    TileLocalPath = MainTilePath.ToLower().Substring(0, 4) == "http" ? false : true;

                    DrawTileMaps(FOCUSMOVE.INIT);
                }
            }
        }
        public string SubTileMapPath
        {
            get { return SubTilePath; }
            set
            {
                if (value == null)
                    SubTilePath = "";
                else if (value != MainTilePath && value != SubTilePath)
                {
                    SubTilePath = value;
                    bool tileinlocal = SubTilePath.ToLower().Substring(0, 4) == "http" ? false : true;

                    if (tileinlocal != TileLocalPath)
                        SubTilePath = "";
                }

                DrawTileMaps(FOCUSMOVE.INIT);
            }
        }
        public float SubMapAlpha
        {
            set
            {
                float[][] matrixItems ={
                   new float[] {1, 0, 0, 0, 0},
                   new float[] {0, 1, 0, 0, 0},
                   new float[] {0, 0, 1, 0, 0},
                   new float[] {0, 0, 0, value, 0},
                   new float[] {0, 0, 0, 0, 1}};
                ColorMatrix colorMatrix = new ColorMatrix(matrixItems);
                ImgAttribute.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                DrawTileMaps(FOCUSMOVE.INIT);
            }
        }

        #region Set Tiles
        private void DrawTileMaps(FOCUSMOVE focus)
        {
//            if (focus == FOCUSMOVE.NONE)
//                return;
//            else if (focus == FOCUSMOVE.INIT)
//            {
//                m_Map?.Dispose();
//                m_Map = _TileMap.GetBitmap();
//            }

//            List<int> Xaxis = new List<int>();
//            List<int> Yaxis = new List<int>();

//            for (int ii = 0; ii < _TileMap.NumTileWidth; ii++)
//                Xaxis.Add(_TileMap.X + ii);

//            for (int ii = 0; ii < _TileMap.NumTileHeight; ii++)
//                Yaxis.Add(_TileMap.Y + ii);

//            try
//            {
//                using (var g = Graphics.FromImage(m_Map))
//                {
//                    using (var _bg = BufferedGraphicsManager.Current.Allocate(g, new Rectangle(0, 0, m_Map.Width, m_Map.Height)))
//                    {
//                        Graphics _graphics = _bg.Graphics;

//                        _graphics.CompositingMode = CompositingMode.SourceOver;
//                        _graphics.CompositingQuality = CompositingQuality.HighSpeed;
//                        _graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
//                        _graphics.SmoothingMode = SmoothingMode.None;
//                        _graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

//                        int _x = 0, _y = 0;

//                        switch (focus)
//                        {
//                            default:
//                            case FOCUSMOVE.INIT:
//                                _graphics.Clear(BackColor);
//                                for (_y = 0; _y < _TileMap.NumTileHeight; _y++)
//                                    for (_x = 0; _x < _TileMap.NumTileWidth; _x++)
//                                    {
//                                        if (TileLocalPath)
//                                        {
//                                            string impath = string.Format(MainTilePath + "{0}\\{1}\\{2}." + TileFmt, _TileMap.MLvlNow, Xaxis[_x], Yaxis[_y]).ToString();
//                                            if (!File.Exists(impath))
//                                                impath = string.Format(MainTilePath + "none.png");

//                                            using (var bmp = new Bitmap(impath))
//                                            {
//                                                _graphics.DrawImage(bmp, _TileMap.GetTileRect(_x, _y));
//#if DEBUG
//                                                _graphics.DrawRectangle(Pens.Magenta, _TileMap.GetTileRect(_x, _y));
//#endif
//                                            }

//                                            if (SubTilePath != "")
//                                            {
//                                                string subimpath = string.Format(SubTilePath + "{0}\\{1}\\{2}." + TileFmt, _TileMap.MLvlNow, Xaxis[_x], Yaxis[_y]).ToString();
//                                                if (File.Exists(subimpath))
//                                                    using (var bmp = new Bitmap(subimpath))
//                                                    {
//                                                        _graphics.DrawImage(bmp, _TileMap.GetTileRect(_x, _y), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, ImgAttribute);
//#if DEBUG
//                                                        _graphics.DrawRectangle(Pens.Magenta, _TileMap.GetTileRect(_x, _y));
//#endif
//                                                    }
//                                            }
//                                        }
//                                        else
//                                        {
//                                            using (var _c = new WebClient())
//                                            using (var _s = _c.OpenRead(string.Format(MainTilePath + "{0}\\{1}\\{2}", _TileMap.MLvlNow, Xaxis[_x], Yaxis[_y]).ToString()))
//                                            using (var _r = new StreamReader(_s))
//                                            using (var bmp = new Bitmap(_r.BaseStream))
//                                            {
//                                                _graphics.DrawImage(bmp, _TileMap.GetTileRect(_x, _y));
//#if DEBUG
//                                                _graphics.DrawRectangle(Pens.Magenta, _TileMap.GetTileRect(_x, _y));
//#endif
//                                            }
//                                        }
//                                    }

//                                break;
//                        }

//                        _bg.Render(g);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("지도 데이터가 없습니다.");
//            }

        }
        #endregion

        #region Zoom
        public void ZoomInOut(bool bZin)
        {
            if (_mouseLClick.X == -1 && _mouseLClick.Y == -1)
                _TileMap.ZoomControl(new Point(this.ClientRectangle.Width / 2, this.ClientRectangle.Height / 2), bZin);
            else
                _TileMap.ZoomControl(_mouseLClick, bZin);

            DrawTileMaps(FOCUSMOVE.INIT);
        }
        public void ZoomInOut(double[] Position)
        {
            _TileMap.ZoomControl(Position);
            
            DrawTileMaps(FOCUSMOVE.INIT);
        }
        #endregion

        #region Homing
        public void SetHome()
        {
            double[] HomePosition = PointToPosition(_mouseRClick, _TileMap);
            double[] GrobalPixel = PositionToGlobalPixel(HomePosition, _TileMap);

            Home.XTile = (int)(GrobalPixel[0] / _TileMap.TileSize);
            Home.YTile = (int)(GrobalPixel[1] / _TileMap.TileSize);
            Home.ZoomLevel = _TileMap.MLvlNow;

            Home.dx = (int)GrobalPixel[0] % _TileMap.TileSize;
            Home.dy = (int)GrobalPixel[1] % _TileMap.TileSize;
        }
        public void GoHome()
        {
            try
            {
                if (Home != null)
                {
                    _TileMap.X = Home.XTile; // 폴더 이름이 X축 값 
                    _TileMap.Y = Home.YTile; // 이미지 이름이 Y축 값

                    _TileMap.MLvlNow = Home.ZoomLevel;

                    _TileMap.SetFocusedRect(Home.dx, Home.dy, this.Size);

                    _TileMap.X -= _TileMap.NumTileWidth / 2;
                    _TileMap.Y -= _TileMap.NumTileHeight / 2;

                    DrawTileMaps(FOCUSMOVE.INIT);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion


        #region Override Functions
        protected override void OnResize(EventArgs e)
        {
            _TileMap.SetFocusedRect(0, 0, this.Size);
            DrawTileMaps(FOCUSMOVE.INIT);
            base.OnResize(e);
        }

        #region Mouse Event

        public bool _viewControl = false;
        private bool _mouseHold = false;
        private Point _mouseLClick = new Point(-1, -1);
        private Point _mouseRClick = new Point(-1, -1);

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    _mouseHold = true;
                    _mouseLClick = e.Location;
                    break;

                case MouseButtons.Right:
                    _mouseRClick = e.Location;
                    break;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_mouseHold && !_viewControl)
                DrawTileMaps(_TileMap.FocusSet((e.X - _mouseLClick.X), (e.Y - _mouseLClick.Y)));

            _mouseLClick = e.Location;
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            _mouseHold = false;
            _mouseLClick = new Point(-1, -1);
            _mouseRClick = new Point(-1, -1);
            base.OnMouseUp(e);
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            ZoomInOut((e.Delta > 0));

            _mouseLClick = e.Location;
            base.OnMouseWheel(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            _mouseLClick = new Point(-1, -1);
            _mouseRClick = new Point(-1, -1);

            base.OnMouseLeave(e);
        }

        #endregion

        #endregion
    }
}
