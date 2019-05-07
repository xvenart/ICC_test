using System;
using System.Linq;
using System.Net;
using System.Threading;
using TankCommon;
using TankServer;

namespace ICC_Tank
{
    class Program
    {
        static uint ParseOrDefault(string v, uint defaultValue)
        {
            uint u;

            if (string.IsNullOrWhiteSpace(v))
            {
                return defaultValue;
            }

            if (uint.TryParse(v, out u))
            {
                return u;
            }

            return defaultValue;
        }

        static void Main(string[] args)
        {
            var mapList = MapManager.GetMapList();
            if (mapList.Count == 0)
            {
                Console.WriteLine("Нет ни одной карты в списке.");
                return;
            }

            var idx = 1;
            if (mapList.Count > 1)
            {
                Console.WriteLine("Введите номер карты для старта:");
                foreach (var m in mapList)
                {
                    Console.WriteLine($"{idx++}. {m}");
                }

                while (true)
                {
                    Console.Write("> ");
                    var n = Console.ReadLine();
                    if (!int.TryParse(n, out idx) || (idx < 1 || idx > mapList.Count))
                    {
                        Console.WriteLine($"Введите число от 1 до {mapList.Count}");
                        continue;
                    }

                    break;
                }
            }

            var map = MapManager.LoadMap(mapList[idx-1]);
            Console.WriteLine($"Используется карта: {mapList[idx - 1]}");

            var port = ParseOrDefault(System.Configuration.ConfigurationManager.AppSettings["port"], 2000);
            var maxBotsCount = ParseOrDefault(System.Configuration.ConfigurationManager.AppSettings["maxBotsCount"], 1000);
            var coreUpdateMs = ParseOrDefault(System.Configuration.ConfigurationManager.AppSettings["coreUpdateMs"], 100);
            var spectatorUpdateMs = ParseOrDefault(System.Configuration.ConfigurationManager.AppSettings["spectatorUpdateMs"], 100);
            var botUpdateMs = ParseOrDefault(System.Configuration.ConfigurationManager.AppSettings["botUpdateMs"], 250);

            var strHostName = Dns.GetHostName();
            var ipEntry = Dns.GetHostEntry(strHostName);
            var ipAddresses = ipEntry.AddressList;

            Console.WriteLine($"Соединение по имени: ws://{strHostName}:{port}");
            Console.WriteLine("Или по IP адресу(ам):");
            foreach (var ipAddress in ipAddresses)
            {
                Console.WriteLine($"\tws://{ipAddress}:{port}");
            }
            
            Console.WriteLine("Нажмите Escape для выхода");

            var tokenSource = new CancellationTokenSource();
            var server = new Server(map, port, maxBotsCount, coreUpdateMs, spectatorUpdateMs, botUpdateMs);
            var serverTask = server.Run(tokenSource.Token);

            try
            {
                while (!serverTask.IsCompleted)
                {
                    Thread.Sleep(100);

                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    {
                        tokenSource.Cancel();
                    }
                }
            }
            finally
            {
                server.Dispose();

                Console.WriteLine("Завершение работы сервера. Нажмите Enter для выхода");
                Console.ReadLine();
            }
        }
    }
}
