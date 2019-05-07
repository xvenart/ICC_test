using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Sharpex2D;
using Sharpex2D.Content;
using Sharpex2D.Input;
using Sharpex2D.Rendering;
using Sharpex2D.Rendering.Scene;
using TankClient;
using Keys = Sharpex2D.Input.Keys;
using Rectangle = Sharpex2D.Math.Rectangle;

namespace TankGuiObserver
{
    public class ObserverScene : Scene
    {
        public Thread _clientThread;
        public CancellationTokenSource _tokenSource;

        protected Font _font;
        protected InputManager _input;

        protected List<IGameComponent> _gameComponents;

        protected GuiSpectator _spectator;
        protected TextInfo _textInfo;

        public override void Update(GameTime gameTime)
        {
            if (null == _gameComponents)
            {
                Connect();
            }

            UIManager.Update(gameTime);

            foreach (var gameComponent in _gameComponents)
            {
                gameComponent.Update(gameTime);
            }

            var kb = _input.Keyboard.GetState();
            if (kb.IsKeyDown(Keys.Escape))
            {
                if(!_tokenSource.IsCancellationRequested)
                {
                    _tokenSource.Cancel();
                }
            }

            if (kb.IsKeyDown(Keys.Pause) || kb.IsKeyDown(Keys.P))
            {
                _spectator.IsPaused = !_spectator.IsPaused;
            }

            if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Space) || kb.IsKeyDown(Keys.Enter))
            {
                SaveResults();
            }

            if (!_clientThread.IsAlive)
            {
                SGL.QueryComponents<GuiObserver>().Exit();
            }
        }

        protected void SaveResults()
        {
            var results = new List<string>(_textInfo.Messages);
            File.WriteAllLines($"results_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt", results);
        }

        public override void Render(RenderDevice renderer, GameTime gameTime)
        {
            if (null == _gameComponents)
            {
                return;
            }

            foreach (var gameComponent in _gameComponents)
            {
                gameComponent.Render(renderer, gameTime);
            }
        }

        public override void Initialize()
        {
            _input = SGL.QueryComponents<InputManager>();
            _font = new Font("Arial", 20f, TypefaceStyle.Regular);
        }

        public void Connect()
        {
            var server = ConfigurationManager.AppSettings["server"];

            _tokenSource = new CancellationTokenSource();
            var clientCore = new ClientCore(server, string.Empty);

            _textInfo = new TextInfo(new Rectangle(
                Screen.PrimaryScreen.Bounds.Width * 0.66f + 10,
                0,
                Screen.PrimaryScreen.Bounds.Width - Screen.PrimaryScreen.Bounds.Width * 0.66f - 10,
                Screen.PrimaryScreen.Bounds.Height));

            _spectator = new GuiSpectator(
                    new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width * 0.66f, Screen.PrimaryScreen.Bounds.Height),
                    _textInfo);

            _clientThread = new Thread(() => { clientCore.Run(false, _spectator.Client, _tokenSource.Token); });
            _clientThread.Start();

            _gameComponents = new List<IGameComponent>
            {
                _spectator,
                _textInfo
            };
        }

        public override void LoadContent(ContentManager content)
        {
        }
    }
}