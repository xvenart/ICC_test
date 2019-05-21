using System;
using System.Linq;
using System.IO;
using TankCommon;
using TankCommon.Enum;
using TankCommon.Objects;
using System.Collections.Generic;
using System.Numerics;

namespace TrukhinaClient
{
    public class TrukhinaClient : IClientBot
    {
        public ServerRequest Request;

        public int[,] LocationMap;
        public int[,] ShortWay;

        public Vector2 CurrentPosition;
        public Vector2 LastPosition;

        public BaseInteractObject ClosestEnemy;
        public List<BaseInteractObject> AllEnemies;

        public bool ready;

        public void Start(ServerRequest request)
        {
            Request = new ServerRequest();

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

            AllEnemies = new List<BaseInteractObject>();

            ClosestEnemy = new BaseInteractObject();
        }

        public ServerResponse Client(int msgCount, ServerRequest request)
        {
            ready = false;

            if (request.Map.Cells != null)
            {
                Start(request);
            }

            if (request.Map == null)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
            }

            Update(request);

            if (!AllEnemies.Any() || AllEnemies.Count == 0)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
            }
            else
            {
                ready = true;
            }

            if (ready)
            {
                if (ClosestEnemy is TankObject && (isInLineX() || isInLineY()))
                {
                    return new ServerResponse { ClientCommand = ClientCommandType.Fire };
                }

                if (ClosestEnemy != null)
                {
                    return new ServerResponse { ClientCommand = GoToTarget() };
                }
            }

            return new ServerResponse { ClientCommand = ClientCommandType.Stop };
        }

        public bool isInLineX()
        {
            return ((Request.Tank.Rectangle.LeftCorner.LeftInt - ClosestEnemy.Rectangle.LeftCorner.LeftInt < 4) && 
                (Request.Tank.Rectangle.LeftCorner.TopInt < ClosestEnemy.Rectangle.LeftCorner.TopInt && Request.Tank.Direction == DirectionType.Down) ||
                (Request.Tank.Rectangle.LeftCorner.TopInt > ClosestEnemy.Rectangle.LeftCorner.TopInt && Request.Tank.Direction == DirectionType.Up)) ? true : false;
        }

        public bool isInLineY()
        {
            return ((Request.Tank.Rectangle.LeftCorner.TopInt - ClosestEnemy.Rectangle.LeftCorner.TopInt < 4) &&
                (Request.Tank.Rectangle.LeftCorner.LeftInt < ClosestEnemy.Rectangle.LeftCorner.LeftInt && Request.Tank.Direction == DirectionType.Right) ||
                (Request.Tank.Rectangle.LeftCorner.LeftInt > ClosestEnemy.Rectangle.LeftCorner.LeftInt && Request.Tank.Direction == DirectionType.Left)) ? true : false;
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

        public ClientCommandType GoToTarget()
        {
            var Step = new Vector2(0, 0);
            var Tank = Request.Tank.Rectangle.LeftCorner;
            for (var i = Tank.LeftInt - 1; i <= Tank.LeftInt + 1; i++)
            {
                for (var j = Tank.TopInt + 1; j >= Tank.TopInt - 1; j--)
                {
                    if (LocationMap[i, j] == 2)
                    {
                        Step = new Vector2(i, j);
                        break;
                    }
                }
            }

            if (Step.X == Tank.LeftInt + 1 && Step.Y == Tank.TopInt)
            {
                if (Request.Tank.Direction != DirectionType.Right && LocationMap[(int)Step.X, (int)Step.Y] != -1)
                {
                    return ClientCommandType.TurnRight;
                }
                else
                {
                    return ClientCommandType.Go;
                }
            }
            else if (Step.X == Tank.LeftInt - 1 && Step.Y == Tank.TopInt && LocationMap[(int)Step.X, (int)Step.Y] != -1)
            {
                if (Request.Tank.Direction != DirectionType.Left)
                {
                    return ClientCommandType.TurnLeft;
                }
                else
                {
                    return ClientCommandType.Go;
                }
            }
            else if (Step.X == Tank.LeftInt && Step.Y == Tank.TopInt + 1 && LocationMap[(int)Step.X, (int)Step.Y] != -1)
            {
                if (Request.Tank.Direction != DirectionType.Down)
                {
                    return ClientCommandType.TurnDown;
                }
                else
                {
                    return ClientCommandType.Go;
                }
            }
            else if (Step.X == Tank.LeftInt && Step.Y == Tank.TopInt - 1 && LocationMap[(int)Step.X, (int)Step.Y] != -1)
            {
                if (Request.Tank.Direction != DirectionType.Up)
                {
                    return ClientCommandType.TurnUp;
                }
                else
                {
                    return ClientCommandType.Go;
                }
            }

            CurrentPosition = new Vector2(Tank.LeftInt, Tank.TopInt);
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
            Request = request;

            ready = false;

            //получаем все объекты на карте
            var AllObjectsOnMap = Request.Map.InteractObjects;
            //помещаем в переменную только танки
            if (AllEnemies.Count > 0)
            {
                AllEnemies.Clear();
            }

            AllEnemies.AddRange(Request.Map.InteractObjects.Where(x => x.Id != Request.Tank.Id));

            if (AllEnemies.Any())
            {
                ready = true;
            }

            //если игра не окончена
            if (ready)
            {
                //находим ближайшего врага
                ClosestEnemy = FindClosestEnemy();

                if (ClosestEnemy != null)
                {
                    //находим кратчайший путь
                    ShortWay = FindShortestWay();

                    //текущую позицию танка помечаем единицей
                    //LocationMap[Request.Tank.Rectangle.LeftCorner.LeftInt, Request.Tank.Rectangle.LeftCorner.TopInt] = 1;

                    FindStep();

                    WriteFiles();
                }
            }
        }

        public void FindStep()
        {
            //соседние с текущим местоположением ячейки
            var Neightbors = new Dictionary<Vector2, double>();

            var goTo = new Vector2(ClosestEnemy.Rectangle.LeftCorner.LeftInt, ClosestEnemy.Rectangle.LeftCorner.TopInt);

            var Tank = Request.Tank.Rectangle.LeftCorner;

            if (LocationMap[Tank.LeftInt, Tank.TopInt - 1] != -1)
            {
                Neightbors.Add(new Vector2(Tank.LeftInt, Tank.TopInt - 1), GetDistanse(Tank.LeftInt, Tank.TopInt - 1, goTo.X, goTo.Y));
            }
            if (LocationMap[Tank.LeftInt, Tank.TopInt + 1] != -1)
            {
                Neightbors.Add(new Vector2(Tank.LeftInt, Tank.TopInt + 1), GetDistanse(Tank.LeftInt, Tank.TopInt + 1, goTo.X, goTo.Y));
            }
            if (LocationMap[Tank.LeftInt + 1, Tank.TopInt] != -1)
            {
                Neightbors.Add(new Vector2(Tank.LeftInt + 1, Tank.TopInt), GetDistanse(Tank.LeftInt + 1, Tank.TopInt, goTo.X, goTo.Y));
            }
            if (LocationMap[Tank.LeftInt - 1, Tank.TopInt] != -1)
            {
                Neightbors.Add(new Vector2(Tank.LeftInt - 1, Tank.TopInt), GetDistanse(Tank.LeftInt - 1, Tank.TopInt, goTo.X, goTo.Y));
            }

            //сортируем список, чтобы первым элементом была ячейка с минимальным весом, туда и будем двигаться
            var closest = Neightbors.OrderBy(x => x.Value).ToList();

            LocationMap[(int)closest[0].Key.X, (int)closest[0].Key.Y] = 2;
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

        public BaseInteractObject FindClosestEnemy()
        {
            var Tank = Request.Tank.Rectangle.LeftCorner;
            //определяем словарь, содержащий танк и его расстояние от бота до него
            var AllDistance = new Dictionary<BaseInteractObject, double>();
            AllDistance.Clear();
            //заполняем словарь
            if (AllEnemies == null || !AllEnemies.Any())
            {
                return null;
            }

            foreach (var i in AllEnemies)
            {
                AllDistance.Add(i, GetDistanse(Tank.LeftInt, Tank.TopInt, i.Rectangle.LeftCorner.LeftInt, i.Rectangle.LeftCorner.TopInt));
            }

            return AllDistance.OrderBy(x => x.Value).First().Key;
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
            while (add)
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

                            //сверху
                            if (j - 1 >= 0 && MarkedMap[i, j - 1] != -2 && MarkedMap[i, j - 1] == -1)
                            {
                                MarkedMap[i, j - 1] = Step + 1;
                            }

                            //снизу
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
                //if (MarkedMap[request.Tank.Rectangle.LeftCorner.LeftInt, request.Tank.Rectangle.LeftCorner.TopInt] > 0)
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
