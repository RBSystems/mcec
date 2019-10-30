﻿//-------------------------------------------------------------------
// Copyright © 2019 Kindel Systems, LLC
// http://www.kindel.com
// charlie@kindel.com
// 
// Published under the MIT License.
// Source on GitHub: https://github.com/tig/mcec  
//-------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace MCEControl {
    
    public enum ServiceNotification {
        None = 0,
        Initialized = 1,
        StatusChange,
        ReceivedLine,
        ClientConnected,
        ClientDisconnected,
        Write,
        WriteFailed,
        Error,
        Wakeup
    }

    public enum ServiceStatus {
        Started,
        Waiting,
        Connected,
        Sleeping,
        Stopped
    }

    /// <summary>
    /// The base class that each of MCE Controller's services are based
    /// on (SocketServer, SocketClient, SerialServer).
    /// 
    /// Allows core code to be able to interact with services (e.g.
    /// start, stop, configure, send replies) without having to 
    /// know what servic is active.
    /// </summary>
    public abstract class Reply {
        //public abstract String Command { get; set; }
        public abstract void Write(String text);
        public void WriteLine(String textLine) {
            Write(textLine + Environment.NewLine);
        }
    }

    public abstract class ServiceBase {
        private log4net.ILog log4;
        protected ILog Log4 { get => log4; set => log4 = value; }

        protected ServiceBase() {
            CurrentStatus  = ServiceStatus.Stopped;
            Log4 = log4net.LogManager.GetLogger("MCEControl");
        }

        public delegate
            void NotificationCallback(ServiceNotification notify, ServiceStatus status, Reply reply, String msg = "");
        public event NotificationCallback Notifications;
        
        public ServiceStatus CurrentStatus { get; set; }

        public abstract void Send(string text, Reply replyContext = null);

        // Send a status notification
        protected void SetStatus(ServiceStatus status, String msg = "") {
            CurrentStatus = status;
            SendNotification(ServiceNotification.StatusChange, status, null, msg);
        }

        protected void SendNotification(ServiceNotification notification, ServiceStatus status, Reply replyContext = null, String msg = "") {
            Notifications?.Invoke(notification, status, replyContext, msg);
        }

        // Send an error notification
        protected void Error(String msg) {
            Log4.Debug(msg);
            Notifications?.Invoke(ServiceNotification.Error, CurrentStatus, null, msg);
        }
    }
}
