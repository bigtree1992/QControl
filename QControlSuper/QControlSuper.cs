using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using WebSocketSharp;

namespace QControlServer
{
    public class QControlSuper
    {
        public Func<string> GetSpuerKey;

        public Action<int> OnConnectResult;
        public Action<int> OnLoginResult;
        public Action<ClientInfo> OnLoginClient;
        public Action<ClientInfo> OnLogoutClient;
        public Action<IList<ClientInfo>> OnGetClientList;
        public Action<string> OnSetInfoResult;
        public Action<string> OnRunTeamViewerResult;
        public Action<string> OnGetTeamViwerKeyResult;

        public Action<string> OnCommandResult;

        private WebSocket m_Socket;
        private DispatcherTimer m_Timer;

        private string m_Address;

        public QControlSuper()
        {
            m_Timer = new DispatcherTimer();
            m_Timer.Interval = new TimeSpan(0, 0, 30);
            m_Timer.Tick += OnHeartTick;
        }

        internal void Connect(string addresss)
        {
            m_Address = addresss;

            m_Socket = new WebSocket(addresss);

            m_Socket.OnOpen += Socket_OnOpen;
            m_Socket.OnMessage += Socket_OnMessage;
            m_Socket.OnError += Socket_OnError;
            m_Socket.OnClose += Socket_OnClose;

            m_Socket.Connect();
            //ToDo:处理连接失败

        }

        internal void SendLogin()
        {
            var Login = new P_SuperLogin();

            if (GetSpuerKey != null)
            {
                Login.key = GetSpuerKey();
            }
            else
            {
                Login.key = "123456";
            }

            SendMessage(Login);
        }
        internal void SendGetClientList(int start,int end)
        {
            var getclientlist = new P_GetClientList();
            getclientlist.start = start;
            getclientlist.end = end;

            if (GetSpuerKey != null)
            {
                getclientlist.key = GetSpuerKey();
            }
            else
            {
                getclientlist.key = "123456";
            }
            SendMessage(getclientlist);
        }

        internal void SendCommand(string machine,string command)
        {
            var msg = new P_Command();
            msg.command = command;
            msg.machine = machine;

            msg.key = GetSpuerKey == null ? "123456" : GetSpuerKey();
            SendMessage(msg);
        }

        internal void SendSendInfo(string machine,string info)
        {
            var msg = new P_SetInfo();
            msg.machine = machine;
            msg.info = info;
            msg.key = GetSpuerKey == null ? "123456" : GetSpuerKey();
            SendMessage(msg);
        }

        internal void SendRunTeamViewer(string machine)
        {
            var msg = new P_RunTeamViewer();
            msg.machine = machine;
            msg.key = GetSpuerKey == null ? "123456" : GetSpuerKey();
            
            SendMessage(msg);
        }

        internal void SendGetTeamViewerKey(string machine)
        {
            var msg = new P_GetTeamViewerKey();
            msg.machine = machine;
            msg.key = GetSpuerKey == null ? "123456" : GetSpuerKey();
            SendMessage(msg);
        }

        internal void CloseSocket()
        {
            try
            {
                if (m_Socket.IsAlive)
                {
                    m_Socket.Close();

                }
            }
            catch
            {

            }

        }

        private P_Heart m_Heart = new P_Heart();

        private void OnHeartTick(object sender, EventArgs e)
        {
            m_Heart.action = Message.Heart;
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            m_Heart.timestamp = Convert.ToInt64(ts.TotalSeconds);

            SendMessage(m_Heart);

            //ToDo: 心跳发送不过去的时候进行尝试重新连接
        }

        private void Socket_OnOpen(object sender, EventArgs e)
        {
            m_Timer?.Start();
            OnConnectResult?.Invoke(100);
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            m_Timer?.Stop();

            Log?.Invoke("Socket Close : " + e.Reason);

            OnConnectResult?.Invoke(101);

            Thread.Sleep(5000);

            Connect(m_Address);
        }

        private void Socket_OnError(object sender, ErrorEventArgs e)
        {
            Log?.Invoke(e.Message);
        }

        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<P_SuperUserMessage>(e.Data);

                if (message.action == Message.LoginResult)
                {
                    Log?.Invoke("Log?.Invokein Result: " + message.code);
                    OnLoginResult?.Invoke(message.code);
                }
                else if (message.action == Message.CommandResult)
                {
                    Log?.Invoke("Command Result: " + message.msg);
                    OnCommandResult?.Invoke(message.msg);
                }
                else if (message.action == Message.GetClientResult)
                {
                    OnGetClientList?.Invoke(message.clients);
                }
                else if (message.action == Message.ClientLogin)
                {
                    if (message.clients.Count > 0)
                    {
                        OnLoginClient?.Invoke(message.clients[0]);
                    }
                }
                else if (message.action == Message.ClientLogOut)
                {
                    if (message.clients.Count > 0)
                    {
                        OnLogoutClient?.Invoke(message.clients[0]);
                    }
                }
                else if (message.action == Message.SetInfoResult)
                {
                    OnSetInfoResult?.Invoke(message.msg);
                }
                else if (message.action == Message.RunTeamViewerResult)
                {
                    OnRunTeamViewerResult?.Invoke(message.msg);
                }
                else if (message.action == Message.GetTeamViewerKeyResult)
                {
                    OnGetTeamViwerKeyResult?.Invoke(message.msg);
                }
                else if (message.action == Message.Heart)
                {

                }
                else
                {
                    Log?.Invoke("UnSupported message Result: " + message.action);
                }
            }
            catch (Exception ex)
            {
                Log?.Invoke(ex.ToString());
            }
        }

        private void SendMessage(Message message)
        {
            m_Socket?.Send(JsonConvert.SerializeObject(message));
        }
       
        public Action<string> Log;
    }
}
