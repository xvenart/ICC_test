using System;
using TankCommon;
using TankCommon.Enum;
using TankCommon.Objects;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.IO;

namespace TankClient
{
    public class TestClient : IClientBot
    {
        public Map _map;

        public int[,] LocationMap;
        public int X;
        public int Y;

        public Vector3 CurrentPosition;
        public Vector3 LastPosition;

        public BaseInteractObject ClosestEnemy;
        public IEnumerable<BaseInteractObject> AllEnemies;

        public bool endBattle = false;

        public void Start(ServerRequest request)
        {
            //определение нынешнего положения танка
            CurrentPosition = new Vector3(request.Tank.Rectangle.LeftCorner.LeftInt, request.Tank.Rectangle.LeftCorner.TopInt, 1);

            //определение финишной точки
            LastPosition = CurrentPosition;

            var Cells = request.Map.Cells;
            X = Cells.GetLength(0);
            Y = Cells.GetLength(1);
            LocationMap = new int[X, Y];

            for (var i = 0; i < X; i++)
            {
                for (var j = 0; j < Y; j++)
                {
                    if (Cells[i, j] == CellMapType.Wall /*|| Cells[i, j] == CellMapType.Water*/ || Cells[i, j] == CellMapType.DestructiveWall)
                    {
                        LocationMap[i, j] = -1;
                    }
                    else
                    {
                        LocationMap[i, j] = 0;
                    }
                }
            }
        }

        public ServerResponse Client(int msgCount, ServerRequest request)
        {
            if (request.Map.Cells != null)
            {
                _map = request.Map;
            }
            else
            {
                return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
            }

            Start(request);

            Move(request);

            FindStep();

            if (isInLine())
            {
                //return new ServerResponse { ClientCommand = SwitchDirection(ClosestEnemy.Rectangle.LeftCorner.LeftInt, ClosestEnemy.Rectangle.LeftCorner.TopInt) };
                return new ServerResponse { ClientCommand = ClientCommandType.Fire };
            }
            /*else
            {
                return new ServerResponse { ClientCommand = ClientCommandType.Fire };
            }*/

            return new ServerResponse { ClientCommand = ClientCommandType.Go };
        }

        public ClientCommandType SwitchDirection(int _X, int _Y)
        {
            if (CurrentPosition.X > _X)
            {
                return ClientCommandType.TurnLeft;
            }
            else if (CurrentPosition.X < _X)
            {
                return ClientCommandType.TurnRight;
            }
            else if (CurrentPosition.Y > _Y)
            {
                return ClientCommandType.TurnDown;
            }
            else if (CurrentPosition.Y < _Y)
            {
                return ClientCommandType.TurnUp;
            }
            else
            {
                return ClientCommandType.Stop;
            }
        }

        public bool isInLine()
        {
            return (CurrentPosition.X == ClosestEnemy.Rectangle.LeftCorner.LeftInt || CurrentPosition.Y == ClosestEnemy.Rectangle.LeftCorner.TopInt) ? true : false;
        }

        public ClientCommandType Move(ServerRequest request)
        {
            //получаем все объекты на карте
            var AllObjectsOnMap = request.Map.InteractObjects;
            //помещаем в переменную только танки
            AllEnemies = AllObjectsOnMap.Where(p => p is TankObject);

            //если вражеских танков на карте нет, прекращаем игру/рассчёт ???
            if (AllEnemies.Count() == 0)
            {
                endBattle = true;
            }

            //если игра не окончена
            if (!endBattle)
            {
                //находим ближайшего врага
                ClosestEnemy = FindClosestEnemy();

                //находим кратчайший путь
                var ShortWay = FindShortestWay();

                WriteFiles(ShortWay);

                //текущую позицию танка помещаем единицей
                CurrentPosition = new Vector3(request.Tank.Rectangle.LeftCorner.LeftInt, request.Tank.Rectangle.LeftCorner.TopInt, 0);
                LocationMap[(int)CurrentPosition.X, (int)CurrentPosition.Y] = 1;

                if (CurrentPosition != LastPosition)
                {
                    return SwitchDirection((ClosestEnemy as TankObject).Rectangle.LeftCorner.LeftInt, (ClosestEnemy as TankObject).Rectangle.LeftCorner.TopInt);
                }

                return ClientCommandType.Go;
            }
            
            return ClientCommandType.None;
        }

        public ClientCommandType FindStep()
        {
            //соседние с текущим местоположением ячейки
            var Neightbors = new List<Vector3>();

            var goTo = new Vector3(ClosestEnemy.Rectangle.LeftCorner.LeftInt, ClosestEnemy.Rectangle.LeftCorner.TopInt, 1);

            for (var i = (int)CurrentPosition.X - 1; i <= (int)CurrentPosition.X + 1; i++)
            {
                for (var j = (int)CurrentPosition.Y + 1; j >= (int)CurrentPosition.Y - 1; j--)
                {
                    /*if (i >= 0 && j >= 0 && i <= (int)CurrentPosition.X && j <= (int)CurrentPosition.Y)
                    {*/
                        Neightbors.Add(new Vector3(i, j, Vector3.Distance(new Vector3(i, j, 0), goTo)));
                    //}
                }
            }

            //сортируем список, чтобы первым элементом была ячейка с минимальным весом, туда и будем двигаться
            var closest = Neightbors.OrderBy(x => x.Z).ToList();

            LocationMap[(int)closest[0].X, (int)closest[0].Y] = 2;
            SwitchDirection((int)closest[0].X, (int)closest[0].Y);

            return ClientCommandType.Go;
        }

        public void WriteFiles(int[,] ShortWay)
        {
            var docPath = Environment.CurrentDirectory;
            var sw = new StreamWriter(Path.Combine(docPath, "Short.txt"));
            for (var i = 0; i < X; i++)
            {
                for (var j = 0; j < Y; j++)
                {
                    sw.Write(ShortWay[i, j] + "\t");
                }
                sw.WriteLine();
            }
            sw.Close();

            sw = new StreamWriter(Path.Combine(docPath, "Location.txt"));
            for (var i = 0; i < X; i++)
            {
                for (var j = 0; j < Y; j++)
                {
                    sw.Write(LocationMap[i, j] + "\t");
                }
                sw.WriteLine();
            }
            sw.Close();
        }

        public BaseInteractObject FindClosestEnemy()
        {
            //определяем словарь, содержащий танк и его расстояние от бота до него
            var AllDistance = new Dictionary<BaseInteractObject, double>();
            //заполняем словарь
            foreach (var i in AllEnemies)
            {
                if (i is TankObject)
                {
                    AllDistance.Add(i, Math.Pow((int)CurrentPosition.X - i.Rectangle.LeftCorner.TopInt, 2) + Math.Pow((int)CurrentPosition.Y - i.Rectangle.LeftCorner.LeftInt, 2));
                }
            }

            return AllDistance.OrderBy(x => x.Value).First().Key;
        }

        public int[,] FindShortestWay()
        {
            bool add = true;
            var MarkedMap = new int[X, Y];

            var Step = 0;

            for (var i = 0; i < X; i++)
            {
                for (var j = 0; j < Y; j++)
                {
                    if (LocationMap[i, j] == -1)
                    {
                        //стена
                        MarkedMap[i, j] = -2;
                    }
                    else if (LocationMap[i, j] == 0)
                    {
                        //ещё не были здесь
                        MarkedMap[i, j] = -1;
                    }
                }
            }

            //помечаем координаты цели на карте
            MarkedMap[ClosestEnemy.Rectangle.LeftCorner.TopInt, ClosestEnemy.Rectangle.LeftCorner.LeftInt] = 0;
            while (add == true)
            {
                for (var i = 1; i < X-1; i++)
                {
                    for (var j = 1; j < Y-1; j++)
                    {
                        //если нынешняя позиция имеет "флаг" Step
                        if (MarkedMap[i, j] == Step)
                        {
                            //определяем левый путь танка: является ли левая ячейка целевой, является ли левая ячейка непроходимым препятствием, находились ли мы в левой ячейке прежде
                            if (i - 1 >= 0 && MarkedMap[i - 1, j] != -2 && MarkedMap[i - 1, j] == -1)
                            {
                                MarkedMap[i - 1, j] = Step + 1;
                            }

                            //справа
                            if (i + 1 >= 0 && MarkedMap[i + 1, j] != -2 && MarkedMap[i + 1, j] == -1)
                            {
                                MarkedMap[i + 1, j] = Step + 1;
                            }

                            //внизу
                            if (j - 1 >= 0 && MarkedMap[i, j - 1] != -2 && MarkedMap[i, j - 1] == -1)
                            {
                                MarkedMap[i, j - 1] = Step + 1;
                            }

                            //сверху
                            if (j + 1 >= 0 && MarkedMap[i, j + 1] != -2 && MarkedMap[i, j + 1] == -1)
                            {
                                MarkedMap[i, j + 1] = Step + 1;
                            }
                        }
                    }
                }

                Step++;

                //если текущее местоположение отмечено как "непустое"
                if (MarkedMap[(int)CurrentPosition.X, (int)CurrentPosition.Y] > 0)
                {
                    //решение найдено
                    add = false;
                }

                //если шагов больше, чем размер карты
                if (Step > X * Y)
                {
                    //решение не найдено
                    add = false;
                }
            }

            return MarkedMap;
        }

        public bool StopMove(ServerRequest request)
        {
            bool move = false;
            for (var i = (int)CurrentPosition.X - 1; i <= (int)CurrentPosition.X + 1; i++)
            {
                for (var j = (int)CurrentPosition.Y + 1; j >= (int)CurrentPosition.Y - 1; j--)
                {
                    if (i == ClosestEnemy.Rectangle.LeftCorner.LeftInt && j == ClosestEnemy.Rectangle.LeftCorner.TopInt)
                    {
                        move = true;
                    }
                }
            }

            return move;
        }
    }
}
