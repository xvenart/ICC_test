using System;
using TankCommon;
using TankCommon.Enum;
using TankCommon.Objects;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;
using System;

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
            CurrentPosition = new Vector3(request.Tank.Rectangle.LeftCorner.TopInt, request.Tank.Rectangle.LeftCorner.LeftInt, 0);

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
                    if (Cells[i, j] == CellMapType.Wall || Cells[i, j] == CellMapType.Water)
                    {
                        LocationMap[i, j] = 1;
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

            if (request.Map.InteractObjects == null)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
            }

            Start(request);

            if (!request.Tank.IsMoving)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.Go };
            }

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
            }

            //текущую позицию танка помещаем единицей
            CurrentPosition = new Vector3(request.Tank.Rectangle.LeftCorner.TopInt - 2, request.Tank.Rectangle.LeftCorner.LeftInt - 2, 0);
            LocationMap[(int)CurrentPosition.X, (int)CurrentPosition.Y] = 1;

            //если танк переместился с LastPosition, то обновляем карту: указываем 0 на месте, где был отмечен танк
            if (CurrentPosition != LastPosition)
            {
                LocationMap[(int)LastPosition.X, (int)LastPosition.Y] = 0;
            }

            if (CurrentPosition.X != ClosestEnemy.Rectangle.LeftCorner.LeftInt - 2 && CurrentPosition.Y != ClosestEnemy.Rectangle.LeftCorner.TopInt - 2)
            {
                return new ServerResponse { ClientCommand = SwitchDirection() };
            }

            //устанавливаем новое положение LastPosition
            LastPosition = CurrentPosition;



            //if (Update(request))
            //{
            //    return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
            //}

            var tank = request.Tank;
            if (null == tank)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.None };
            }

            /*if (!tank.IsMoving)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.Go };
            }*/

            if (tank.IsMoving)
            {
                return new ServerResponse { ClientCommand = SwitchDirection() };
            }

            if (endBattle)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.Stop };
            }

            return new ServerResponse { ClientCommand = ClientCommandType.Fire };
        }

        public ClientCommandType SwitchDirection()
        {
            var DiffX = ClosestEnemy.Rectangle.LeftCorner.LeftInt - CurrentPosition.X - 2;
            var DiffY = ClosestEnemy.Rectangle.LeftCorner.TopInt - CurrentPosition.Y - 2;
            if (DiffX > 0)
            {
                return ClientCommandType.TurnRight;
            }
            else if (DiffX < 0)
            {
                return ClientCommandType.TurnLeft;
            }
            else if (DiffY > 0)
            {
                return ClientCommandType.TurnUp;
            }
            else
            {
                return ClientCommandType.TurnDown;
            }
        }

        public bool Update(ServerRequest request)
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

            int[,] ShortWay;

            //если игра не окончена
            if (!endBattle)
            {
                //находим ближайшего врага
                ClosestEnemy = FindClosestEnemy();

                //находим кратчайший путь
                ShortWay = FindShortestWay();


                //текущую позицию танка помещаем единицей
                CurrentPosition = new Vector3(request.Tank.Rectangle.LeftCorner.TopInt - 2, request.Tank.Rectangle.LeftCorner.LeftInt - 2, 0);
                ShortWay[(int)CurrentPosition.X, (int)CurrentPosition.Y] = 1;

                //если танк переместился с LastPosition, то обновляем карту: указываем 0 на месте, где был отмечен танк
                if (CurrentPosition != LastPosition)
                {
                    ShortWay[(int)LastPosition.X, (int)LastPosition.Y] = 0;
                }

                //устанавливаем новое положение LastPosition
                LastPosition = CurrentPosition;
            }

                if (CurrentPosition == LastPosition)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            
        }

        public BaseInteractObject FindClosestEnemy()
        {
            //определяем словарь, содержащий танк и его расстояние от бота до него
            var AllDistance = new Dictionary<BaseInteractObject, double>();
            //заполняем словарь
            foreach (var i in AllEnemies)
            {
                AllDistance.Add(i, Math.Pow((int)CurrentPosition.X - i.Rectangle.LeftCorner.TopInt - 2, 2) + Math.Pow((int)CurrentPosition.Y - i.Rectangle.LeftCorner.LeftInt - 2, 2));
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
                    else
                    {
                        //ещё не были здесь
                        MarkedMap[i, j] = -1;
                    }
                }
            }

            //помечаем координаты цели на карте
            MarkedMap[ClosestEnemy.Rectangle.LeftCorner.TopInt - 2, ClosestEnemy.Rectangle.LeftCorner.LeftInt - 2] = 0;
            while (add == true)
            {
                add = false;

                for (var i = 1; i < X - 1; i++)
                {
                    for (var j = 1; j < Y - 1; j++)
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

                add = true;

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

        public bool Move(ServerRequest request)
        {
            //соседние с текущим местоположением ячейки
            var Neightbors = new List<Vector3>();

            var goTo = new Vector3(ClosestEnemy.Rectangle.LeftCorner.TopInt - 2, ClosestEnemy.Rectangle.LeftCorner.LeftInt - 2, 1);

            for (var i = (int)CurrentPosition.X - 1; i <= (int)CurrentPosition.X + 1; i++)
            {
                for (var j = (int)CurrentPosition.Y + 1; j <= (int)CurrentPosition.Y - 1; j--)
                {
                    Neightbors.Add(new Vector3(i, j, Vector3.Distance(new Vector3(i, j, 0), goTo)));
                }
            }

            //сортируем список, чтобы первым элементом была ячейка с минимальным весом, туда и будем двигаться
            var closest = Neightbors.Min();

            //перемещение танка
            var _rect = request.Tank.Rectangle;
            request.Tank.Rectangle = new Rectangle(new Point((decimal)Neightbors[0].X, (decimal)Neightbors[0].Y), _rect.Width, _rect.Height);
            CurrentPosition = new Vector3(closest.X, closest.Y, 0);

            return true;
        }

        public bool StopMove(int TargetX, int TargetY)
        {
            bool move = false;
            for (var i = (int)CurrentPosition.X - 1; i <= (int)CurrentPosition.X + 1; i++)
            {
                for (var j = (int)CurrentPosition.Y + 1; j >= (int)CurrentPosition.Y - 1; j--)
                {
                    if (i == TargetX && j == TargetY)
                    {
                        move = true;
                    }
                }
            }

            return move;
        }
    }
}
