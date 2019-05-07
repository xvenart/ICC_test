using TankCommon;
using TankCommon.Enum;
using TankCommon.Objects;

namespace TankClient.TestBot
{
    class BattleField
    {
        public static int[,] LocationMap;
        //public static int EnemyCount;
        //public static int PlayerCount = 1;
        public static int X;
        public static int Y;

        public static BaseInteractObject EnemyTank;
        public static BaseInteractObject MyTank;

        public static bool ready = false;

        //Инициализация массива для карты и её заполнение, начало рассчёта кратчайшего пути
        public void Start(ServerRequest request)
        {
            var Cells = request.Map.Cells;
            LocationMap = new int[Cells.GetLength(0), Cells.GetLength(1)];

            for (var i = 0; i < Cells.GetLength(0); i++)
            {
                for (var j = 0; j < Cells.GetLength(1); j++)
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

            ready = true;
        }

        public void Update()
        {

        }

        //отмечаем на карте позиции врагов
        public void SetEnemiesPositionsOnMap(ServerRequest request)
        {
            foreach (var i in request.Map.InteractObjects)
            {
                if (i is TankObject)
                {
                    LocationMap[i.Rectangle.LeftCorner.TopInt, i.Rectangle.LeftCorner.LeftInt] = -1;
                }
            }
        }

        //отмечаем на карте позицию бота
        public void SetBotPosition(ServerRequest request)
        {
            LocationMap[request.Tank.Rectangle.LeftCorner.TopInt, request.Tank.Rectangle.LeftCorner.LeftInt] = -1;
        }
    }
}
