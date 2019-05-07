using System;
using TankCommon;
using TankCommon.Enum;
using TankCommon.Objects;

namespace TankClient.TestBot
{
    public class TestClient : IClientBot
    {
        public Vector3 CurrentPosition;
        public Vector3 LastPosition;

        public bool ready = true;
        public bool readyAttack = true;
        public bool endBattle = false;

        public BaseInteractObject ClosestEnemy;
        public IEnumerable<BaseInteractObject> AllEnemies;

        public void Start(ServerRequest request)
        {
            //определение нынешнего положения танка
            CurrentPosition = new Vector3(request.Tank.Rectangle.LeftCorner.TopInt, request.Tank.Rectangle.LeftCorner.LeftInt, 0);

            //определение финишной точки
            LastPosition = CurrentPosition;
        }

        public ServerResponse Update(ServerRequest request)
        {
            //проверка на готовность к рассчёту
            if (BattleField.ready)
            {
                if (ready)
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
                        //определяем словарь, содержащий танк и его расстояние от бота
                        var AllDistance = new Dictionary<BaseInteractObject, double>();
                        //заполняем словарь
                        foreach (var i in AllEnemies)
                        {
                            AllDistance.Add(i, Math.Pow((int)CurrentPosition.X - i.Rectangle.LeftCorner.TopInt, 2) + Math.Pow((int)CurrentPosition.Y - i.Rectangle.LeftCorner.LeftInt, 2));
                        }

                        //находим минимальное расстояние, соответствующий танк - наша цель
                        ClosestEnemy = AllDistance.OrderBy(x => x.Value).First().Key;

                        //определяем координаты врага
                        var TargetX = ClosestEnemy.Rectangle.LeftCorner.TopInt;
                        var TargetY = ClosestEnemy.Rectangle.LeftCorner.LeftInt;

                        //находим кратчайший путь
                        var WaveMap = FindWave(request, TargetX, TargetY);

                        // ???
                        if (!StopMove(TargetX, TargetY))
                        {
                            Move(request, WaveMap);
                        }

                        if (readyAttack)
                        {
                            if (StopMove(TargetX, TargetY))
                            {
                                //по содержанию исходной функции, разрешаем атаку (исх.: добавляется задержка)
                                readyAttack = true;
                            }
                        }
                    }
                }

                //текущую позицию танка помещаем единицей
                CurrentPosition = new Vector3(request.Tank.Rectangle.LeftCorner.TopInt, request.Tank.Rectangle.LeftCorner.LeftInt, 0);
                BattleField.LocationMap[(int)CurrentPosition.X, (int)CurrentPosition.Y] = 1;

                //если танк переместился с LastPosition, то обновляем карту: указываем 0 на месте, где был отмечен танк
                if (CurrentPosition != LastPosition)
                {
                    BattleField.LocationMap[(int)LastPosition.X, (int)LastPosition.Y] = 0;
                }

                //устанавливаем новое положение LastPosition
                LastPosition = CurrentPosition;
                return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
            }
        }

        public int[,] FindWave(ServerRequest request, int targetX, int targetY)
        {
            bool add = true;
            var MarkedMap = new int[BattleField.X, BattleField.Y];

            var Step = 0;

            for (var i = 0; i < BattleField.X; i++)
            {
                for (var j = 0; j < BattleField.Y; j++)
                {
                    if (BattleField.LocationMap[i, j] == -1)
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
            MarkedMap[targetX, targetY] = 0;
            while (add == true)
            {
                add = false;

                for (var i = 0; i < BattleField.X; i++)
                {
                    for (var j = 0; j < BattleField.Y; j++)
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
                if (MarkedMap[request.Tank.Rectangle.LeftCorner.LeftInt, request.Tank.Rectangle.LeftCorner.TopInt] > 0)
                {
                    //решение найдено
                    add = false;
                }

                //если шагов больше, чем размер карты
                if (Step > BattleField.X * BattleField.Y)
                {
                    //решение не найдено
                    add = false;
                }
            }

            return MarkedMap;
        }

        public void Move(ServerRequest request, int[,] MarkedMap)
        {
            //готовность
            ready = false;

            //соседние с текущим местоположением ячейки
            var Neightbors = new List<int>();

            //переменная для последующего хранения ячейки, в которую перейдёт танк
            var MoveTo = new Vector3(-1, 0, 10);

            //добавляем окружающие ячейки
            Neightbors.Add(MarkedMap[(int)CurrentPosition.X, (int)CurrentPosition.Y + 1]);
            Neightbors.Add(MarkedMap[(int)CurrentPosition.X + 1, (int)CurrentPosition.Y + 1]);
            Neightbors.Add(MarkedMap[(int)CurrentPosition.X + 1, (int)CurrentPosition.Y]);
            Neightbors.Add(MarkedMap[(int)CurrentPosition.X + 1, (int)CurrentPosition.Y - 1]);
            Neightbors.Add(MarkedMap[(int)CurrentPosition.X, (int)CurrentPosition.Y - 1]);
            Neightbors.Add(MarkedMap[(int)CurrentPosition.X - 1, (int)CurrentPosition.Y - 1]);
            Neightbors.Add(MarkedMap[(int)CurrentPosition.X - 1, (int)CurrentPosition.Y - 1]);
            Neightbors.Add(MarkedMap[(int)CurrentPosition.X - 1, (int)CurrentPosition.Y + 1]);

            //проверяем все соседние ячейки на возможность движения 
            for (var i = 0; i < Neightbors.Count; i++)
            {
                if (Neightbors[i] == -2)
                {
                    Neightbors[i] = 9999;
                }
            }

            ////сортируем список, чтобы первым элементом была ячейка с минимальным весом, туда и будем двигаться
            //Neightbors.Sort();

            //поиск координат ячейки с минимальным весом
            for (var i = (int)CurrentPosition.X - 1; i <= (int)CurrentPosition.X + 1; i++)
            {
                for (var j = (int)CurrentPosition.Y + 1; j >= (int)CurrentPosition.Y - 1; j--)
                {
                    if (MarkedMap[i, j] == Neightbors.Min())
                    {
                        MoveTo = new Vector3(i, j, 10);
                    }
                }
            }

            //в случае, когда двигаться некуда и танк окружён, позиция не меняется
            if (MoveTo == new Vector3(-1, 0, 10))
            {
                MoveTo = new Vector3(CurrentPosition.X, CurrentPosition.Y, 10);
            }

            //перемещение танка
            request.Tank.Rectangle.LeftCorner = new Point((decimal)MoveTo.X, (decimal)MoveTo.Y);
            request.Tank.IsMoving = true;

            ready = true;
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

        public ServerResponse Client(int msgCount, ServerRequest request)
        {
            

            if (request.Map.Cells != null)
            {
                _map = request.Map;
            }
            else if (null == _map)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
            }

            _map.InteractObjects = request.Map.InteractObjects;

            var tank = request.Tank;
            if (null == tank)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.None };
            }

            if (!tank.IsMoving)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.Go };
            }

            if (tank.IsMoving)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
            }

            rectangle = tank.Rectangle;

            return new ServerResponse { ClientCommand = ClientCommandType.Fire };
        }
    }
}
