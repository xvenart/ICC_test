using System.Configuration;
using Sharpex2D;
using Sharpex2D.Content;
using Sharpex2D.Input;
using Sharpex2D.Math;
using Sharpex2D.Rendering;
using Sharpex2D.Rendering.Scene;

namespace TankGuiObserver
{
    public class LoginScene : Scene
    {
        protected Font _font;
        protected InputManager _input;
        protected string _server;

        public override void Update(GameTime gameTime)
        {
            UIManager.Update(gameTime);

            var kb = _input.Keyboard.GetState();
            if (kb.IsKeyDown(Keys.Escape))
            {
                SGL.QueryComponents<GuiObserver>().Exit();
            }
            else if (kb.IsKeyDown(Keys.Enter))
            {
                var sceneManager = SGL.QueryComponents<SceneManager>();
                sceneManager.ActiveScene = sceneManager.Get<ObserverScene>();
            }
        }

        public override void Render(RenderDevice renderer, GameTime gameTime)
        {
            var msg = "Нажмите Enter для входа, ESC - для завершения работы";
            var dim = renderer.MeasureString(msg, _font);

            renderer.DrawString(msg, _font, new Vector2(10, 40), Color.White);
            renderer.DrawString($"Сервер: {_server}", _font, new Vector2(10, 45 + dim.Y), Color.White);
        }

        public override void Initialize()
        {
            _input = SGL.QueryComponents<InputManager>();
            _server = ConfigurationManager.AppSettings["server"];
        }

        public override void LoadContent(ContentManager content)
        {
            _font = new Font("Arial", 20f, TypefaceStyle.Regular);
        }
    }
}