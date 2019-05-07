using System;
using System.Collections.Generic;
using System.Linq;
using Sharpex2D;
using Sharpex2D.Math;
using Sharpex2D.Rendering;
using Color = Sharpex2D.Rendering.Color;
using Rectangle = Sharpex2D.Math.Rectangle;
using TankClient;
using TankCommon;
using TankCommon.Enum;
using TankCommon.Objects;

namespace TankGuiObserver
{
    public class GuiSpectator : IClientBot, IGameComponent
    {
        public bool IsPaused { get; set; }

        protected Map _map;
        protected Map _drawingMap;

        protected DateTime _lastMapUpdate;
        protected readonly object _syncObject = new object();
        protected int _msgCount;
        protected bool _wasUpdate;

        protected Rectangle _visibleArea;

        protected TextInfo _textInfo;

        protected Pen _pen;
        protected Pen _pen2;

        protected Font _bigFont;

        protected float zoomWidth;
        protected float zoomHeight;

        protected DateTime _lastServerUpdate = DateTime.Now;

        public GuiSpectator(Rectangle visibleArea, TextInfo textInfo)
        {
            _visibleArea = visibleArea;
            _textInfo = textInfo;

            _pen = new Pen(Color.Red, 3);
            _pen2 = new Pen(Color.LightBlue, 1);

            _bigFont = new Font("Times", 128f, TypefaceStyle.Bold);
        }

        protected void FillBlock(RenderDevice renderer, Rectangle rectangle, Color color)
        {
            renderer.FillRectangle(color, rectangle);
            renderer.DrawRectangle(_pen2, rectangle);
        }

        public ServerResponse Client(int msgCount, ServerRequest request)
        {
            lock (_syncObject)
            {
                _lastServerUpdate = DateTime.Now;

                if (request.Map.Cells != null)
                {
                    _map = request.Map;
                    _lastMapUpdate = DateTime.Now;
                }
                else if (null == _map)
                {
                    return new ServerResponse {ClientCommand = ClientCommandType.UpdateMap};
                }

                _map.InteractObjects = request.Map.InteractObjects;
                _msgCount = msgCount;
                _wasUpdate = true;

                return new ServerResponse {ClientCommand = ClientCommandType.None};
            }
        }

        public void Update(GameTime gameTime)
        {
            if (IsPaused)
            {
                return;
            }

            if (_wasUpdate)
            {
                lock (_syncObject)
                {
                    _wasUpdate = false;
                    _drawingMap = new Map(_map, _map.InteractObjects);
                }
            }
        }

        public void RenderPause(RenderDevice renderer)
        {
            var dim = renderer.MeasureString("П А У З А", _bigFont);
            var pos = new Vector2((_visibleArea.Width - dim.X) / 2, (_visibleArea.Height - dim.Y) / 2);

            renderer.DrawString("П А У З А", _bigFont, pos, Color.Yellow);
        }

        public void Render(RenderDevice renderer, GameTime gameTime)
        {
            if (null == _drawingMap)
            {
                if (IsPaused)
                {
                    RenderPause(renderer);
                }

                return;
            }

            // рисуем всю карту
            var color = Color.Black;

            zoomWidth = _visibleArea.Width / _drawingMap.MapWidth;
            zoomHeight = _visibleArea.Height / _drawingMap.MapHeight;

            for (var i = 0; i < _drawingMap.MapHeight; i++)
            {
                for (var j = 0; j < _drawingMap.MapWidth; j++)
                {
                    var c = _drawingMap[i, j];
                    switch (c)
                    {
                        case CellMapType.Void:
                            color = Color.Black;
                            break;
                        case CellMapType.Wall:
                            color = Color.White;
                            break;
                        case CellMapType.DestructiveWall:
                            color = Color.DarkRed;
                            break;
                        case CellMapType.Water:
                            color = Color.DarkBlue;
                            break;
                        case CellMapType.Grass:
                            continue;
                        default:
                            color = Color.Gray;
                            break;
                    }

                    var r = new Rectangle(j * zoomWidth, i * zoomHeight, zoomWidth, zoomHeight);
                    FillBlock(renderer, r, color);
                }
            }

            // рисуем танки
            foreach (var tank in _drawingMap.InteractObjects.OfType<TankObject>())
            {
                color = Color.SandyBrown;
                var r = new Rectangle(Convert.ToSingle(tank.Rectangle.LeftCorner.Left) * zoomWidth,
                    Convert.ToSingle(tank.Rectangle.LeftCorner.Top) * zoomHeight,
                    Convert.ToSingle(tank.Rectangle.Width) * zoomWidth,
                    Convert.ToSingle(tank.Rectangle.Height) * zoomHeight);
                FillBlock(renderer, r, color);
            }

            // из карты рисуем траву
            for (var i = 0; i < _drawingMap.MapHeight; i++)
            {
                for (var j = 0; j < _drawingMap.MapWidth; j++)
                {
                    var c = _drawingMap[i, j];
                    switch (c)
                    {
                        case CellMapType.Grass:
                            color = Color.GreenYellow;
                            break;
                        default:
                            continue;
                    }

                    var r = new Rectangle(j * zoomWidth, i * zoomHeight, zoomWidth, zoomHeight);
                    FillBlock(renderer, r, color);
                }
            }

            // рисуем все объекты кроме танков
            foreach (var obj in _drawingMap.InteractObjects)
            {
                if (obj is TankObject)
                {
                    continue;
                }

                if (obj is UpgradeInteractObject upgradeObject)
                {
                    switch (upgradeObject.Type)
                    {
                        case UpgradeType.BulletSpeed:
                            color = Color.Yellow;
                            break;
                        case UpgradeType.Damage:
                            color = Color.Red;
                            break;
                        case UpgradeType.Health:
                            color = Color.Aquamarine;
                            break;
                        case UpgradeType.MaxHp:
                            color = Color.Blue;
                            break;
                        case UpgradeType.Speed:
                            color = Color.CornflowerBlue;
                            break;
                    }
                }
                else if (obj is BulletObject bulletObject)
                {
                    //if (bulletObject.Direction == DirectionType.Left || bulletObject.Direction == DirectionType.Right)
                    //{
                    //    co = '—';
                    //}
                    //else
                    //{
                    //    co = '|';
                    //}

                    color = Color.LightYellow;
                }
                else
                {
                    color = Color.Gray;
                }

                var r = new Rectangle(Convert.ToSingle(obj.Rectangle.LeftCorner.Left) * zoomWidth,
                    Convert.ToSingle(obj.Rectangle.LeftCorner.Top) * zoomHeight,
                    Convert.ToSingle(obj.Rectangle.Width) * zoomWidth,
                    Convert.ToSingle(obj.Rectangle.Height) * zoomHeight);
                FillBlock(renderer, r, color);
            }

            renderer.DrawRectangle(_pen, _visibleArea);

            var tanks = _drawingMap.InteractObjects.OfType<TankObject>().OrderByDescending(t => t.Score).ToList();

            var msgs = new List<string> {$"Клиентов: {tanks.Count}"};

            var idx = 1;
            foreach (var tank in tanks)
            {
                msgs.Add($"{idx}. {tank.Nickname}: {tank.Score}; HP: {tank.Hp} / {tank.MaximumHp}; L: {tank.Rectangle.LeftCorner.Left}; T: {tank.Rectangle.LeftCorner.Top}");
                idx++;
            }
            _textInfo.Messages = msgs;

            if (IsPaused)
            {
                RenderPause(renderer);
            }
        }

        public int Order => 1;
    }
}
