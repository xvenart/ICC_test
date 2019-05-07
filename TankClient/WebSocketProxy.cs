using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;
using WebSocket = WebSocket4Net.WebSocket;
using WebSocketState = WebSocket4Net.WebSocketState;

namespace TankClient
{
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

            ws = new WebSocket(serverUri.ToString()) {ReceiveBufferSize = 1024 * 1024};
            ws.MessageReceived += WsOnMessageReceived;
        }

        protected readonly Queue<string> _messages = new Queue<string>();
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
}
