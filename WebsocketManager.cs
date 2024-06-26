﻿using System;
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

        protected virtual void OnWebSocketClose(object sender, EventArgs e)
        {
            IsConnected = false;
            IsHost = false;
            ConnectionPending = false;
            Plugin.LogInfo("Disconnected from websocket");
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
    }
}
