using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TankCommon;
using TankCommon.Enum;
using TankCommon.Objects;

namespace TankClient
{
    public class Spectator : IClientBot
    {
        protected Map _map;
        protected DateTime _lastMapUpdate;
        protected readonly CancellationToken _cancellationToken;
        protected readonly object _syncObject = new object();
        protected int _msgCount;
        protected bool _wasUpdate;

        public Spectator(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
#pragma warning disable 4014
            DisplayMap();
#pragma warning restore 4014
        }

        protected async Task DisplayMap()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100);
                if (!_wasUpdate)
                {
                    continue;
                }

                Map map;
                lock (_syncObject)
                {
                    _wasUpdate = false;
                    map = new Map(_map, _map.InteractObjects);
                }

                var dw = 0;

                Console.CursorTop = 0;
                Console.CursorLeft = 0;

                for (var i = 0; i < map.MapHeight; i++)
                {
                    Console.Write(new string(' ', Console.WindowWidth - 1));
                    Console.CursorLeft = 0;
                    for (var j = 0; j < map.MapWidth; j++)
                    {

                        var co = '?';
                        var c = map[i, j];
                        if (c == CellMapType.DestructiveWall)
                        {
                            dw++;
                        }

                        switch (c)
                        {
                            case CellMapType.Void:
                                Console.BackgroundColor = ConsoleColor.Black;
                                co = ' ';
                                break;
                            case CellMapType.Wall:
                                Console.BackgroundColor = ConsoleColor.White;
                                co = '#';
                                break;
                            case CellMapType.DestructiveWall:
                                Console.BackgroundColor = ConsoleColor.DarkRed;
                                co = ':';
                                break;
                            case CellMapType.Water:
                                Console.BackgroundColor = ConsoleColor.DarkBlue;
                                co = '~';
                                break;
                            case CellMapType.Grass:
                                Console.BackgroundColor = ConsoleColor.Green;
                                co = '_';
                                break;
                        }

                        var point = new Point(j, i);
                        var intersectObject = MapManager.GetObjectAtPoint(point, map.InteractObjects);
                        if (intersectObject != null)
                        {
                            if (intersectObject is UpgradeInteractObject upgradeObject)
                            {
                                if (co == ' ')
                                {
                                    switch (upgradeObject.Type)
                                    {
                                        case UpgradeType.BulletSpeed:
                                            {
                                                co = '™';
                                                break;
                                            }
                                        case UpgradeType.Damage:
                                            {
                                                co = '@';
                                                break;
                                            }
                                        case UpgradeType.Health:
                                            {
                                                co = '+';
                                                break;
                                            }
                                        case UpgradeType.MaxHp:
                                            {
                                                co = 'Ú';
                                                break;
                                            }
                                        case UpgradeType.Speed:
                                            {
                                                co = 'š';
                                                break;
                                            }
                                    }
                                }
                            }
                            else if (intersectObject is TankObject tankObject)
                            {
                                if (co == ' ')
                                {
                                    Console.BackgroundColor = ConsoleColor.DarkMagenta;
                                    switch (tankObject.Direction)
                                    {
                                        case DirectionType.Left:
                                            co = '<';
                                            break;
                                        case DirectionType.Right:
                                            co = '>';
                                            break;
                                        case DirectionType.Up:
                                            co = '^';
                                            break;
                                        case DirectionType.Down:
                                            co = 'v';
                                            break;
                                    }
                                }
                            }
                            else if (intersectObject is BulletObject bulletObject)
                            {
                                if (bulletObject.Direction == DirectionType.Left || bulletObject.Direction == DirectionType.Right)
                                {
                                    co = '—';
                                }
                                else
                                {
                                    co = '|';
                                }
                            }
                            else
                            {
                                co = '?';
                            }
                        }

                        Console.Write(co);
                        Console.BackgroundColor = ConsoleColor.Black;
                    }

                    Console.WriteLine();
                }

                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.CursorLeft = 0;
                Console.WriteLine($"Обновление карты от {_lastMapUpdate}");

                Console.WriteLine();
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.CursorLeft = 0;
                Console.WriteLine($"Сообщений в очереди {_msgCount}");

                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.CursorLeft = 0;
                Console.WriteLine($"Bots: {map.InteractObjects.OfType<TankObject>().Count()}; " +
                                  $"Spectators: {map.InteractObjects.OfType<SpectatorObject>().Count()}; " +
                                  $"Bullets: {map.InteractObjects.OfType<BulletObject>().Count()}; " + $"" +
                                  $"D.w.: {dw}");

                //Console.WriteLine();
                //foreach (var interactObject in map.InteractObjects)
                //{
                //    //Console.Write($"{interactObject.Id}; {interactObject.Rectangle.LeftCorner} ");

                //    if (interactObject is TankObject tankObject)
                //    {
                //        Console.Write($"T {tankObject.Tag} : ({tankObject.Score}) {tankObject.Hp} / {tankObject.MaximumHp}");
                //    }
                //    else if (interactObject is BulletObject bulletObject)
                //    {
                //        Console.Write($"B {bulletObject.DamageHp}");
                //    }
                //    else
                //    {
                //        Console.Write("?");
                //    }

                //    Console.WriteLine(new string(' ', Console.WindowWidth));
                //}

                //Console.WriteLine(new string(' ', Console.WindowWidth));
                //Console.WriteLine(new string(' ', Console.WindowWidth));
                //Console.WriteLine(new string(' ', Console.WindowWidth));
            }
        }

        public ServerResponse Client(int msgCount, ServerRequest request)
        {
            lock (_syncObject)
            {
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
    }
}
