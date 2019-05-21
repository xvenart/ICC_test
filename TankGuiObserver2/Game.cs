namespace TankGuiObserver2
{
    using SharpDX;
    using SharpDX.DXGI;
    using SharpDX.Windows;
    using SharpDX.Direct2D1;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;
    using SharpDX.DirectInput;
    using AlphaMode = SharpDX.Direct2D1.AlphaMode;
    using Device = SharpDX.Direct3D11.Device;
    using Factory = SharpDX.DXGI.Factory;

    class Game : System.IDisposable
    {
        RenderForm _renderForm;
        RenderTarget _renderTarget2D;
        SharpDX.Direct2D1.Factory _factory2D;
        Surface _surface;
        SwapChain _swapChain;
        Device _device;

        bool _isClientThreadRunning;
        System.Threading.Thread _clientThread;
        GuiSpectator _spectatorClass;
        System.Threading.CancellationTokenSource _tokenSource;
        GuiObserverCore _guiObserverCore;
        Connector _connector;

        bool _isEnterPressed;
        bool _isTabPressed;
        bool _isFPressed;
        DirectInput _directInput;
        Keyboard _keyboard;
        GameRender _gameRender;

        float _bitmapOpacity;
        SharpDX.Mathematics.Interop.RawRectangleF _destinationRectangle;
        BitmapInterpolationMode _interpolationMode;
        Bitmap _bitmap;

        public Game(string windowName,
            int windowWidth, int windowHeight,
            bool isWindowed = true)
        {
            _renderForm = new RenderForm(windowName);
            _renderForm.Width = windowWidth;
            _renderForm.Height = windowHeight;
            _renderForm.AllowUserResizing = false;

            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(
                        (int)(_renderForm.Width),
                        (int)(_renderForm.Height),
                        new Rational(60, 1),
                        Format.R8G8B8A8_UNorm),
                IsWindowed = isWindowed,
                OutputHandle = _renderForm.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device.CreateWithSwapChain(DriverType.Hardware,
                DeviceCreationFlags.BgraSupport,
                new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 },
                desc, out _device, out _swapChain);

            _factory2D = new SharpDX.Direct2D1.Factory();
            Factory factory = _swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(_renderForm.Handle,
                    WindowAssociationFlags.IgnoreAll);

            Texture2D backBuffer = Texture2D.FromSwapChain<Texture2D>(_swapChain, 0);
            _surface = backBuffer.QueryInterface<Surface>();

            _renderTarget2D = new RenderTarget(_factory2D, _surface, new RenderTargetProperties(
                                 new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));

            //WEB_SOCKET
            string serverString = string.Empty;
            if (!System.IO.Directory.Exists("config"))
            {
                System.IO.Directory.CreateDirectory("config");
            }
            if (!System.IO.File.Exists(@"config/config.txt"))
            {
                System.IO.File.WriteAllText(@"config/config.txt", @"server-ws://127.0.0.1:2000");
            }

            serverString =
                    System.IO.File.ReadAllText(@"config/config.txt")
                    .Split(new char[] { '\n' })[0]
                    .Split(new char[] { '-' })[1];

            _guiObserverCore = new GuiObserverCore(serverString, string.Empty);
            _tokenSource = new System.Threading.CancellationTokenSource();
            _spectatorClass = new GuiSpectator(_tokenSource.Token);
            _connector = new Connector(serverString);

            _gameRender = new GameRender(_renderForm, _factory2D, _renderTarget2D);

            _directInput = new DirectInput();
            _keyboard = new Keyboard(_directInput);
            _keyboard.Properties.BufferSize = 128;
            _keyboard.Acquire();
        }

        public void RunGame()
        {
            RenderLoop.Run(_renderForm, Draw);
        }

        public void Draw()
        {
            _renderTarget2D.BeginDraw();
            KeyboardState kbs = _keyboard.GetCurrentState();//_keyboard.Poll();
            foreach (var key in kbs.PressedKeys)
            {
                if (key == Key.F)
                {
                    bool canChange = _gameRender.GetElapsedMs() > 500;
                    if (canChange)
                    {
                        if (!_isFPressed)
                        {
                            _isFPressed = true;
                        }
                        else
                        {
                            _isFPressed = false;
                        }
                    }
                }
                else if (key == Key.Tab)
                {
                    _isTabPressed = true;
                }
                else if (key == Key.Return && _spectatorClass.Map != null)
                {
                    _isEnterPressed = true;
                }
                else if (key == Key.Escape)
                {
                    _renderForm.Close();
                }
            }

            //Drawing a gama
            if (_isEnterPressed)
            {
                _gameRender.Map = _spectatorClass.Map;
                _gameRender.DrawClientInfo();
                _gameRender.DrawTanks(_spectatorClass.Map.InteractObjects);
                _gameRender.DrawMap();
                _gameRender.DrawInteractiveObjects(_spectatorClass.Map.InteractObjects);
            }
            else
            {
                if (_spectatorClass.Map != null)
                {
                    _gameRender.DrawLogo();
                }
                else
                {
                    _gameRender.DrawWaitingLogo();
                    if (!_connector.ServerRunning)
                    {
                        _connector.IsServerRunning();
                    }
                    else if (!_isClientThreadRunning)
                    {
                        _isClientThreadRunning = true;
                        _clientThread = new System.Threading.Thread(() => {
                            _guiObserverCore.Run(_spectatorClass.Client, _tokenSource.Token);
                        });
                        _clientThread.Start();
                    }
                }
            }

            if (_isFPressed)
            {
                _gameRender.DrawFPS();
            }

            try
            {
                _renderTarget2D.EndDraw();
            }
            catch
            {
            }

            _swapChain.Present(0, PresentFlags.None);
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
            _renderForm.Dispose();
            _renderTarget2D.Dispose();
            _factory2D.Dispose();
            _surface.Dispose();
            _swapChain.Dispose();
            _device.ImmediateContext.ClearState();
            _device.ImmediateContext.Flush();
            _device.Dispose();
            _connector.Dispose();
            _gameRender.Dispose();
        }

    }
}