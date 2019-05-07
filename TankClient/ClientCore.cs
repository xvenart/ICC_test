using System;
using System.Threading;
using System.Threading.Tasks;
using TankCommon;
using TankCommon.Enum;
using WebSocket4Net;

namespace TankClient
{
    public class ClientCore
    {
        protected readonly Uri _serverUri;
        protected readonly string _nickName;

        public ClientCore(string server, string nickname)
        {
            _serverUri = new Uri(server);
            _nickName = nickname;
        }

        public void Run(bool verbose, Func<int, ServerRequest, ServerResponse> bot, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Подсоединение к серверу {_serverUri} как {_nickName}...");

            using (var wc = new WebSocketProxy(_serverUri))
            {
                try
                {
                    wc.Open();
                    var loginResponse = new ServerResponse
                    {
                        CommandParameter = _nickName,
                        ClientCommand = ClientCommandType.Login
                    };
                    Console.WriteLine($"{DateTime.Now:T} Логин на сервер как {_nickName}");
                    wc.Send(loginResponse.ToJson(), cancellationToken);
                    Console.WriteLine($"{DateTime.Now:T} Логин успешно выполнен.");

                    var tokenSource = new CancellationTokenSource();

                    while (!cancellationToken.IsCancellationRequested && wc.State == WebSocketState.Open)
                    {
                        var inputData = wc.GetMessage();
                        if (string.IsNullOrWhiteSpace(inputData))
                        {
                            Thread.Sleep(10);
                            continue;
                        }

                        if (verbose)
                        {
                            Console.Write(new string(' ', Console.WindowWidth - 1));
                            Console.CursorLeft = 0;
                            Console.WriteLine( $"{DateTime.Now:T} Получено состояние от сервера, {inputData.Length} байт");
                        }

                        var serverRequest = inputData.FromJson<ServerRequest>();
                        if (serverRequest?.Map == null)
                        {
                            continue;
                        }

                        var serverResponse = bot(wc.MsgCount, serverRequest);
                        var outputData = serverResponse.ToJson();

                        if (verbose)
                        {
                            Console.WriteLine();
                            Console.Write(new string(' ', Console.WindowWidth - 1));
                            Console.CursorLeft = 0;
                            Console.WriteLine($"Сообщений в очереди {wc.MsgCount}");

                            Console.Write(new string(' ', Console.WindowWidth - 1));
                            Console.CursorLeft = 0;
                            Console.WriteLine($"{DateTime.Now:T} Передача команды серверу: {serverResponse.ClientCommand}");
                        }

                        wc.Send(outputData, cancellationToken);

                        if (verbose)
                        {
                            Console.Write(new string(' ', Console.WindowWidth - 1));
                            Console.CursorLeft = 0;
                            Console.WriteLine($"{DateTime.Now:T} Команда серверу передана.");
                        }
                    }

                    if (wc.State != WebSocketState.Open)
                    {

                        Console.WriteLine($"{DateTime.Now:T} Закрыто соединение с сервером");
                    }
                    else
                    {
                        Console.WriteLine($"{DateTime.Now:T} Запрос на прекращение работы");
                        var logoutResponse = new ServerResponse
                        {
                            ClientCommand = ClientCommandType.Logout
                        };
                        var outputData = logoutResponse.ToJson();

                        try
                        {
                            wc.Send(outputData, CancellationToken.None);
                        }
                        catch
                        {
                        }

                        Thread.Sleep(1000);
                    }
                    
                    tokenSource.Cancel();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{DateTime.Now:T} Исключение во время выполнения: {e}");
                }
            }
        }
    }
}
