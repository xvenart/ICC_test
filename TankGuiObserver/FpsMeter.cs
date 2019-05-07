using System;
using Sharpex2D;
using Sharpex2D.Common;
using Sharpex2D.Rendering;
using Color = Sharpex2D.Rendering.Color;
using Vector2 = Sharpex2D.Math.Vector2;

namespace TankGuiObserver
{
    public class FpsMeter : Singleton<FpsMeter>, IGameComponent
    {
        protected Font _font;

        public FpsMeter()
        {
            _font = new Font("Arial", 14f, TypefaceStyle.Italic);
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Render(RenderDevice renderer, GameTime gameTime)
        {
            var fps = 1000 / gameTime.ElapsedGameTime;

            var textPosition = new Vector2(1, 1);
            renderer.DrawString($"FPS: {fps:0.0}; {DateTime.Now:HH:mm:ss}", _font, textPosition, Color.Black);
        }

        public int Order => 10000;
    }
}