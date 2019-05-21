using System;
using TankCommon;
using TankCommon.Enum;
using TankCommon.Objects;
using System.Threading.Tasks;
using System.Threading;

using WebSocket4Net;

namespace TankGuiObserver2
{
    public class Connector : System.IDisposable
    {
        object syncObject = new object();
        Uri _serverUri;
        public bool ServerRunning { get; private set; }
        WebSocketProxy _webSocketProxy;

        public Connector(string server)
        {
            _serverUri = new Uri(server);
            _webSocketProxy = new WebSocketProxy(_serverUri);
        }

        public bool IsServerRunning()
        {
            if (_webSocketProxy.State == WebSocketState.Open)
            {
                ServerRunning = true;
            }
            else
            {
                try
                {
                    if (_webSocketProxy.State != WebSocketState.Open &&
                        _webSocketProxy.State != WebSocketState.Connecting)
                    {
                        _webSocketProxy.Open();
                        Thread.Sleep(300);
                        ServerRunning = (_webSocketProxy.State == WebSocketState.Open);
                        return ServerRunning;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"TankGuiSpectator2.Connector.IsServerRunning {DateTime.Now:T} Исключение во время выполнения: {e}");
                }

            }

            return ServerRunning;
        }

        public void Dispose()
        {
            _webSocketProxy.Dispose();
        }
    }

    class GuiSpectator
    {
        public Map Map { get; set; }
        protected DateTime _lastMapUpdate;
        protected readonly CancellationToken _cancellationToken;
        protected readonly object _syncObject = new object();
        protected int _msgCount;
        protected bool _wasUpdate;

        public GuiSpectator(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
#pragma warning disable 4014
            DisplayMap();
#pragma warning restore 4014
        }

        protected async Task DisplayMap()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100);
                if (!_wasUpdate)
                {
                    continue;
                }

                Map map;
                lock (_syncObject)
                {
                    _wasUpdate = false;
                    map = new Map(Map, Map.InteractObjects);
                }
            }
        }
        public ServerResponse Client(int msgCount, ServerRequest request)
        {
            lock (_syncObject)
            {
                if (request.Map.Cells != null)
                {
                    Map = request.Map;
                    _lastMapUpdate = DateTime.Now;
                }
                else if (Map == null)
                {
                    return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
                }

                Map.InteractObjects = request.Map.InteractObjects;
                _msgCount = msgCount;
                _wasUpdate = true;

                return new ServerResponse { ClientCommand = ClientCommandType.None };
            }
        }
    }

    /// <summary>
    /// WebSocket implementation from
    /// https://www.codeproject.com/Articles/618032/Using-WebSocket-in-NET-Part
    /// </summary>
    public class WebSocketProxy : IDisposable
    {
        private Uri serverUri;
        private WebSocket ws;

        public WebSocketProxy(Uri url)
        {
            serverUri = url;

            var protocol = serverUri.Scheme;
            if (!protocol.Equals("ws") && !protocol.Equals("wss"))
            {
                throw new ArgumentException("Unsupported protocol: " + protocol);
            }

            ws = new WebSocket(serverUri.ToString()) { ReceiveBufferSize = 1024 * 1024 };
            ws.MessageReceived += WsOnMessageReceived;
        }

        protected readonly System.Collections.Generic.Queue<string> _messages = new System.Collections.Generic.Queue<string>();
        protected readonly object _syncObject = new object();

        public int MsgCount
        {
            get
            {
                lock (_syncObject)
                {
                    return _messages.Count;
                }
            }
        }

        public string GetMessage()
        {
            lock (_syncObject)
            {
                return _messages.Count > 0 ? _messages.Dequeue() : (string)null;
            }
        }

        private void WsOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            lock (_syncObject)
            {
                _messages.Enqueue(e.Message);
            }
        }

        public void Open()
        {
            ws.Open();
        }

        public async Task SendAsync(string str, CancellationToken cancellationToken)
        {
            Send(str, cancellationToken);
            await Task.FromResult(0);
        }

        public void Send(string str, CancellationToken cancellationToken)
        {
            ws.Send(str);
        }

        public WebSocketState State => ws.State;

        public void Close()
        {
            if (State == WebSocketState.Open)
            {
                ws.Close();
            }

            ws.Dispose();
        }

        public void Dispose()
        {
            Close();
        }
    }

    public class GuiObserverCore
    {
        protected readonly Uri _serverUri;
        protected readonly string _nickName;
        private WebSocketProxy _webSocketProxy;

        public GuiObserverCore(string server, string nickname)
        {
            _serverUri = new Uri(server);
            _nickName = nickname;
            _webSocketProxy = new WebSocketProxy(_serverUri);
        }

        public void Run(Func<int, ServerRequest, ServerResponse> bot, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Подсоединение к серверу {_serverUri} как {_nickName}...");

            try
            {
                if (_webSocketProxy.State != WebSocketState.Open &&
                    _webSocketProxy.State != WebSocketState.Connecting)
                    _webSocketProxy.Open();

                ServerResponse loginResponse = new ServerResponse
                {
                    CommandParameter = _nickName,
                    ClientCommand = ClientCommandType.Login
                };
                Console.WriteLine($"{DateTime.Now:T} Логин на сервер как {_nickName}");
                _webSocketProxy.Send(loginResponse.ToJson(), cancellationToken);
                Console.WriteLine($"{DateTime.Now:T} Логин успешно выполнен.");

                CancellationTokenSource tokenSource = new CancellationTokenSource();

                while (!cancellationToken.IsCancellationRequested && _webSocketProxy.State == WebSocketState.Open)
                {
                    var inputData = _webSocketProxy.GetMessage();
                    if (string.IsNullOrWhiteSpace(inputData))
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    var serverRequest = inputData.FromJson<ServerRequest>();
                    if (serverRequest?.Map == null)
                    {
                        continue;
                    }

                    var serverResponse = bot(_webSocketProxy.MsgCount, serverRequest);
                    var outputData = serverResponse.ToJson();

                    _webSocketProxy.Send(outputData, cancellationToken);
                }

                if (_webSocketProxy.State != WebSocketState.Open)
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
                        _webSocketProxy.Send(outputData, CancellationToken.None);
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