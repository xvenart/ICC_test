using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TankCommon.Enum;
using TankCommon.Objects;

namespace TankCommon
{
    public static class MapManager
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
        public static Map LoadMap(int mapHeight = 20, int mapWidth = 20, CellMapType primaryObject = CellMapType.Grass, int percentOfPrimObj = 50, int percentAnotherObj = 50)
        {
            var mapData = GenerateMap(mapHeight, mapWidth, primaryObject, percentOfPrimObj, percentAnotherObj);
            var cells = new List<List<CellMapType>>();

            if (percentOfPrimObj + percentAnotherObj > 100)
            {
                throw new InvalidDataException("Это сочетание работоспособно, но нет смысла ставить процентов больше 100");
            }

            if (mapData.GetLength(0) <= 5 || mapData.GetLength(1) <= 5)
            {
                throw new InvalidDataException("Слишком маленькая карта");
            }

            //Финальный массив должен быть умножен на ширину и длинну константных клеток
            var cellArr = new CellMapType[mapHeight * Constants.CellHeight, mapWidth * Constants.CellWidth];
            var cellArrHeight = cellArr.GetLength(0);
            var cellArrWidth = cellArr.GetLength(1);
            //Масштабирую карту исходя из констант
            for (var height = 0; height < cellArrHeight; height++)
            {
                for (var width = 0; width < cellArrWidth; width++)
                {
                    var oldHeight = (int)Math.Ceiling((decimal)(height / Constants.CellHeight));
                    var oldWidth = (int)Math.Ceiling((decimal)(width / Constants.CellHeight));
                    cellArr[height, width] = mapData[oldHeight, oldWidth];
                }
            }

            
            return new Map(cellArr);
        }


        public static Map ReadMap(MapType mapType)
        {
            string fileName;
            //В зависимости от пришедшего типа карты выбираю файл карты
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
                default:
                    throw new InvalidDataException("Неизвестный тип карты");
            }
            using (FileStream fstream = File.OpenRead(@"../maps/" + fileName))
            {
                // Преобразуем строку в байты
                byte[] array = new byte[fstream.Length];
                // считываем данные
                fstream.Read(array, 0, array.Length);
                // Декодируем байты в строку
                string textFromFile = System.Text.Encoding.Default.GetString(array);
                return TranslateFromTxt(textFromFile);
            }
        }

        private static Map TranslateFromTxt(string textFromFile)
        {
            var map = new Map();
            var mapHeight = 0;
            //var mapWidth = 0;
            //var widthCount = 0;

            foreach (var symbol in textFromFile)
            {
                if (symbol == '\n')
                {
                    mapHeight++;
                }
                else
                {
                    
                }
            }
            //ВЫчисляю из файла ширину и высоту карты, не считаю символы(\n и \r)
            map.MapHeight = mapHeight + 1;
            map.MapWidth = (textFromFile.Length + 1 - (map.MapHeight * 2)) / map.MapHeight;
            map.CellHeight = Constants.CellHeight;
            map.CellWidth = Constants.CellWidth;

            map = ConvertFromMap(map, textFromFile);
            map.InteractObjects = new List<BaseInteractObject>();
            return map;
        }

        /// <summary>
        /// Прочитанную карту переводит в CellMapType
        /// </summary>
        /// <param name="map"></param>
        /// <param name="textMap"></param>
        /// <returns></returns>
        private static Map ConvertFromMap(Map map, string textMap)
        {
            var x = 0;
            var y = 0;
            var mapData = new CellMapType[map.MapHeight, map.MapWidth];
            for (var i = 1; i < textMap.Length; i++)
            {
                var symbol = textMap[i];
                switch (symbol)
                {
                    case 'с':
                        mapData[y, x] = CellMapType.Wall;
                        x++;
                        break;
                    case 'c':
                        mapData[y, x] = CellMapType.Wall;
                        x++;
                        break;
                    case 'т':
                        mapData[y, x] = CellMapType.Grass;
                        x++;
                        break;
                    case 'в':
                        mapData[y, x] = CellMapType.Water;
                        x++;
                        break;
                    case ' ':
                        mapData[y, x] = CellMapType.Void;
                        x++;
                        break;
                    case 'д':
                        mapData[y, x] = CellMapType.DestructiveWall;
                        x++;
                        break;
                    default:
                        if (symbol.Equals('\n') || symbol.Equals('\r'))
                        {
                            Console.Write("");
                            break;
                        }
                        throw new InvalidDataException("Неизвестный символ");
                }
                if (x == map.MapWidth)
                {
                    y++;
                    x = 0;
                }
            }

            //Финальный массив должен быть умножен на ширину и длинну константных клеток
            var cellArr = new CellMapType[map.MapHeight * Constants.CellHeight, map.MapWidth * Constants.CellWidth];
            var cellArrHeight = cellArr.GetLength(0);
            var cellArrWidth = cellArr.GetLength(1);
            //Масштабирую карту исходя из констант
            for (var height = 0; height < cellArrHeight; height++)
            {
                for (var width = 0; width < cellArrWidth; width++)
                {
                    var oldHeight = (int)Math.Ceiling((decimal)(height / Constants.CellHeight));
                    var oldWidth = (int)Math.Ceiling((decimal)(width / Constants.CellHeight));
                    cellArr[height, width] = mapData[oldHeight, oldWidth];
                }
            }

            map.Cells = cellArr;
            map.MapHeight = map.Cells.GetLength(0);
            map.MapWidth = map.Cells.GetLength(1);
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
            var preMap = new CellMapType[mapHeight,mapWidth];
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
            map = DrawVerticals(map, percentOfPrimObj, percentAnotherObj, rnd);
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
            var arrSymbols = new CellMapType[] { wall, water, grass, dWall, field};
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
