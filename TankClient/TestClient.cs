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
        public int[,] LocationMap;
        public int[,] ShortWay;
        public int X, Y;

        public Vector2 CurrentPosition;
        public Vector2 LastPosition;

        public BaseInteractObject ClosestEnemy;
        public IEnumerable<BaseInteractObject> AllEnemies;

        public bool ready;
        public bool endBattle;

        public void Start(ServerRequest request)
        {
            //определение нынешнего положения танка
            CurrentPosition = new Vector2(request.Tank.Rectangle.LeftCorner.LeftInt, request.Tank.Rectangle.LeftCorner.TopInt);

            //определение финишной точки
            LastPosition = CurrentPosition;

            LocationMap = new int[request.Map.Cells.GetLength(0), request.Map.Cells.GetLength(1)];

            for (var i = 0; i < request.Map.Cells.GetLength(0); i++)
            {
                for (var j = 0; j < request.Map.Cells.GetLength(1); j++)
                {
                    if (request.Map.Cells[i, j] == CellMapType.Wall || request.Map.Cells[i, j] == CellMapType.DestructiveWall)
                    {
                        LocationMap[i, j] = -1;
                    }
                    else
                    {
                        LocationMap[i, j] = 0;
                    }
                }
            }

            AllEnemies = request.Map.InteractObjects.Where(x => (x is TankObject && x.Id != request.Tank.Id) || x is UpgradeInteractObject);

            endBattle = false;
        }

        public ServerResponse Client(int msgCount, ServerRequest request)
        {
            ready = false;

            if (request.Map == null)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
            }
            else if (request.Map.Cells != null)
            {
                Start(request);
            }

            if (AllEnemies == null || !AllEnemies.Any())
            {
                return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
            }
            else if (AllEnemies != null)
            {
                ready = true;
            }

            var docPath = Environment.CurrentDirectory;
            var sw = new StreamWriter(Path.Combine(docPath, "InteractObjects.txt"));
            foreach (var i in request.Map.InteractObjects)
            {
                sw.WriteLine($"{i.Rectangle.LeftCorner.LeftInt}, {i.Rectangle.LeftCorner.TopInt}");
            }
            sw.Close();

            if (ready)
            {

                if (request.Map.Cells != null)
                {
                    return new ServerResponse { ClientCommand = Move(request) };
                }
                else if (isInLineX() || isInLineY())
                {
                    return new ServerResponse { ClientCommand = ClientCommandType.Stop };
                }
                else if (!isNeedMove())
                {
                    return new ServerResponse { ClientCommand = ClientCommandType.Fire };
                }
            }

            return new ServerResponse { ClientCommand = ClientCommandType.None };
        }

        public bool isNeedMove()
        {
            if (Math.Abs(CurrentPosition.X - ClosestEnemy.Rectangle.LeftCorner.LeftInt) > 5 || Math.Abs(CurrentPosition.Y - ClosestEnemy.Rectangle.LeftCorner.TopInt) > 5)
            {
                return true;
            }
            return false;
        }

        public ClientCommandType SwitchDirection()
        {
            if ((int)CurrentPosition.X > ClosestEnemy.Rectangle.LeftCorner.LeftInt || LocationMap[(int)CurrentPosition.X + 1, (int)CurrentPosition.Y] == -1)
            {
                return ClientCommandType.TurnLeft;
            }
            else if ((int)CurrentPosition.X < ClosestEnemy.Rectangle.LeftCorner.LeftInt || LocationMap[(int)CurrentPosition.X - 1, (int)CurrentPosition.Y] == -1)
            {
                return ClientCommandType.TurnRight;
            }
            else if ((int)CurrentPosition.Y > ClosestEnemy.Rectangle.LeftCorner.TopInt || LocationMap[(int)CurrentPosition.X, (int)CurrentPosition.Y + 1] == -1)
            {
                return ClientCommandType.TurnDown;
            }
            else if ((int)CurrentPosition.Y < ClosestEnemy.Rectangle.LeftCorner.TopInt || LocationMap[(int)CurrentPosition.X, (int)CurrentPosition.Y - 1] == -1)
            {
                return ClientCommandType.TurnUp;
            }
            else
            {
                return ClientCommandType.Go;
            }
        }

        public bool isInLineX()
        {
            return (CurrentPosition.X == ClosestEnemy.Rectangle.LeftCorner.LeftInt) ? true : false;
        }

        public bool isInLineY()
        {
            return (CurrentPosition.Y == ClosestEnemy.Rectangle.LeftCorner.TopInt) ? true : false;
        }

        public ClientCommandType GoToTarget(ServerRequest request)
        {
            for (var i = (int)CurrentPosition.X - 1; i < (int)CurrentPosition.X + 1; i++)
            {
                for (var j = (int)CurrentPosition.Y + 1; j > (int)CurrentPosition.Y - 1; j--)
                {
                    if (LocationMap[i,j] == 2)
                    {
                        if (request.Tank.IsMoving)
                        {
                            return ClientCommandType.Stop;
                        }
                        //return SwitchDirection();
                    }
                    if (LocationMap[i,j] == -1)
                    {
                        return SwitchDirection();
                    }
                }
            }
            //return SwitchDirection();
            return ClientCommandType.Go;
        }

        public ClientCommandType Move(ServerRequest request)
        {
            //получаем все объекты на карте
            var AllObjectsOnMap = request.Map.InteractObjects;
            //помещаем в переменную только танки
            AllEnemies = AllObjectsOnMap.Where(x => (x is TankObject && x.Id != request.Tank.Id) || x is UpgradeInteractObject);

            //если вражеских танков на карте нет, прекращаем игру 
            if (AllEnemies == null)
            {
                endBattle = true;
            }

            //если игра не окончена
            if (!endBattle)
            {
                //находим ближайшего врага
                ClosestEnemy = FindClosestEnemy(request);

                //находим кратчайший путь
                ShortWay = FindShortestWay(request);

                //текущую позицию танка помечаем единицей
                LocationMap[(int)CurrentPosition.X, (int)CurrentPosition.Y] = 1;

                FindStep();

                WriteFiles();

                for (var i = (int)CurrentPosition.X - 1; i <= (int)CurrentPosition.X + 1; i++)
                {
                    for (var j = (int)CurrentPosition.Y + 1; j >= (int)CurrentPosition.Y - 1; j--)
                    {
                        if (LocationMap[i, j] == 2)
                        {
                            if ((int)CurrentPosition.X != i && (int)CurrentPosition.Y != j)
                            {
                                return SwitchDirection();
                            }
                        }
                    }
                }

                LocationMap[(int)CurrentPosition.X, (int)CurrentPosition.Y] = 0;
                CurrentPosition = new Vector2(request.Tank.Rectangle.LeftCorner.LeftInt, request.Tank.Rectangle.LeftCorner.TopInt);
                LastPosition = CurrentPosition;
                LocationMap[(int)CurrentPosition.X, (int)CurrentPosition.Y] = 1;
            }
            return ClientCommandType.Stop;
        }

        public void FindStep()
        {
            //соседние с текущим местоположением ячейки
            var Neightbors = new List<Vector3>();

            var goTo = new Vector2(ClosestEnemy.Rectangle.LeftCorner.LeftInt, ClosestEnemy.Rectangle.LeftCorner.TopInt);

            for (var i = (int)CurrentPosition.X - 1; i <= (int)CurrentPosition.X + 1; i++)
            {
                for (var j = (int)CurrentPosition.Y + 1; j >= (int)CurrentPosition.Y - 1; j--)
                {
                    Neightbors.Add(new Vector3(i, j, Vector2.Distance(new Vector2(i, j), goTo)));
                }
            }

            //сортируем список, чтобы первым элементом была ячейка с минимальным весом, туда и будем двигаться
            var closest = Neightbors.OrderBy(x => x.Z).ToList();

            LocationMap[(int)closest[0].X, (int)closest[0].Y] = 2;
        }

        public void WriteFiles()
        {
            var docPath = Environment.CurrentDirectory;
            var sw = new StreamWriter(Path.Combine(docPath, "Short.txt"));
            for (var i = 0; i < ShortWay.GetLength(0); i++)
            {
                for (var j = 0; j < ShortWay.GetLength(1); j++)
                {
                    sw.Write(ShortWay[i, j] + "\t");
                }
                sw.WriteLine();
            }
            sw.Close();

            sw = new StreamWriter(Path.Combine(docPath, "Location.txt"));
            for (var i = 0; i < LocationMap.GetLength(0); i++)
            {
                for (var j = 0; j < LocationMap.GetLength(1); j++)
                {
                    sw.Write(LocationMap[i, j] + "\t");
                }
                sw.WriteLine();
            }
            sw.Close();

            sw = new StreamWriter(Path.Combine(docPath, "AllEnemies.txt"));
            foreach (var i in AllEnemies)
            {
                sw.WriteLine($"{i.Rectangle.LeftCorner.LeftInt}, {i.Rectangle.LeftCorner.TopInt}");
            }
            sw.Close();
        }

        public BaseInteractObject FindClosestEnemy(ServerRequest request)
        {
            //определяем словарь, содержащий танк и его расстояние от бота до него
            var AllDistance = new Dictionary<BaseInteractObject, double>();
            //заполняем словарь
            foreach (var i in AllEnemies)
            {
                AllDistance.Add(i, Math.Pow((int)CurrentPosition.X - i.Rectangle.LeftCorner.LeftInt, 2) + Math.Pow((int)CurrentPosition.Y - i.Rectangle.LeftCorner.TopInt, 2));
            }

            return AllDistance.OrderBy(x => x.Value).First().Key;
        }

        public int[,] FindShortestWay(ServerRequest request)
        {
            var add = true;
            var MarkedMap = new int[LocationMap.GetLength(0), LocationMap.GetLength(1)];

            var Step = 0;

            for (var i = 0; i < MarkedMap.GetLength(0); i++)
            {
                for (var j = 0; j < MarkedMap.GetLength(1); j++)
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
            MarkedMap[(int)CurrentPosition.X, (int)CurrentPosition.Y] = 0;
            while (add == true)
            {
                for (var i = 1; i < MarkedMap.GetLength(0) - 1; i++)
                {
                    for (var j = 1; j < MarkedMap.GetLength(1) - 1; j++)
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
                if (MarkedMap[(int)CurrentPosition.X, (int)CurrentPosition.Y] == -2)
                {
                    //решение найдено
                    add = false;
                }

                //если шагов больше, чем размер карты
                if (Step > MarkedMap.GetLength(0) * MarkedMap.GetLength(1))
                {
                    //решение не найдено
                    add = false;
                }
            }

            return MarkedMap;
        }

    }
}
