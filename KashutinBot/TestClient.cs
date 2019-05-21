using System;
using TankCommon;
using TankCommon.Enum;
using TankCommon.Objects;

namespace TankClient
{
    public class Bot1 : IClientBot
    {
        protected Rectangle rectangle;
        protected Map _map;
        private delegate ServerResponse Script();
        private static int stepsBeforeOver = 0;
        private static  Script delegateScript = Stop;
        private static Rectangle lastRectangle;

        public ServerResponse Client(int msgCount, ServerRequest request)
        {
            //Если карта существует присвоить локальной карте карту
            if (request.Map.Cells != null)
            {
                _map = request.Map;
                var translMap = TranslMapForPassfind(_map);
            }
            //Если в карте ничего нет, то просить обновления карты
            else if (null == _map)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
            }

            //записать в карту все интерактивные объекты
            _map.InteractObjects = request.Map.InteractObjects;

            var myTank = request.Tank;
            if (null == myTank)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.None };
            }

            //если размер танка не известен, то узнать
            if (null == rectangle)
            {
                rectangle = myTank.Rectangle;
            }

            rectangle = myTank.Rectangle;

            return BeAliveFirtStep(_map, myTank);
            
        }


        /// <summary>
        /// Выполняет простую методологию по выживанию
        /// </summary>
        /// <returns></returns>
        private ServerResponse BeAliveFirtStep(Map map, TankObject myTank)
        {
            //Если на танковую клетку от меня есть пуля и летит в меня => Уклониться
            ServerResponse serverResponse = GetaweyFromNearBullet(map, myTank);

            //Если жизнь вне опасности выполнять другие функции
            if (serverResponse.ClientCommand == ClientCommandType.None)
            {
                return GoToPointAndFire(map, myTank, 6, 6);
            }
            
            return serverResponse;
        }

        /// <summary>
        /// Двигает танк к указаной точке
        /// </summary>
        /// <param name="map"></param>
        /// <param name="myTank"></param>
        /// <param name="destinationX"></param>
        /// <param name="destinationY"></param>
        /// <returns></returns>
        private static ServerResponse GoToPointAndFire(Map map, TankObject myTank, int destinationX, int destinationY)
        {
            BaseInteractObject nearestObj = FindNearestTankWithoutWalls(map, myTank);
            destinationX = nearestObj.Rectangle.LeftCorner.LeftInt;
            destinationY = nearestObj.Rectangle.LeftCorner.TopInt;

            if (map.InteractObjects != null)
            {
                if ( Math.Abs(destinationX - GetMyX(myTank)) > Math.Abs(destinationY - GetMyY(myTank)) )
                {
                    //Если левый угол моего танка по Х меньше, чем левый угол достигаемого объекта и я не повёрнут направо
                    if (GetMyX(myTank) < destinationX && myTank.Direction != DirectionType.Right)
                    {
                        //повернуть направо
                        return TurnRight();
                    }
                    else
                    {
                        //Если мой левый угол меньше, чем левый угол  апгрейда
                        if (GetMyX(myTank) < destinationX)
                        {
                            //Ехать
                            if (lastRectangle is null || (lastRectangle.LeftCorner.Left != myTank.Rectangle.LeftCorner.Left && lastRectangle.LeftCorner.Top != myTank.Rectangle.LeftCorner.Top))
                            {
                                lastRectangle = myTank.Rectangle;
                                return Go();
                            }
                            else
                            {
                                return Fire();
                            }
                        }
                    }

                    //Если левый угол моего танка по Х больше, чем левый угол достигаемого объекта и я не повёрнут на
                    if (GetMyX(myTank) > destinationX && myTank.Direction != DirectionType.Left)
                    {
                        //повернуть направо
                        return TurnLeft();
                    }
                    else
                    {
                        //Если мой левый угол меньше, чем левый угол  апгрейда
                        if (GetMyX(myTank) > destinationX)
                        {
                            //Ехать
                            if (lastRectangle is null || (lastRectangle.LeftCorner.Left != myTank.Rectangle.LeftCorner.Left && lastRectangle.LeftCorner.Top != myTank.Rectangle.LeftCorner.Top))
                            {
                                lastRectangle = myTank.Rectangle;
                                return Go();
                            }
                            else
                            {
                                return Fire();
                            }
                        }
                    }
                }
                else
                {
                    //Если левый угол моего танка по Y меньше, чем левый угол улучшающего объекта и я не повёрнут вниз
                    if (GetMyY(myTank) < destinationY && myTank.Direction != DirectionType.Down)
                    {
                        //повернуть вниз
                        return TurnDown();
                    }
                    else
                    {
                        //Если мой левый угол меньше по Y, чем левый угол  апгрейда
                        if (GetMyY(myTank) < destinationY)
                        {
                            //Ехать
                            if (lastRectangle is null || (lastRectangle.LeftCorner.Left != myTank.Rectangle.LeftCorner.Left && lastRectangle.LeftCorner.Top != myTank.Rectangle.LeftCorner.Top))
                            {
                                lastRectangle = myTank.Rectangle;
                                return Go();
                                
                            }
                            else
                            {
                                return Fire();
                            }
                        }
                    }
                    
                    //Если левый угол моего танка по Y больше, чем левый угол улучшающего объекта и я не повёрнут вверх
                    if (GetMyY(myTank) > destinationY && myTank.Direction != DirectionType.Up)
                    {
                        //повернуть вверх
                        return TurnUp();
                    }
                    else
                    {
                        //Если мой левый угол меньше, чем левый угол объекта
                        if (GetMyY(myTank) > destinationY)
                        {
                            //Ехать
                            if (lastRectangle is null || (lastRectangle.LeftCorner.Left != myTank.Rectangle.LeftCorner.Left && lastRectangle.LeftCorner.Top != myTank.Rectangle.LeftCorner.Top))
                            {
                                lastRectangle = myTank.Rectangle;
                                return Go();
                            }
                            else
                            {
                                return Fire();
                            }
                        }
                        else
                        {
                            return Fire();
                        }
                    }
                }
            }

            return new ServerResponse { ClientCommand = ClientCommandType.None };
        }

        /// <summary>
        /// Находит ближайший объект (без стен)
        /// </summary>
        /// <param name="map"></param>
        /// <param name="myTank"></param>
        /// <returns></returns>
        private static BaseInteractObject FindNearestTankWithoutWalls(Map map, TankObject myTank)
        {
            int shortestDistToElem = map.MapHeight;
            var lastIndex = map.InteractObjects.Count - 1;
            BaseInteractObject nearestObject = map.InteractObjects[lastIndex];

            //Если есть интерактивные объекты
            if (map.InteractObjects != null)
            {
                foreach (var elem in map.InteractObjects)
                {
                    //Если этот элемент это 
                    if ( elem is TankObject && !Equals(elem.Id, myTank.Id) )
                    {
                        var distToElem = (Math.Abs(elem.Rectangle.LeftCorner.LeftInt - GetMyX(myTank)) + Math.Abs(elem.Rectangle.LeftCorner.TopInt - GetMyY(myTank)));
                        if (distToElem < shortestDistToElem)
                        {
                            shortestDistToElem = distToElem;
                            nearestObject = elem;
                        }
                    }
                }
            }
            return nearestObject;
        }

        /// <summary>
        /// Переводит карту в массив лишь с проходимыми и не проходимыми полями
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        private static int [,] TranslMapForPassfind(Map map)
        {
            var transArr = new int[map.MapHeight, map.MapWidth];
            var arrXLen = transArr.GetLength(0);
            var arrYLen = transArr.GetLength(1);
            for (var x = 0; x < arrXLen; x++)
            {
                for (var y = 0; y < arrYLen; y++) {
                    if (map.Cells[x,y].Equals(CellMapType.Grass) || map.Cells[x, y].Equals(CellMapType.Void)) //Исправить. брать в учёт размер танка
                    {
                        transArr[x, y] = 0;
                    }
                    else
                    {
                        transArr[x, y] = -1;
                    }
                }
            }

            return transArr;
        } //надо переделать учитывая проходимость танка

        /// <summary>
        /// Принимает направление и передвигает туда танк
        /// </summary>
        /// <param name="direction"> 0 = вниз| 1 = вправо| 2 = вверх| 3 = влево</param>
        private static ServerResponse EasyMove(int direction)
        {
            if(direction == 0 && stepsBeforeOver == 0)
            {
                delegateScript = TurnDown;
                stepsBeforeOver = 2;
            }
            if (direction == 1 && stepsBeforeOver == 0)
            {
                delegateScript = TurnRight;
                stepsBeforeOver = 2;
            }
            if (direction == 2 && stepsBeforeOver == 0)
            {
                delegateScript = TurnUp;
                stepsBeforeOver = 2;
            }
            if (direction == 3 && stepsBeforeOver == 0)
            {
                delegateScript = TurnLeft;
                stepsBeforeOver = 2;
            }
            if (stepsBeforeOver == 1)
            {
                delegateScript = Go;
            }
            if (stepsBeforeOver > 0)
            {
                stepsBeforeOver--;
            }

            return delegateScript();
        }

        /// <summary>
        /// Проверяет есть ли пуля на расстоянии одной Танковой клетки вокруг танка  и если есть, то уклоняется
        /// </summary>
        /// <param name="map"></param>
        /// <param name="myTank"></param>
        /// <returns></returns>
        private static ServerResponse GetaweyFromNearBullet(Map map, TankObject myTank)
        {
            var servResp = new ServerResponse { ClientCommand = ClientCommandType.None };
            var myX = myTank.Rectangle.LeftCorner.LeftInt;
            var myY = myTank.Rectangle.LeftCorner.TopInt;

            foreach(var elem in map.InteractObjects)
            {
                if (elem is BulletObject)
                {
                    var bullX = elem.Rectangle.LeftCorner.LeftInt;
                    var bullY = elem.Rectangle.LeftCorner.TopInt;
                    //Если пуля близко
                    if (Math.Abs(myX - bullX) < (Constants.CellHeight * 3) && Math.Abs(myY - bullY) < (Constants.CellWidth * 3))
                    {
                        //Попадёт ли она?
                        if ( IsThisBulletHitTank(bullX, bullY, myX, myY, (elem as BulletObject).Direction) )
                        {
                            //Уклониться в зависимости от направления пули
                            servResp = GoOutFromBulletWay(map, myTank,(elem as BulletObject), (elem as BulletObject).Direction);
                        }
                    }
                }
            }

            return servResp;
        }

        /// <summary>
        /// Проверяет попадёт ли пуля в мой танк
        /// </summary>
        /// <param name="map"></param>
        /// <param name="myTank"></param>
        /// <param name="bullX"></param>
        /// <param name="bullY"></param>
        /// <param name="myX"></param>
        /// <param name="myY"></param>
        /// <param name="bullDirec">Направление пули, которая проверяется на попадение в танк</param>
        /// <returns></returns>
        private static bool IsThisBulletHitTank(int bullX, int bullY , int myX, int myY, DirectionType bullDirec)
        {
            //Если совпала по Х
            if ( bullX >= myX && bullX <= myX + (Constants.CellWidth - 1) )
            {
                //Если выше(меньше) по Y и направлена вниз
                if (bullY < myY && bullDirec == DirectionType.Down)
                {
                    return true;
                }
                //Если ниже(больше) по Y и направлена вверх
                if (bullY > myY && bullDirec == DirectionType.Up)
                {
                    return true;
                }
            }
            if (bullY >= myY && bullY <= myY + (Constants.CellHeight - 1))
            {
                //Если левее(меньше) по X и направлена вправо
                if (bullX < myX && bullDirec == DirectionType.Right)
                {
                    return true;
                }
                //Если правее(больше) по Х и направлена влево
                if (bullX > myX && bullDirec == DirectionType.Left)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Проверяетможет ли танк улониться и в какую сторону и уклоняет танк
        /// </summary>
        /// <param name="map"></param>
        /// <param name="myTank"></param>
        /// <param name="bullet"></param>
        /// <param name="direction">Направление пули</param>
        /// <returns></returns>
        private static ServerResponse GoOutFromBulletWay(Map map, TankObject myTank, BulletObject bullet, DirectionType bullDirec)
        {
            var bullX = bullet.Rectangle.LeftCorner.LeftInt;
            var bullY = bullet.Rectangle.LeftCorner.TopInt;
            var myX = myTank.Rectangle.LeftCorner.LeftInt;
            var myY = myTank.Rectangle.LeftCorner.TopInt;

            //Если совпала по Х
            if (bullX >= myX && bullX <= myX + (Constants.CellWidth - 1))
            {
                //Если выше(меньше) по Y и направлена вниз
                if (bullY < myY && bullDirec == DirectionType.Down)
                {
                    //Если направо можно ходить направо, то увернуться направо
                    if(map.Cells[myX + ((Constants.CellWidth * 2) - 1),myY] == CellMapType.Void || map.Cells[myX, myY] == CellMapType.Grass)
                    {
                        if (myTank.Direction != DirectionType.Right)
                        {
                            return new ServerResponse { ClientCommand = ClientCommandType.TurnRight };
                        }
                        else
                        {
                            return new ServerResponse { ClientCommand = ClientCommandType.Go };
                        }
                    }
                    else
                    {
                        //Если можно ходить налево, то увернуться налево
                        if (map.Cells[myX - Constants.CellWidth, myY] == CellMapType.Void || map.Cells[myX, myY] == CellMapType.Grass)
                        {
                            if (myTank.Direction != DirectionType.Left)
                            {
                                return new ServerResponse { ClientCommand = ClientCommandType.TurnLeft };
                            }
                            else
                            {
                                return new ServerResponse { ClientCommand = ClientCommandType.Go };
                            }
                        }
                        //Иначе стрелять в снаряд
                    }
                }
                //Если ниже(больше) по Y и направлена вверх
                if (bullY > myY && bullDirec == DirectionType.Up)
                {
                    
                }
            }

            return new ServerResponse { ClientCommand = ClientCommandType.None };
        }

        private static ServerResponse Fire()
        {
            return new ServerResponse { ClientCommand = ClientCommandType.Fire };
        }

        private static ServerResponse TurnRight()
        {
            return new ServerResponse { ClientCommand = ClientCommandType.TurnRight };
        }

        private static ServerResponse TurnLeft()
        {
            return new ServerResponse { ClientCommand = ClientCommandType.TurnLeft };
        }

        private static ServerResponse TurnUp()
        {
            return new ServerResponse { ClientCommand = ClientCommandType.TurnUp };
        }

        private static ServerResponse TurnDown()
        {
            return new ServerResponse { ClientCommand = ClientCommandType.TurnDown };
        }

        private static ServerResponse Go()
        {
            return new ServerResponse { ClientCommand = ClientCommandType.Go };
        }

        private static ServerResponse Stop()
        {
            return new ServerResponse { ClientCommand = ClientCommandType.Stop };
        }

        private static int GetMyX(TankObject tank)
        {
            return tank.Rectangle.LeftCorner.LeftInt;
        }

        private static int GetMyY(TankObject tank)
        {
            return tank.Rectangle.LeftCorner.TopInt;
        }

    }
}