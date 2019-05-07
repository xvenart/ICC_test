using System.Collections.Generic;
using Sharpex2D;
using Sharpex2D.Math;
using Sharpex2D.Rendering;

namespace TankGuiObserver
{
    public class TextInfo : IGameComponent
    {
        public List<string> Messages = new List<string>();

        protected Rectangle _visibleArea;
        protected Font _font;

        public TextInfo(Rectangle visibleArea)
        {
            _visibleArea = visibleArea;
            _font = new Font("Arial", 12f, TypefaceStyle.Regular);
        }


        public void Update(GameTime gameTime)
        {
        }

        public void Render(RenderDevice renderer, GameTime gameTime)
        {
            var left = _visibleArea.Left;
            var top = _visibleArea.Top;
            foreach (var message in Messages)
            {
                var dim = renderer.MeasureString(message, _font);
                var pos = new Vector2(left, top);
                renderer.DrawString(message, _font, pos, Color.White);
                top += dim.Y + 5;
            }
        }

        public int Order => 1;
    }
}