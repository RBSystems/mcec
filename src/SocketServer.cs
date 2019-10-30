﻿//-------------------------------------------------------------------
// Copyright © 2019 Kindel Systems, LLC
// http://www.kindel.com
// charlie@kindel.com
// 
// Published under the MIT License.
// Source on GitHub: https://github.com/tig/mcec  
//-------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using log4net;

namespace MCEControl {
    /// <summary>
    /// Implements the TCP/IP server using asynchronous sockets
    /// </summary>
    sealed public class SocketServer : ServiceBase, IDisposable {
        // An ConcurrentDictionary is used to keep track of worker sockets that are designed
        // to communicate with each connected client. For thread safety.
        private readonly ConcurrentDictionary<int, Socket> _clientList = new ConcurrentDictionary<int, Socket>();

        // The following variable will keep track of the cumulative 
        // total number of clients connected at any time. Since multiple threads
        // can access this variable, modifying this variable should be done
        // in a thread safe manner
        // TODO: Ask and asnwer the question: "Why not just use _clientList.Count?". It has "snapshot semantics". 
        private int _clientCount;

        #region IDisposable Members
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        // Disposable members
        private Socket _mainSocket;

        private void Dispose(bool disposing) {
            Log4.Debug("SocketServer disposing...");
            if (!disposing) return;
            foreach (var i in _clientList.Keys) {
                Socket socket;
                _clientList.TryRemove(i, out socket);
                if (socket != null) {
                    Log4.Debug("Closing Socket #" + i);
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
            if (_mainSocket != null) {
                _mainSocket.Shutdown(SocketShutdown.Both);
                _mainSocket.Close();
                _mainSocket = null;
            }
        }

        //-----------------------------------------------------------
        // Control functions (Start, Stop, etc...)
        //-----------------------------------------------------------
        public void Start(int port) {

            try {
                Log4.Debug("SocketServer Start");
                // Create the listening socket...
                _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ipLocal = new IPEndPoint(IPAddress.Any, port);
                // Bind to local IP Address...
                Log4.Debug("Binding to IP address: " + ipLocal.Address + ":" + ipLocal.Port);
                _mainSocket.Bind(ipLocal);
                // Start listening...
                Log4.Debug("_mainSocket.Listen");
                _mainSocket.Listen(4);
                // Create the call back for any client connections...
                SetStatus(ServiceStatus.Started);
                SetStatus(ServiceStatus.Waiting);
                _mainSocket.BeginAccept(OnClientConnect, null);
            }
            catch (SocketException se) {
                SendNotification(ServiceNotification.Error, CurrentStatus, null, $"Start: {se.Message}, {se.HResult:X} ({se.SocketErrorCode})");
                SetStatus(ServiceStatus.Stopped);
            }
        }

        public void Stop() {
            Log4.Debug("SocketServer Stop");
            Dispose(true);
            SetStatus(ServiceStatus.Stopped);
        }

        //-----------------------------------------------------------
        // Async handlers
        //-----------------------------------------------------------
        private void OnClientConnect(IAsyncResult async) {
            Log4.Debug("SocketServer OnClientConnect");

            if (_mainSocket == null) return;
            ServerReplyContext serverReplyContext = null;
            try {
                // Here we complete/end the BeginAccept() asynchronous call
                // by calling EndAccept() - which returns the reference to
                // a new Socket object
                var workerSocket = _mainSocket.EndAccept(async);

                // Now increment the client count for this client 
                // in a thread safe manner
                Interlocked.Increment(ref _clientCount);

                // Add the workerSocket reference to the list
                _clientList.GetOrAdd(_clientCount, workerSocket);

                serverReplyContext = new ServerReplyContext(this, workerSocket, _clientCount);

                Log4.Debug("Opened Socket #" + _clientCount);

                SetStatus(ServiceStatus.Connected);
                SendNotification(ServiceNotification.ClientConnected, CurrentStatus, serverReplyContext);

                // Send a welcome message to client
                // TODO: Notify client # & IP address
                //string msg = "Welcome client " + _clientCount + "\n";
                //SendMsgToClient(msg, m_clientCount);

                // Let the worker Socket do the further processing for the 
                // just connected client
                BeginReceive(serverReplyContext);
            }
            catch (SocketException se) {
                SendNotification(ServiceNotification.Error, CurrentStatus, serverReplyContext, $"OnClientConnect: {se.Message}, {se.HResult:X} ({se.SocketErrorCode})");
                // See http://msdn.microsoft.com/en-us/library/windows/desktop/ms740668(v=vs.85).aspx
                //if (se.SocketErrorCode == SocketError.ConnectionReset) // WSAECONNRESET (10054)
                {
                    // Forcibly closed
                    CloseSocket(serverReplyContext);
                }
            }
            catch (Exception e) {
                SendNotification(ServiceNotification.Error, CurrentStatus, serverReplyContext, $"OnClientConnect: {e.Message}");
                CloseSocket(serverReplyContext);
            }

            // Since the main Socket is now free, it can go back and wait for
            // other clients who are attempting to connect
            _mainSocket.BeginAccept(OnClientConnect, null);
        }

        // Start waiting for data from the client
        private void BeginReceive(ServerReplyContext serverReplyContext) {
            Log4.Debug("SocketServer BeginReceive");
            try {
                serverReplyContext.Socket.BeginReceive(serverReplyContext.DataBuffer, 0,
                                    serverReplyContext.DataBuffer.Length,
                                    SocketFlags.None,
                                    OnDataReceived,
                                    serverReplyContext);
            }
            catch (SocketException se) {
                SendNotification(ServiceNotification.Error, CurrentStatus, serverReplyContext, $"BeginReceive: {se.Message}, {se.HResult:X} ({se.SocketErrorCode})");
                CloseSocket(serverReplyContext);
            }
        }

        private void CloseSocket(ServerReplyContext serverReplyContext) {
            Log4.Debug("SocketServer CloseSocket");
            if (serverReplyContext == null) return;

            // Remove the reference to the worker socket of the closed client
            // so that this object will get garbage collected
            Socket socket;
            _clientList.TryRemove(serverReplyContext.ClientNumber, out socket);
            if (socket != null) {
                Log4.Debug("Closing Socket #" + serverReplyContext.ClientNumber);
                Interlocked.Decrement(ref _clientCount);
                SendNotification(ServiceNotification.ClientDisconnected, CurrentStatus, serverReplyContext);
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        enum TelnetVerbs {
            WILL = 251,
            WONT = 252,
            DO = 253,
            DONT = 254,
            IAC = 255
        }

        enum TelnetOptions {
            SGA = 3
        }
        
        // This the call back function which will be invoked when the socket
        // detects any client writing of data on the stream
        private void OnDataReceived(IAsyncResult async) {
            var clientContext = (ServerReplyContext)async.AsyncState;
            if (_mainSocket == null || !clientContext.Socket.Connected) return;
            try {
                // Complete the BeginReceive() asynchronous call by EndReceive() method
                // which will return the number of characters written to the stream 
                // by the client
                SocketError err;
                var iRx = clientContext.Socket.EndReceive(async, out err);
                if (err != SocketError.Success || iRx == 0) {
                    CloseSocket(clientContext);
                    return;
                }

                // TODO: Shouldn't all this logic be the same between Client/Server?
                // _currentCommand contains the current command we are parsing out and 
                // _currentIndex is the index into it
                //int n = 0;
                for (int i = 0; i < iRx; i++) {
                    byte b = clientContext.DataBuffer[i];
                    switch (b) {
                        case (byte)TelnetVerbs.IAC:
                            // interpret as a command
                            i++;
                            if (i < iRx) {
                                byte verb = clientContext.DataBuffer[i];
                                switch (verb) {
                                    case (int)TelnetVerbs.IAC:
                                        //literal IAC = 255 escaped, so append char 255 to string
                                        clientContext.CmdBuilder.Append(verb);
                                        break;
                                    case (int)TelnetVerbs.DO:
                                    case (int)TelnetVerbs.DONT:
                                    case (int)TelnetVerbs.WILL:
                                    case (int)TelnetVerbs.WONT:
                                        // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                        i++;
                                        byte inputoption = clientContext.DataBuffer[i];
                                        if (i < iRx) {
                                            clientContext.Socket.Send(new[] { (byte)TelnetVerbs.IAC });
                                            if (inputoption == (int)TelnetOptions.SGA)
                                                clientContext.Socket.Send(new[]{verb == (int) TelnetVerbs.DO
                                                                        ? (byte) TelnetVerbs.WILL
                                                                        : (byte) TelnetVerbs.DO});
                                            else
                                                clientContext.Socket.Send(new[]{verb == (int) TelnetVerbs.DO
                                                                        ? (byte) TelnetVerbs.WONT
                                                                        : (byte) TelnetVerbs.DONT});
                                            clientContext.Socket.Send(new[] { inputoption });
                                        }
                                        break;
                                }
                            }
                            break;

                        case (byte)'\r':
                        case (byte)'\n':
                        case (byte)'\0':
                            // Skip any delimiter chars that might have been left from earlier input
                            if (clientContext.CmdBuilder.Length > 0) {
                                SendNotification(ServiceNotification.ReceivedLine, CurrentStatus, clientContext, clientContext.CmdBuilder.ToString());
                                // Reset n to start new command
                                clientContext.CmdBuilder.Clear();
                            }
                            break;

                        default:
                            clientContext.CmdBuilder.Append((char)b);
                            break;
                    }
                }

                // Continue the waiting for data on the Socket
                BeginReceive(clientContext);
            }
            catch (SocketException se) {
                if (se.SocketErrorCode == SocketError.ConnectionReset) // Error code for Connection reset by peer 
                {
                    SendNotification(ServiceNotification.Error, CurrentStatus, clientContext, $"OnDataReceived: {se.Message}, {se.HResult:X} ({se.SocketErrorCode})");
                    CloseSocket(clientContext);
                }
                else {
                    SendNotification(ServiceNotification.Error, CurrentStatus, clientContext, $"OnDataReceived: {se.Message}, {se.HResult:X} ({se.SocketErrorCode})");
                }
            }
        }

        public void SendAwakeCommand(String cmd, String host, int port) {
            Log4.Debug("SocketServer SendAwakeCommand");
            if (String.IsNullOrEmpty(host)) {
                SendNotification(ServiceNotification.Wakeup, CurrentStatus, null, "No wakeup host specified.");
                return;
            }
            if (port == 0) {
                SendNotification(ServiceNotification.Wakeup, CurrentStatus, null, "Invalid port.");
                return;
            }
            try {
                // Try to resolve the remote host name or address
                var resolvedHost = Dns.GetHostEntry(host);
                var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try {
                    // Create the endpoint that describes the destination
                    var destination = new IPEndPoint(resolvedHost.AddressList[0], port);

                    SendNotification(ServiceNotification.Wakeup, CurrentStatus, null,
                                     $"Attempting connection to: {destination}");
                    clientSocket.Connect(destination);
                }
                catch (SocketException err) {
                    // Connect failed so close the socket and try the next address
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    clientSocket = null;
                    SendNotification(ServiceNotification.Wakeup, CurrentStatus, null,
                                     "Error connecting.\r\n" + $"   Error: {err.Message}");
                }
                // Make sure we have a valid socket before trying to use it
                if ((clientSocket != null)) {
                    try {
                        clientSocket.Send(Encoding.ASCII.GetBytes(cmd + "\r\n"));

                        SendNotification(ServiceNotification.Wakeup, CurrentStatus, null,
                                         "Sent request " + cmd + " to wakeup host.");

                        // For TCP, shutdown sending on our side since the client won't send any more data
                        clientSocket.Shutdown(SocketShutdown.Send);
                    }
                    catch (SocketException err) {
                        SendNotification(ServiceNotification.Wakeup, CurrentStatus, null,
                                         $"Error occured while sending or receiving data.\r\n   Error: {err.Message}");
                    }
                    clientSocket.Dispose();
                }
                else {
                    SendNotification(ServiceNotification.Wakeup, CurrentStatus, null,
                                     "Unable to establish connection to server!");
                }
            }
            catch (SocketException err) {
                SendNotification(ServiceNotification.Wakeup, CurrentStatus, null,
                                 $"Socket error occured: {err.Message}");
            }
        }

        public override void Send(string text, Reply replyContext = null) {
            if (text is null) throw new ArgumentNullException(nameof(text));
            if (CurrentStatus != ServiceStatus.Connected ||
                _mainSocket == null)
                return;

            if (replyContext == null) {
                foreach (var i in _clientList.Keys) {
                    Socket client;
                    if (_clientList.TryGetValue(i, out client)) {
                        Reply reply = new ServerReplyContext(this, client, i);
                        Send(text, reply);
                    }
                }
            }
            else {
                if (((ServerReplyContext)replyContext).Socket.Send(Encoding.UTF8.GetBytes(text)) > 0) {
                    SendNotification(ServiceNotification.Write, CurrentStatus, replyContext, text.Trim());
                }
                else {
                    SendNotification(ServiceNotification.WriteFailed, CurrentStatus, replyContext, text);
                }
            }
        }

        #region Nested type: ServerReplyContext
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "none")]
        public class ServerReplyContext : Reply {
            internal StringBuilder CmdBuilder { get; set; }
            internal Socket Socket { get; set; }
            internal int ClientNumber { get; set; }


            // Buffer to store the data sent by the client
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "none")]
            public byte[] DataBuffer = new byte[1024];

            private readonly SocketServer _server;

            // Constructor which takes a Socket and a client number
            public ServerReplyContext(SocketServer server, Socket socket, int clientNumber) {
                CmdBuilder = new StringBuilder();
                _server = server;
                Socket = socket;
                ClientNumber = clientNumber;
            }

            protected string Command {
                get { return CmdBuilder.ToString(); }
                set { }
            }

            public override void Write(String text) {
                _server.Send(text, this);
            }
        }

        #endregion
    }
}
