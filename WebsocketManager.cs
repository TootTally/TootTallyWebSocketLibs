using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace TootTallyWebsocketLibs
{
    public class WebsocketManager
    {
        private WebSocket _websocket;
        public bool IsHost { get; private set; }
        public bool IsConnected { get; private set; }
        public bool ConnectionPending { get; protected set; }

        protected string _id { get; private set; }
        protected string _url { get; private set; }
        protected string _version { get; private set; }

        public WebsocketManager(string id, string url, string version)
        {
            _url = url;
            _id = id;
            _version = version;
        }

        public void SendToSocket(byte[] data)
        {
            if (HasAnyErrorsCheck()) return;
            _websocket.SendAsync(data, null);
        }

        public void SendToSocket(string data)
        {
            if (HasAnyErrorsCheck()) return;
            _websocket.SendAsync(data, null);
        }

        public bool HasAnyErrorsCheck(bool outputToLogs = true)
        {
            if (_websocket == null)
            {
                Plugin.LogError("Websocket was null.");
                return true;
            }
            return false;
        }

        protected virtual void OnDataReceived(object sender, MessageEventArgs e) { }

        protected virtual void CloseWebsocket()
        {
            _websocket.CloseAsync();
            Plugin.LogInfo("Disconnecting from " + _websocket.Url);
            _websocket = null;
        }

        protected virtual void OnWebSocketOpen(object sender, EventArgs e)
        {
            IsConnected = true;
            ConnectionPending = false;
            Plugin.LogInfo($"Connected to WebSocket server {_websocket.Url}");
        }

        protected virtual void OnWebSocketClose(object sender, CloseEventArgs e)
        {
            IsConnected = false;
            IsHost = false;
            ConnectionPending = false;
            if (codeToReasonDict.ContainsKey(e.Code))
                Plugin.LogInfo($"Disconnected from server [{e.Code}]: {codeToReasonDict[e.Code]}.");
            else
                Plugin.LogInfo($"Disconnected from server [{e.Code}]: {e.Reason}.");
        }

        protected virtual void OnWebSocketError(object sender, ErrorEventArgs e)
        {
            IsConnected = false;
            IsHost = false;
            ConnectionPending = false;
            Plugin.LogError(e.Message);
        }

        public void ConnectToWebSocketServer(string url, string apiKey, bool isHost)
        {
            _websocket = CreateNewWebSocket(url);
            _websocket.CustomHeaders = new Dictionary<string, string>() { { "Authorization", "APIKey " + apiKey }, { "Version", _version } };
            Plugin.LogInfo($"Connecting to WebSocket server...");
            IsHost = isHost;
            _websocket.ConnectAsync();
        }

        private WebSocket CreateNewWebSocket(string url)
        {
            var ws = new WebSocket(url);
            //if (Plugin.Instance.DebugMode.Value) 
            //ws.Log.Level = LogLevel.Debug; //Too risky since it shows API KEY in the logs
            ws.OnError += (sender, e) => { Plugin.LogError(e.Message); };
            ws.OnOpen += OnWebSocketOpen;
            ws.OnClose += OnWebSocketClose;
            ws.OnMessage += OnDataReceived;
            ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            return ws;
        }

        public static Dictionary<ushort, string> codeToReasonDict = new Dictionary<ushort, string>()
        {
            {1000, "Disconnected" },
            {1005, "Self-Disconnected" },
            {1001, "Forced Disconnected" },
            {4001, "Invalid API Key" },
            {4002, "Invalid Version" },
            {4003, "Outdated Version" },
            {4101, "Multiplayer Server Only" },
            {4102, "Invalid Spectator ID" },
            {4103, "Already Broadcasting" },
            {4104, "Already Spectating Someone" },
            {4105, "Invalid Userstate" },
            {4106, "Unauthorized Spectating" },
            {4201, "Invalid Lobby Code" },
            {4202, "Wrong Password" },
            {4203, "Exceeded Player Cap" },
            {4204, "Banned From Lobby" },
        };
    }
}
