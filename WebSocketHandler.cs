using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace TestRunnerKafkaConsumer
{
    public class WebSocketHandler
    {
        private HttpListener listener;
        public static ConcurrentDictionary<string, WebSocket> connectedClients;

        public WebSocketHandler(string listenerPrefix)
        {
            listener = new HttpListener();
            listener.Prefixes.Add(listenerPrefix);
            listener.Start();
            connectedClients = new ConcurrentDictionary<string, WebSocket>();
            Console.WriteLine($"Listening for WebSocket connections on {listenerPrefix}");
            StartAcceptWebSocket();
        }

        private void StartAcceptWebSocket()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                        WebSocket webSocket = webSocketContext.WebSocket;
                        Guid clientId = Guid.NewGuid();
                        connectedClients.TryAdd(clientId.ToString(), webSocket);
                        HandleClientWebSocket(webSocket, clientId);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            });
        }

        private async Task HandleClientWebSocket(WebSocket webSocket, Guid clientId)
        {
            var buffer = new ArraySegment<byte>(new byte[1024]);
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    // Обработка принятого сообщения
                    Console.WriteLine($"Received message from client {clientId}: {message}");
                    if(message == "Run Finished")
                    {
                        connectedClients.TryAdd(clientId.ToString(), webSocket);
                    }
                    // Отправка обратно клиенту
                    //byte[] responseBuffer = Encoding.UTF8.GetBytes("Message received: " + message);
                    //await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            connectedClients.TryRemove(clientId.ToString(), out _);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Socket closed", CancellationToken.None);
            webSocket.Dispose();
        }
        public async Task Send(WebSocket webSocket, string message)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
