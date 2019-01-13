using Microsoft.Win32;
using Newtonsoft.Json;
using QCommon;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using WebSocketSharp;

namespace QCommon
{
    public class QControlClient
    {
        private WebSocket m_Socket;
        private System.Timers.Timer m_Timer;

        private string m_Address;

        public QControlClient()
        {
            m_Timer = new System.Timers.Timer(40000);
            m_Timer.AutoReset = true;
            m_Timer.Elapsed += OnTimerElapsed;

        }

        private P_Heart m_Heart = new P_Heart();

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            m_Heart.action = Message.Heart;
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            m_Heart.timestamp = Convert.ToInt64(ts.TotalSeconds);

            SendMessage(m_Heart);

            Log?.Invoke("OnHeartTick : " + m_Heart.timestamp);
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

            Log?.Invoke("Start Connect.. " + addresss);
        }

        internal void Close()
        {
            CloseSocket();

            m_Timer.Stop();

            Log?.Invoke("Close.");
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



        private void Socket_OnOpen(object sender, EventArgs e)
        {
            m_Timer?.Start();

            var login = new P_ClientLogin();
            login.machine = MachineCode.GetMachineCode();
            login.info = Config.GetClientInfo();

            SendMessage(login);

            Log?.Invoke("Socket_OnOpen");
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            m_Timer?.Stop();

            Log?.Invoke("Socket Close : " + e.Reason);
            Thread.Sleep(60000);

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
                var message = JsonConvert.DeserializeObject<P_ClientMessage>(e.Data);
                if (message.action == Message.LoginResult)
                {
                    Log("Login Result: " + message.code);
                }
                else if (message.action == Message.RunCommand)
                {
                    var result = Utils.RunScript(message.msg);
                    var result_msg = new P_CommandResult();
                    result_msg.result = result;
                    SendMessage(result_msg);
                }
                else if (message.action == Message.RunTeamViewer)
                {
                    var result_msg = new P_RunTeamViewerResult();
                    result_msg.result = Utils.RunTeamViewer() ? "Success" : "Failed";
                    SendMessage(result_msg);

                    Log("RunTeamViewer Result: " + result_msg.result);
                }
                else if (message.action == Message.GetTeamViewerKey)
                {
                    var result_msg = new P_GetTeamViewerKeyResult();
                    result_msg.result = Utils.GetTeamViewerKey();
                    SendMessage(result_msg);
                }
                else if (message.action == Message.SetInfo)
                {
                    if (!string.IsNullOrEmpty(message.msg))
                    {
                        Config.SetClientInfo(message.msg);
                        string info = Config.GetClientInfo();

                        var result_msg = new P_SetInfoResult();
                        result_msg.machine = MachineCode.GetMachineCode();
                        result_msg.result = info;
                        SendMessage(result_msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private void SendMessage(Message message)
        {
            m_Socket?.Send(JsonConvert.SerializeObject(message));
        }

        public Action<string> Log;
    }
}
