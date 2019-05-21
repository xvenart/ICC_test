namespace TankGuiObserver2
{
    using System;
    using System.Collections.Generic;

    using SharpDX;
    using SharpDX.Windows;
    using SharpDX.Direct2D1;
    using SharpDX.DirectWrite;
    using SharpDX.Mathematics.Interop;

    using TankCommon.Enum;
    using TankCommon.Objects;
    using System.Diagnostics;
    using System.Linq;
    using SharpDX.DXGI;

    using AlphaMode = SharpDX.Direct2D1.AlphaMode;

    struct ImmutableObject
    {
        public char BitmapIndex;
        public RawRectangleF Rectangle;
        public ImmutableObject(
            char bitmapIndex,
            RawRectangleF rectangle)
        {
            Rectangle = rectangle;
            BitmapIndex = bitmapIndex;
        }
    }

    struct DestuctiveWalls
    {
        public char ColorBrushIndex;
        public int RowIndex;
        public int ColumnIndex;
        public RawRectangleF Rectangle;
        public DestuctiveWalls(
            char colorBrushIndex,
            int row, int column,
            RawRectangleF rectangle)
        {
            ColorBrushIndex = colorBrushIndex;
            RowIndex = row;
            ColumnIndex = column;
            Rectangle = rectangle;
        }
    }

    public class TextAnimation
    {
        private Stopwatch _textTimer;
        private string _cAnimatedString;
        private string _animatedString;

        public TextAnimation()
        {
            _textTimer = new Stopwatch();
            _textTimer.Start();
        }

        public void SetAnimatedString(string animatedString)
        {
            _animatedString = animatedString;
            _cAnimatedString = animatedString;
        }

        public string GetAnimatedString()
        {
            return _animatedString;
        }

        public void AnimationStart(int ms, string frame)
        {
            if (_textTimer.ElapsedMilliseconds > ms)
            {
                _animatedString += frame;
                if (_animatedString.Length > (_cAnimatedString.Length + 3))
                    _animatedString = _cAnimatedString;
                _textTimer.Reset();
                _textTimer.Start();
            }
        }
    }

    public class TextColorAnimation
    {
        private Stopwatch _textTimer;
        private RawColor4 _color;

        public TextColorAnimation()
        {
            _textTimer = new Stopwatch();
            _textTimer.Start();
        }

        public void AnimationStart(int ms, ref SolidColorBrush brush)
        {
            if (_textTimer.ElapsedMilliseconds > ms)
            {
                _color = brush.Color;

                if (_color.A < 0.8f)
                    _color.A += 0.15f;
                else if (_color.A > 0.0f)
                    _color.A -= 0.15f;

                brush.Color = _color;
                _textTimer.Reset();
                _textTimer.Start();
            }
        }
    }

    class FpsCounter
    {
        private int fps;
        private int ms;
        public int FPSCounter { get; set; }
        public Stopwatch FPSTimer { get; set; }

        public FpsCounter()
        {
            FPSTimer = new Stopwatch();
            FPSTimer.Start();
        }

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public void CalculateFpsMs()
        {
            fps = (int)((1000.0f * FPSCounter) / FPSTimer.ElapsedMilliseconds);
            ms = (int)FPSTimer.ElapsedMilliseconds / FPSCounter;
            FPSTimer.Reset();
            FPSTimer.Stop();
            FPSTimer.Start();
            FPSCounter = 0;
        }

        public override string ToString()
        {
            return string.Format("{0}fps, {1}ms", fps, ms);
        }
    }

    class GameRender : System.IDisposable
    {
        private RenderForm RenderForm;
        private RenderTarget RenderTarget2D;
        private SharpDX.Direct2D1.Factory _factory2D;

        private bool _isImmutableObjectsInitialized;
        private bool _isDestructiveObjectsInitialized;
        private bool _isMapSet;
        private int _mapWidth;
        private int _mapHeight;
        private float _zoomWidth;
        private float _zoomHeight;
        private Map _map;
        public Map Map { set { _map = value; }  }
        private List<ImmutableObject> _immutableMapObjects;
        private List<DestuctiveWalls> _destuctiveWallsObjects;
        private List<TankObject> _clientInfoTanks;
        private RawVector2 _clientInfoLeftPoint;
        private RawVector2 _clientInfoRightPoint;
        private RawColor4 _blackScreen;
        private RectangleF _fullTextBackground;
        private RectangleF _fpsmsTextRect;
        private RectangleF _statusTextRect;
        private RectangleF _logoTextRect;
        private RectangleF _clientInfoRect;
        private RectangleF _clientInfoTextRect;
        private RectangleF _clientInfoListRect;
        private SharpDX.DirectWrite.Factory directFactory;
        private TextFormat _statusTextFormat;
        private TextFormat _fpsmsTextFormat;
        private TextFormat _logoBrushTextFormat;
        private SolidColorBrush[] _mapObjectsColors;
        private FpsCounter _fpsCounter;
        private TextAnimation _textAnimation;
        private TextColorAnimation _textColorAnimation;

        public long GetElapsedMs()
        {
            return _fpsCounter.FPSTimer.ElapsedMilliseconds;
        }

        RawRectangleF dstinationRectangle;
        float opacity;
        BitmapInterpolationMode interpolationMode;
        Bitmap _tankUpBitmap;
        Bitmap _tankDownBitmap;
        Bitmap _tankLeftBitmap;
        Bitmap _tankRightBitmap;

        Bitmap _waterBitmap;
        Bitmap _grassBitmap;
        Bitmap _destructiveWallBitmap;
        Bitmap _wallBitmap;

        Bitmap _bulletSpeedUpgradeBitmap;
        Bitmap _damageUpgradeBitmap;
        Bitmap _healthUpgradeBitmap;
        Bitmap _maxHpUpgradeBitmap;
        Bitmap _speedUpgradeBitmap;

        Bitmap _bulletUpBitmap;

        Bitmap[] _bitmaps;

        public GameRender(
            RenderForm renderForm,
            SharpDX.Direct2D1.Factory factory2D,
            RenderTarget renderTarget)
        {
            RenderForm = renderForm;
            _factory2D = factory2D;
            RenderTarget2D = renderTarget;

            _immutableMapObjects = new List<ImmutableObject>();
            _destuctiveWallsObjects = new List<DestuctiveWalls>();
            _clientInfoTanks = new List<TankObject>(10);

            //textRenderer
            _blackScreen = new RawColor4(0.0f, 0.0f, 0.0f, 1.0f);

            _mapObjectsColors = new SolidColorBrush[] {
            /*0*/ new SolidColorBrush(RenderTarget2D, Color.White),
            /*1*/ new SolidColorBrush(RenderTarget2D, Color.DarkRed),
            /*2*/ new SolidColorBrush(RenderTarget2D, Color.DarkBlue),
            /*3*/ new SolidColorBrush(RenderTarget2D, Color.GreenYellow),
            /*4*/ new SolidColorBrush(RenderTarget2D, Color.SandyBrown),
            /*5*/ new SolidColorBrush(RenderTarget2D, Color.Yellow), //bullet speed
            /*6*/ new SolidColorBrush(RenderTarget2D, Color.Red), //Damage
            /*7*/ new SolidColorBrush(RenderTarget2D, Color.Aquamarine), //Health
            /*8*/ new SolidColorBrush(RenderTarget2D, Color.Blue), //MaxHP
            /*9*/ new SolidColorBrush(RenderTarget2D, Color.CornflowerBlue),//Speed
            /*10*/ new SolidColorBrush(RenderTarget2D, Color.LightYellow), //Bullet
            /*11*/ new SolidColorBrush(RenderTarget2D, Color.White), //_defaultBrush
            /*12*/ new SolidColorBrush(RenderTarget2D, Color.Green), //_greenBrush
            /*13*/ new SolidColorBrush(RenderTarget2D, new RawColor4(0.3f, 0.3f, 0.3f, 0.9f)), //_backgroundBrush
            /*14*/ new SolidColorBrush(RenderTarget2D, new RawColor4(1.0f, 1.0f, 1.0f, 1.0f)) //_logoBrush
            };

            _fpsmsTextRect = new RectangleF(25, 5, 150, 30);
            _fullTextBackground = new RectangleF(
                _fpsmsTextRect.Left, _fpsmsTextRect.Top,
                _fpsmsTextRect.Width, _fpsmsTextRect.Height);
            _logoTextRect = new RectangleF((float)RenderForm.Width / 5, (float)RenderForm.Height / 3, 1500, 100);
            _statusTextRect = new RectangleF(
                _logoTextRect.X + _logoTextRect.X,
                RenderForm.Height - (RenderForm.Height - _logoTextRect.Bottom - 200), 800, 30);
            _clientInfoRect = new RectangleF(1000, 0, 1920 - 1000, 1080);
            _clientInfoTextRect = new RectangleF(
                _clientInfoRect.X + 0.39f * _clientInfoRect.X,
                _clientInfoRect.Y + 0.05f * _clientInfoRect.Height, 300, 100);
            _clientInfoLeftPoint = new RawVector2(_clientInfoRect.X,
                _clientInfoTextRect.Y + 0.6f * _clientInfoTextRect.Height);
            _clientInfoRightPoint = new RawVector2(
                _clientInfoRect.X + _clientInfoRect.Width,
                _clientInfoTextRect.Y + 0.6f * _clientInfoTextRect.Height);
            _clientInfoListRect = new RectangleF(
                1080, _clientInfoRightPoint.Y + 50,
                900, 200);

            directFactory =
                new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);
            _statusTextFormat = new TextFormat(directFactory, "Arial", FontWeight.Regular, FontStyle.Normal, 30.0f);
            _fpsmsTextFormat = new TextFormat(directFactory, "Arial", FontWeight.Regular, FontStyle.Normal, 24.0f);
            _logoBrushTextFormat =
                new TextFormat(directFactory, "Arial", FontWeight.Normal, FontStyle.Italic, 180.0f);

            _textAnimation = new TextAnimation();
            _textAnimation.SetAnimatedString("Waiting for connection to the server");
            _textColorAnimation = new TextColorAnimation();
            _fpsCounter = new FpsCounter();

            //Loading resources
            LoadResources();

        }

        [System.Runtime.CompilerServices.MethodImpl(256)]
        protected void FillBlock(RawRectangleF rectangle, SolidColorBrush brush)
        {
            RenderTarget2D.FillRectangle(rectangle, brush);
        }

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public void DrawFPS()
        {
            ++_fpsCounter.FPSCounter;
            if (_fpsCounter.FPSTimer.ElapsedMilliseconds > 1000)
            {
                _fpsCounter.CalculateFpsMs();
            }
            RenderTarget2D.FillRectangle(_fullTextBackground, _mapObjectsColors[13]);
            RenderTarget2D.DrawText(_fpsCounter.ToString(), _fpsmsTextFormat, _fpsmsTextRect, _mapObjectsColors[12]);
        }

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public void DrawLogo()
        {
            RenderTarget2D.Clear(_blackScreen);
            _textColorAnimation.AnimationStart(300, ref _mapObjectsColors[11]);
            RenderTarget2D.DrawText("Battle City v0.1",
                _logoBrushTextFormat, _logoTextRect, _mapObjectsColors[14]);
            RenderTarget2D.DrawText("Press Enter to start a game",
                _statusTextFormat, _statusTextRect, _mapObjectsColors[11]);
        }

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public void DrawWaitingLogo()
        {
            RenderTarget2D.Clear(_blackScreen);
            _textColorAnimation.AnimationStart(600, ref _mapObjectsColors[11]);
            RenderTarget2D.DrawText("Battle City v0.1",
                _logoBrushTextFormat, _logoTextRect, _mapObjectsColors[14]);
            _textAnimation.AnimationStart(300, ".");
            RenderTarget2D.DrawText(_textAnimation.GetAnimatedString(),
                _statusTextFormat, _statusTextRect, _mapObjectsColors[11]);
        }

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public void DrawMap()
        {
            // рисуем всю карту
            if (!_isMapSet)
            {
                _isMapSet = true;
                _mapWidth = _map.MapWidth;
                _mapHeight = _map.MapHeight;

                _zoomWidth = 1080 / _mapWidth;
                _zoomHeight = RenderTarget2D.Size.Height / _mapHeight;
            }

            //неизменяемые
            RawRectangleF rawRectangleTemp = new RawRectangleF();
            if (!_isImmutableObjectsInitialized)
            {
                _isImmutableObjectsInitialized = true;

                int i, j;

                //#### ################ ###########
                //#### текстуры блоками 5 на 5 ####
                //#### ################ ###########
                int blocksInARow = _mapWidth / 5;
                int blocksInACol = _mapHeight / 5;
                List<SharpDX.Point> walls = new List<SharpDX.Point>();
                List<SharpDX.Point> water = new List<SharpDX.Point>();
                List<SharpDX.Point> grass = new List<SharpDX.Point>();
                for (int r = 0; r < blocksInARow; r++)
                {
                    for (int c = 0; c < blocksInACol; c++)
                    {
                        for (i = (5 * r); i < (5 * r + 5); i++)
                        {
                            for (j = (5 * c); j < (5 * c + 5); j++)
                            {
                                if (_map[i, j] == CellMapType.Wall)
                                {
                                    walls.Add(new SharpDX.Point(i, j));
                                }
                                else if (_map[i, j] == CellMapType.Water)
                                {
                                    water.Add(new SharpDX.Point(i, j));
                                }
                                if (_map[i, j] == CellMapType.Grass)
                                {
                                    grass.Add(new SharpDX.Point(i, j));
                                }
                            }
                        }
                        if (walls.Count == 25)
                        {
                            rawRectangleTemp.Left = 5 * c * _zoomWidth;
                            rawRectangleTemp.Top = 5 * r * _zoomHeight;
                            rawRectangleTemp.Right = (5 * c + 5) * _zoomWidth;
                            rawRectangleTemp.Bottom = (5 * r + 5) * _zoomHeight;
                            _immutableMapObjects.Add(new ImmutableObject((char)0, rawRectangleTemp));
                            walls.Clear();
                        }
                        if (water.Count == 25)
                        {
                            rawRectangleTemp.Left = 5 * c * _zoomWidth;
                            rawRectangleTemp.Top = 5 * r * _zoomHeight;
                            rawRectangleTemp.Right = (5 * c + 5) * _zoomWidth;
                            rawRectangleTemp.Bottom = (5 * r + 5) * _zoomHeight;
                            _immutableMapObjects.Add(new ImmutableObject((char)1, rawRectangleTemp));
                            water.Clear();
                        }
                        if (grass.Count == 25)
                        {
                            rawRectangleTemp.Left = 5 * c * _zoomWidth;
                            rawRectangleTemp.Top = 5 * r * _zoomHeight;
                            rawRectangleTemp.Right = (5 * c + 5) * _zoomWidth;
                            rawRectangleTemp.Bottom = (5 * r + 5) * _zoomHeight;
                            _immutableMapObjects.Add(new ImmutableObject((char)2, rawRectangleTemp));
                            grass.Clear();
                        }
                    }
                }
            }
            else
            {
                foreach (var obj in _immutableMapObjects)
                {
                    RenderTarget2D.DrawBitmap(_bitmaps[obj.BitmapIndex], obj.Rectangle, 1.0f, BitmapInterpolationMode.Linear);
                }
            }

            if (!_isDestructiveObjectsInitialized)
            {
                _isDestructiveObjectsInitialized = true;
                for (var i = 5; i < (_mapHeight - 5); i++)
                {
                    for (var j = 5; j < (_mapWidth - 5); j++)
                    {
                        var c = _map[i, j];
                        if (c == CellMapType.DestructiveWall)
                        {
                            rawRectangleTemp.Left = j * _zoomWidth;
                            rawRectangleTemp.Top = i * _zoomHeight;
                            rawRectangleTemp.Right = j * _zoomWidth + _zoomWidth;
                            rawRectangleTemp.Bottom = i * _zoomHeight + _zoomHeight;
                            _destuctiveWallsObjects.Add(new DestuctiveWalls((char)1, i, j, rawRectangleTemp));
                            RenderTarget2D.DrawBitmap(_bitmaps[3], rawRectangleTemp, 1.0f, BitmapInterpolationMode.Linear);
                        }
                    }
                }
            }
            else
            {
                foreach (var obj in _destuctiveWallsObjects)
                {
                    if (_map[obj.RowIndex, obj.ColumnIndex] == CellMapType.DestructiveWall)
                    {
                        RenderTarget2D.DrawBitmap(_bitmaps[3], obj.Rectangle, 1.0f, BitmapInterpolationMode.Linear);
                    }
                }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public void DrawInteractiveObjects(List<BaseInteractObject> baseInteractObjects)
        {
            RawRectangleF rawRectangleTemp = new RawRectangleF();
            foreach (var obj in baseInteractObjects)
            {
                if (obj is UpgradeInteractObject upgradeObject)
                {
                    switch (upgradeObject.Type)
                    {
                        case UpgradeType.BulletSpeed:
                            {
                                rawRectangleTemp.Left = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Left) * _zoomWidth;
                                rawRectangleTemp.Top = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Top) * _zoomHeight;
                                rawRectangleTemp.Right = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Left + upgradeObject.Rectangle.Width) * _zoomWidth;
                                rawRectangleTemp.Bottom = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Top + upgradeObject.Rectangle.Height) * _zoomHeight;
                                RenderTarget2D.DrawBitmap(_bulletSpeedUpgradeBitmap, rawRectangleTemp, opacity, interpolationMode);
                            }
                            break;
                        case UpgradeType.Damage:
                            {
                                rawRectangleTemp.Left = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Left) * _zoomWidth;
                                rawRectangleTemp.Top = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Top) * _zoomHeight;
                                rawRectangleTemp.Right = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Left + upgradeObject.Rectangle.Width) * _zoomWidth;
                                rawRectangleTemp.Bottom = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Top + upgradeObject.Rectangle.Height) * _zoomHeight;
                                RenderTarget2D.DrawBitmap(_damageUpgradeBitmap, rawRectangleTemp, opacity, interpolationMode);
                            }
                            break;
                        case UpgradeType.Health:
                            {
                                rawRectangleTemp.Left = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Left) * _zoomWidth;
                                rawRectangleTemp.Top = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Top) * _zoomHeight;
                                rawRectangleTemp.Right = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Left + upgradeObject.Rectangle.Width) * _zoomWidth;
                                rawRectangleTemp.Bottom = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Top + upgradeObject.Rectangle.Height) * _zoomHeight;
                                RenderTarget2D.DrawBitmap(_healthUpgradeBitmap, rawRectangleTemp, opacity, interpolationMode);
                            }
                            break;
                        case UpgradeType.MaxHp:
                            {
                                rawRectangleTemp.Left = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Left) * _zoomWidth;
                                rawRectangleTemp.Top = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Top) * _zoomHeight;
                                rawRectangleTemp.Right = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Left + upgradeObject.Rectangle.Width) * _zoomWidth;
                                rawRectangleTemp.Bottom = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Top + upgradeObject.Rectangle.Height) * _zoomHeight;
                                RenderTarget2D.DrawBitmap(_maxHpUpgradeBitmap, rawRectangleTemp, opacity, interpolationMode);
                            }
                            break;
                        case UpgradeType.Speed:
                            {
                                rawRectangleTemp.Left = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Left) * _zoomWidth;
                                rawRectangleTemp.Top = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Top) * _zoomHeight;
                                rawRectangleTemp.Right = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Left + upgradeObject.Rectangle.Width) * _zoomWidth;
                                rawRectangleTemp.Bottom = Convert.ToSingle(upgradeObject.Rectangle.LeftCorner.Top + upgradeObject.Rectangle.Height) * _zoomHeight;
                                RenderTarget2D.DrawBitmap(_speedUpgradeBitmap, rawRectangleTemp, opacity, interpolationMode);
                            }
                            break;
                    }
                }
                else if (obj is BulletObject bulletObject)
                {
                    rawRectangleTemp.Left = Convert.ToSingle(bulletObject.Rectangle.LeftCorner.Left) * _zoomWidth;
                    rawRectangleTemp.Top = Convert.ToSingle(bulletObject.Rectangle.LeftCorner.Top) * _zoomHeight;
                    rawRectangleTemp.Right = Convert.ToSingle(bulletObject.Rectangle.LeftCorner.Left + bulletObject.Rectangle.Width) * _zoomWidth;
                    rawRectangleTemp.Bottom = Convert.ToSingle(bulletObject.Rectangle.LeftCorner.Top + bulletObject.Rectangle.Height) * _zoomHeight;
                    FillBlock(rawRectangleTemp, _mapObjectsColors[10]);
                    //RenderTarget2D.DrawBitmap(_bulletUpBitmap, rawRectangleTemp, opacity, interpolationMode);
                }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public void DrawTanks(List<BaseInteractObject> baseInteractObjects)
        {
            RawRectangleF rawRectangleTemp = new RawRectangleF();
            foreach (var obj in baseInteractObjects)
            {
                if (obj is TankObject tankObject)
                {
                    rawRectangleTemp.Left = Convert.ToSingle(tankObject.Rectangle.LeftCorner.Left) * _zoomWidth;
                    rawRectangleTemp.Top = Convert.ToSingle(tankObject.Rectangle.LeftCorner.Top) * _zoomHeight;
                    rawRectangleTemp.Right = Convert.ToSingle(tankObject.Rectangle.LeftCorner.Left + tankObject.Rectangle.Width) * _zoomWidth;
                    rawRectangleTemp.Bottom = Convert.ToSingle(tankObject.Rectangle.LeftCorner.Top + tankObject.Rectangle.Height) * _zoomHeight;
                    if (tankObject.Direction == DirectionType.Up)
                    {
                        RenderTarget2D.DrawBitmap(_tankUpBitmap, rawRectangleTemp, opacity, interpolationMode);
                    }
                    else if (tankObject.Direction == DirectionType.Down)
                    {
                        RenderTarget2D.DrawBitmap(_tankDownBitmap, rawRectangleTemp, opacity, interpolationMode);
                    }
                    else if (tankObject.Direction == DirectionType.Left)
                    {
                        RenderTarget2D.DrawBitmap(_tankLeftBitmap, rawRectangleTemp, opacity, interpolationMode);
                    }
                    else if (tankObject.Direction == DirectionType.Right)
                    {
                        RenderTarget2D.DrawBitmap(_tankRightBitmap, rawRectangleTemp, opacity, interpolationMode);
                    }
                }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public void DrawClientInfo()
        {
            RenderTarget2D.Clear(_blackScreen);
            RenderTarget2D.FillRectangle(_clientInfoRect, _mapObjectsColors[13]);
            RenderTarget2D.DrawLine(_clientInfoLeftPoint, _clientInfoRightPoint, _mapObjectsColors[12], 10);
            RenderTarget2D.DrawText("Client info", _statusTextFormat, _clientInfoTextRect, _mapObjectsColors[12]);
            _clientInfoTanks.AddRange(
                _map.InteractObjects.OfType<TankObject>().OrderByDescending(t => t.Score).ToList());
            int index = 1;
            RectangleF heightIncriment = _clientInfoListRect;

            string nicknameFormatted = "Nickname            ";//20
            RenderTarget2D.DrawText(
                    $"Id " + nicknameFormatted + " Score Hp",
                    _statusTextFormat, heightIncriment, _mapObjectsColors[12]);
            heightIncriment.Y += _clientInfoListRect.Height / 4;
            int diffLen;
            foreach (var tank in _clientInfoTanks)
            {
                diffLen = Math.Abs(nicknameFormatted.Length - tank.Nickname.Length);
                RenderTarget2D.DrawText(
                    $"{index}. {tank.Nickname}{new string(' ', diffLen)} " +
                    $"{(int)tank.Score} {(int)tank.Hp}",
                    _statusTextFormat, heightIncriment, _mapObjectsColors[12]);
                heightIncriment.Y += _clientInfoListRect.Height / 4;
                ++index;
            }
            _clientInfoTanks.Clear();
        }

        public void LoadResources()
        {
            dstinationRectangle = new RawRectangleF(0, 0, 100, 100);
            opacity = 1.0f;
            interpolationMode = BitmapInterpolationMode.Linear;
            _wallBitmap = LoadFromFile(RenderTarget2D, @"img\wall.png");
            _tankUpBitmap = LoadFromFile(RenderTarget2D, @"img\tank\tankUp.png");
            _tankDownBitmap = LoadFromFile(RenderTarget2D, @"img\tank\tankDown.png");
            _tankLeftBitmap = LoadFromFile(RenderTarget2D, @"img\tank\tankLeft.png");
            _tankRightBitmap = LoadFromFile(RenderTarget2D, @"img\tank\tankRight.png");
            _waterBitmap = LoadFromFile(RenderTarget2D, @"img\water2.png");
            _grassBitmap = LoadFromFile(RenderTarget2D, @"img\Grass_T.png");
            _destructiveWallBitmap = LoadFromFile(RenderTarget2D, @"img\brick4k.png");

            _bulletSpeedUpgradeBitmap = LoadFromFile(RenderTarget2D, @"img\upgrade\BulletSpeed.png");
            _damageUpgradeBitmap = LoadFromFile(RenderTarget2D, @"img\upgrade\Damage.png");
            _healthUpgradeBitmap = LoadFromFile(RenderTarget2D, @"img\upgrade\Health.png");
            _maxHpUpgradeBitmap = LoadFromFile(RenderTarget2D, @"img\upgrade\MaxHp.png");
            _speedUpgradeBitmap = LoadFromFile(RenderTarget2D, @"img\upgrade\Speed.png");

            //_bulletUpBitmap = LoadFromFile(RenderTarget2D, @"img\Bullet_T.png");

            _bitmaps = new Bitmap[] {
                _wallBitmap, _waterBitmap, _grassBitmap,
                _destructiveWallBitmap
            };

        }

        public static Bitmap LoadFromFile(RenderTarget renderTarget, string file)
        {
            // Loads from file using System.Drawing.Image
            using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(file))
            {
                var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var bitmapProperties = new BitmapProperties(
                    new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
                var size = new Size2(bitmap.Width, bitmap.Height);

                // Transform pixels from BGRA to RGBA
                int stride = bitmap.Width * sizeof(int);
                using (var tempStream = new DataStream(bitmap.Height * stride, true, true))
                {
                    // Lock System.Drawing.Bitmap
                    var bitmapData = bitmap.LockBits(sourceArea,
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                    // Convert all pixels 
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        int offset = bitmapData.Stride * y;
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            // Not optimized 
                            byte B = System.Runtime.InteropServices.Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte G = System.Runtime.InteropServices.Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte R = System.Runtime.InteropServices.Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte A = System.Runtime.InteropServices.Marshal.ReadByte(bitmapData.Scan0, offset++);
                            int rgba = R | (G << 8) | (B << 16) | (A << 24);
                            tempStream.Write(rgba);
                        }

                    }
                    bitmap.UnlockBits(bitmapData);
                    tempStream.Position = 0;

                    return new Bitmap(renderTarget, size, tempStream, stride, bitmapProperties);
                }
            }
        }

        public void Dispose()
        {
            for (int mapIndex = 0; mapIndex < _mapObjectsColors.Length; mapIndex++)
            {
                _mapObjectsColors[mapIndex].Dispose();
            }
        }

    }
}