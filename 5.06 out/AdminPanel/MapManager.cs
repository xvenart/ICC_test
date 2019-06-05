using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TankCommon.Enum;
using TankCommon.Objects;

namespace AdminPanel
{
    public class MapManager
    {
        /// <summary>
        /// Создаёт карту по переданным параметрам
        /// </summary>
        /// <param name="mapHeight">Высота создаваемой карты</param>
        /// <param name="mapWidth">Ширина создаваемой карты</param>
        /// <param name="primaryObject">Объекты, которых должно быть больше всего на карте</param>
        /// <param name="percentOfPrimObj">Процент объектов преобладающих на карте</param>
        /// <param name="percentAnotherObj">Процент присутствия на карте второстепенных объектов</param>
        /// <returns>Объект типа карта</returns>
        public static CellMapType[,] LoadMap(int mapHeight = 20, int mapWidth = 20, CellMapType primaryObject = CellMapType.Grass, int percentOfPrimObj = 50, int percentAnotherObj = 50)
        {
            //формирование карты 
            var mapData = GenerateMap(mapHeight, mapWidth, primaryObject, percentOfPrimObj, percentAnotherObj);

            if (percentOfPrimObj + percentAnotherObj > 100)
            {
                throw new InvalidDataException("Это сочетание работоспособно, но нет смысла ставить процентов больше 100");
            }

            if (mapData.GetLength(0) <= 5 || mapData.GetLength(1) <= 5)
            {
                throw new InvalidDataException("Слишком маленькая карта");
            }

            return mapData;
        }

        /// <summary>
        /// Чтение карты из файла
        /// </summary>
        /// <param name="mapType"></param>
        /// <returns name="TranslateFromText"></returns>
        public static Map ReadMap(MapType mapType)
        {
            //имя файла с картой
            string fileName;
            //в зависимости от пришедшего типа карты выбирается файл карты
            switch (mapType)
            {
                case MapType.Manual_Map_1:
                    fileName = "Manual_Map_1.txt";
                    break;
                case MapType.Manual_Map_2:
                    fileName = "Manual_Map_2.txt";
                    break;
                case MapType.Promotional:
                    fileName = "Promotional.txt";
                    break;
                case MapType.Test_Map:
                    fileName = "Test.txt";
                    break;
                default:
                    throw new InvalidDataException("Неизвестный тип карты");
            }

            //чтение из файла
            //var SR = new StreamReader(@"../maps/" + fileName, Encoding.ASCII, true);
            //var textFromFile = SR.ReadToEnd();
            var bytesFromFile = File.ReadAllBytes(@"../maps/" + fileName);
            var textFromFile = Encoding.ASCII.GetString(bytesFromFile, 0, bytesFromFile.Length);

            //разбиение полученной строки на массив символов
            var mapContext = textFromFile.Split('\n');

            for (var i = 0; i < mapContext.GetLength(0); i++)
            {
                mapContext[i] = mapContext[i].Replace("\n", "").Replace("\r", "");
            }

            var mapHeight = mapContext.Count();
            var mapWidth = mapContext[0].Count();

            var map = new Map();
            var mCells = new CellMapType[mapHeight, mapWidth];

            for (var x = 0; x < mapHeight; x++)
            {
                for (var y = 0; y < mapWidth; y++)
                {
                    switch (mapContext[x][y])
                    {
                        case 'c':
                            mCells[x, y] = CellMapType.Wall;
                            break;
                        case 'с':
                            mCells[x, y] = CellMapType.Wall;
                            break;
                        case 'т':
                            mCells[x, y] = CellMapType.Grass;
                            break;
                        case 'в':
                            mCells[x, y] = CellMapType.Water;
                            break;
                        case ' ':
                            mCells[x, y] = CellMapType.Void;
                            break;
                        case 'д':
                            mCells[x, y] = CellMapType.DestructiveWall;
                            break;
                        default:
                            mCells[x, y] = CellMapType.Wall;
                            break;
                    }
                }
            }            

            map.Cells = mCells;
            map.MapHeight = map.Cells.GetLength(1);
            map.MapWidth = map.Cells.GetLength(0);
            map.CellHeight = Constants.CellHeight;
            map.CellWidth = Constants.CellWidth;
            map.InteractObjects = new List<BaseInteractObject>();

            return map;
        }

        public static List<KeyValuePair<Point, CellMapType>> WhatOnMap(Rectangle rectangle, Map map)
        {
            var left = rectangle.LeftCorner.LeftInt;
            var top = rectangle.LeftCorner.TopInt;

            var result = new List<KeyValuePair<Point, CellMapType>>(rectangle.WidthInt * rectangle.HeightInt);
            for (var i = top; i < top + rectangle.Height; i++)
            {
                for (var j = left; j < left + rectangle.Width; j++)
                {
                    result.Add(new KeyValuePair<Point, CellMapType>(new Point(j, i), map[i, j]));
                }
            }

            return result;
        }

        public static BaseInteractObject GetObjectAtPoint(Point location, IEnumerable<BaseInteractObject> interactObjects)
        {
            return interactObjects.FirstOrDefault(i => i.Rectangle.IsPointInRectange(location));
        }

        public static BaseInteractObject GetIntersectedObject(Rectangle rectangle, IEnumerable<BaseInteractObject> interactObjects)
        {
            return interactObjects.FirstOrDefault(i => i.Rectangle.IsRectangleIntersected(rectangle));
        }

        /// <summary>
        ///Генерирует квадратную карту по переданным высоте и ширине
        /// </summary>
        /// <param name="mapHeight"></param>
        /// <param name="mapWidth"></param>
        /// <param name="primaryObject"></param>
        /// <param name="percentOfPrimObj"></param>
        /// <param name="percentAnotherObj"></param>
        /// <returns>Двумерный массив сосотоящий из CellMapType</returns>
        private static CellMapType[,] GenerateMap(int mapHeight, int mapWidth, CellMapType primaryObject, int percentOfPrimObj, int percentAnotherObj)
        {
            //создаю и заполняю массив, по краям карты ставлю стены
            var preMap = new CellMapType[mapHeight, mapWidth];
            for (var y = 0; y < mapHeight; y++)
            {
                for (var x = 0; x < mapWidth; x++)
                {
                    if (y == 0 || x == 0 || y == mapHeight - 1 || x == mapWidth - 1)
                    {
                        preMap[y, x] = CellMapType.Wall;
                    }
                    else
                    {
                        preMap[y, x] = CellMapType.Void;
                    }
                }
            }

            //вызов метода для наполнения карты
            preMap = GeneratePrimitiveOnMap(preMap, primaryObject, percentOfPrimObj, percentAnotherObj);
            return preMap;
        }

        /// <summary>
        /// Генерирует псевдорандомную карту состоящую из сплошных линий
        /// </summary>
        /// <param name="map"></param>
        /// <param name="primaryObject"></param>
        /// <param name="percentOfPrimObj"></param>
        /// <param name="percentAnotherObj"></param>
        /// <returns></returns>
        private static CellMapType[,] GeneratePrimitiveOnMap(CellMapType[,] map, CellMapType primaryObject, int percentOfPrimObj, int percentAnotherObj)
        {
            var rnd = new Random();
            //отрисовка "вертикальных объектов"
            map = DrawVerticals(map, percentOfPrimObj, percentAnotherObj, rnd);
            //отрисовка "горизонтальных объектов"
            map = DrawHorizontals(map, primaryObject, percentOfPrimObj, rnd);

            return map;
        }

        /// <summary>
        /// Рисует горизонтальные линии на карте из тех объектов, которых должно быть больше
        /// </summary>
        /// <param name="map"></param>
        /// <param name="symbol"></param>
        /// <param name="percentOfPrimObj"></param>
        /// <param name="rnd"></param>
        /// <returns></returns>
        private static CellMapType[,] DrawHorizontals(CellMapType[,] map, CellMapType symbol, int percentOfPrimObj, Random rnd)
        {
            int rndNum;
            var mapWidth = map.GetLength(1);
            var mapHeight = map.GetLength(0);
            for (var y = 2; y < mapHeight - 2; y++)
            {
                rndNum = rnd.Next(0, 100);
                for (var x = 2; x < mapWidth - 2; x++)
                {
                    if (rndNum < percentOfPrimObj)
                    {
                        if (map[y - 1, x - 1] != CellMapType.Wall && map[y - 1, x - 1] != CellMapType.Water && map[y - 1, x - 1] != CellMapType.DestructiveWall &&
                            map[y + 1, x - 1] != CellMapType.Wall && map[y + 1, x - 1] != CellMapType.Water && map[y + 1, x - 1] != CellMapType.DestructiveWall)
                        {
                            //создаю строку того типа, которого должно быть больше
                            if (x % 4 == 0 || x % 5 == 0)
                            {
                                map[y, x] = symbol;
                            }
                        }
                    }
                }
            }
            return map;
        }

        /// <summary>
        /// Рисует вертикальные линии на карте, проверяя не пересёкся ли он с непроходимой линией и устраняя непроходимость
        /// </summary>
        /// <param name="map"></param>
        /// <param name="rnd"></param>
        /// <param name="percentOfPrimObj">Для вычисления необходимого количества остальных объектов</param>
        /// <param name="percentAnotherObj">Сколько процентов площади должны занимать не преобладающие на карте объекты</param>
        /// <returns></returns>
        private static CellMapType[,] DrawVerticals(CellMapType[,] map, int percentOfPrimObj, int percentAnotherObj, Random rnd)
        {
            var wall = CellMapType.Wall;
            var water = CellMapType.Water;
            var dWall = CellMapType.DestructiveWall;
            var field = CellMapType.Void;
            var grass = CellMapType.Grass;

            int rndNum;
            var mapHeight = map.GetLength(0);
            var mapWidth = map.GetLength(1);
            var arrSymbols = new CellMapType[] { wall, water, grass, dWall, field };
            for (var x = 2; x < mapWidth - 2; x++)
            {
                rndNum = rnd.Next(0, 100);
                var rndForObj = rnd.Next(0, 5);
                for (var y = 2; y < mapHeight - 2; y++)
                {
                    if (rndNum < (percentOfPrimObj + percentAnotherObj) && rndNum > percentOfPrimObj)
                    {
                        if (y % 4 == 0 || y % 5 == 0)
                        {
                            map[y, x] = arrSymbols[rndForObj];
                        }
                    }
                }
            }
            return map;
        }
    }
}
