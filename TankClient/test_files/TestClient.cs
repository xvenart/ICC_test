using System;
using System.Linq;
using System.IO;
using TankCommon;
using TankCommon.Enum;
using TankCommon.Objects;
using System.Collections.Generic;
using System.Numerics;

namespace TankClient
{
    public class TestClient : IClientBot
    {
        protected Rectangle rectangle;
        protected Map _map;

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
                    if (request.Map.Cells[i, j] == CellMapType.Wall || request.Map.Cells[i, j] == CellMapType.DestructiveWall || request.Map.Cells[i, j] == CellMapType.Water)
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
            //AllEnemies = request.Map.InteractObjects.Where(x => x is TankObject || x is UpgradeInteractObject);
            //AllEnemies = request.Map.InteractObjects;

            endBattle = false;
        }

        public ServerResponse Client(int msgCount, ServerRequest request)
        {
            ready = false;

            Start(request);
            
            if (request.Map == null)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
            }

            if (AllEnemies == null && !AllEnemies.Any())
            {
                return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
            }
            else
            {
                ready = true;
            }

            if (ready)
            {
                Update(request);

                if (ClosestEnemy == null)
                {
                    return new ServerResponse { ClientCommand = Walk() };
                }

                if (GetDistanse(request.Tank.Rectangle.LeftCorner.LeftInt, request.Tank.Rectangle.LeftCorner.TopInt, ClosestEnemy.Rectangle.LeftCorner.LeftInt, ClosestEnemy.Rectangle.LeftCorner.TopInt) < 10)
                {
                    return new ServerResponse { ClientCommand = ClientCommandType.Fire };
                }

                if (ClosestEnemy != null)
                {
                    return new ServerResponse { ClientCommand = GoToTarget(request) };
                }
            }

            return new ServerResponse { ClientCommand = ClientCommandType.Stop };
        }

        public bool isInLineX(ServerRequest request)
        {
            return (request.Tank.Rectangle.LeftCorner.LeftInt == ClosestEnemy.Rectangle.LeftCorner.LeftInt) ? true : false;
        }

        public bool isInLineY(ServerRequest request)
        {
            return (request.Tank.Rectangle.LeftCorner.TopInt == ClosestEnemy.Rectangle.LeftCorner.TopInt) ? true : false;
        }

        public ClientCommandType Walk()
        {
            var Rand = new Random();
            switch (Rand.Next(0, 10))
            {
                case 0:
                    return ClientCommandType.TurnLeft;
                case 1:
                    return ClientCommandType.TurnUp;
                case 2:
                    return ClientCommandType.TurnRight;
                case 3:
                    return ClientCommandType.TurnDown;
                default:
                    return ClientCommandType.Go;
            }
        }

        public ClientCommandType GoToTarget(ServerRequest request)
        {
            var tX = 0;
            var tY = 0;
            var tank = request.Tank.Rectangle.LeftCorner;
            for (var i = tank.LeftInt - 1; i <= tank.LeftInt + 1; i++)
            {
                for (var j = tank.TopInt + 1; j >= tank.TopInt - 1; j--)
                {
                    if (LocationMap[i, j] == 2)
                    {
                        tX = i;
                        tY = j;
                        break;
                    }
                }
            }


            if (tank.LeftInt > tX && request.Tank.Direction != DirectionType.Left)
            {
                return ClientCommandType.TurnLeft;
            }
            /*else if (tank.LeftInt > tX && request.Tank.Direction == DirectionType.Left)
            {
                return ClientCommandType.Go;
            }*/
            else if (tank.TopInt > tY && request.Tank.Direction != DirectionType.Up)
            {
                return ClientCommandType.TurnUp;
            }
            /*else if (tank.TopInt > tY && request.Tank.Direction == DirectionType.Up)
            {
                return ClientCommandType.Go;
            }*/
            else if (tank.LeftInt < tX && request.Tank.Direction != DirectionType.Right)
            {
                return ClientCommandType.TurnRight;
            }
            /*else if (tank.LeftInt < tX && request.Tank.Direction == DirectionType.Right)
            {
                return ClientCommandType.Go;
            }*/
            else if (tank.TopInt < tY && request.Tank.Direction != DirectionType.Down)
            {
                return ClientCommandType.TurnDown;
            }
            /*else if (tank.TopInt < tY && request.Tank.Direction == DirectionType.Down)
            {
                return ClientCommandType.Go;
            }*/


            CurrentPosition = new Vector2(tank.LeftInt, tank.TopInt);
            LocationMap[(int)CurrentPosition.X, (int)CurrentPosition.Y] = 1;

            if (CurrentPosition != LastPosition)
            {
                LocationMap[(int)LastPosition.X, (int)LastPosition.Y] = 0;
                LastPosition = CurrentPosition;
            }

            return ClientCommandType.Go;
        }

        public void Update(ServerRequest request)
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
                ShortWay = FindShortestWay();

                //текущую позицию танка помечаем единицей
                LocationMap[request.Tank.Rectangle.LeftCorner.LeftInt, request.Tank.Rectangle.LeftCorner.TopInt] = 1;

                FindStep(request);

                WriteFiles();
            }
        }

        public void FindStep(ServerRequest request)
        {
            //соседние с текущим местоположением ячейки
            var Neightbors = new List<Vector3>();

            var goTo = new Vector2(ClosestEnemy.Rectangle.LeftCorner.LeftInt, ClosestEnemy.Rectangle.LeftCorner.TopInt);

            var Tank = request.Tank.Rectangle.LeftCorner;

            for (var i = Tank.LeftInt - 1; i <= Tank.LeftInt + 1; i++)
            {
                for (var j = Tank.TopInt + 1; j >= Tank.TopInt - 1; j--)
                {
                    if (!(i > Tank.LeftInt && j > Tank.TopInt) || !(i < Tank.LeftInt && j < Tank.TopInt))
                    {
                        if (ShortWay[i, j] != -2)
                        {
                            Neightbors.Add(new Vector3(i, j, (float)GetDistanse(i, j, goTo.X, goTo.Y)));
                        }
                    }
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
            var Tank = request.Tank.Rectangle.LeftCorner;
            //определяем словарь, содержащий танк и его расстояние от бота до него
            var AllDistance = new Dictionary<BaseInteractObject, double>();
            //заполняем словарь
            if (AllEnemies == null || !AllEnemies.Any())
            {
                return null;
            }
            foreach (var i in AllEnemies)
            {
                AllDistance.Add(i, GetDistanse(Tank.LeftInt, Tank.TopInt, i.Rectangle.LeftCorner.LeftInt, i.Rectangle.LeftCorner.TopInt));
            }

            return (AllDistance.Count > 1) ? AllDistance.OrderBy(x => x.Value).First().Key : null;
            //return AllDistance.OrderBy(x => x.Value).First().Key;
        }

        public int[,] FindShortestWay()
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
                    else
                    {
                        //ещё не были здесь
                        MarkedMap[i, j] = -1;
                    }
                }
            }

            //помечаем координаты цели на карте
            MarkedMap[ClosestEnemy.Rectangle.LeftCorner.LeftInt, ClosestEnemy.Rectangle.LeftCorner.TopInt] = 0;
            while (add == true)
            {
                for (var i = 0; i < MarkedMap.GetLength(0); i++)
                {
                    for (var j = 0; j < MarkedMap.GetLength(1); j++)
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
                            if (j + 1 >= 0 && MarkedMap[i, j + 1] != -2 && MarkedMap[i, j + 1] == -1)
                            {
                                MarkedMap[i, j - 1] = Step + 1;
                            }

                            //сверху
                            if (j - 1 >= 0 && MarkedMap[i, j - 1] != -2 && MarkedMap[i, j - 1] == -1)
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
                if (Step > MarkedMap.GetLength(0) * MarkedMap.GetLength(1))
                {
                    //решение не найдено
                    add = false;
                }
            }

            return MarkedMap;
        }

        public double GetDistanse(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }
    }
}
