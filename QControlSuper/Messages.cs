using QCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace QControlServer
{
    public class Message
    {
        public string action;

        public const string Heart = "heart";

        public const string SuperLogin = "super_login";
        public const string LoginResult = "login_result";

        public const string ClientLogin = "client_login";
        public const string ClientLogOut = "client_logout";

        public const string GetClientList = "get_client_list";
        public const string GetClientResult = "get_client_result";

        public const string RunCommand = "run_command";
        public const string CommandResult = "command_result";

        public const string RunTeamViewer = "run_teamviewer";
        public const string RunTeamViewerResult = "run_teamviewer_result";

        public const string GetTeamViewerKey = "get_teamviewer_key";
        public const string GetTeamViewerKeyResult = "get_teamviewer_key_result";

        public const string SetInfo = "set_info";
        public const string SetInfoResult = "set_info_result";
    }

    /// <summary>
    /// SpuerUser心跳包
    /// SpuerUser发送给Server
    /// </summary>
    public class P_Heart : Message
    {
        public P_Heart()
        {
            action = Message.Heart;
        }

        public long timestamp;
    }

    /// <summary>
    /// SuperUser登录
    /// </summary>
    public class P_SuperLogin : Message
    {
        public P_SuperLogin()
        {
            action = Message.SuperLogin;
        }

        public string key;
    }

    public class P_GetClientList : Message
    {
        public P_GetClientList()
        {
            action = Message.GetClientList;
        }

        public string key;

        public int start;

        public int end;
    }

    public class Command : Message
    {
        public Command()
        {
        }

        public string key;
        public string machine;
    }

    /// <summary>
    /// SuperUser发送给Server
    /// </summary>
    public class P_Command : Command
    {
        public P_Command()
        {
            action = Message.RunCommand;
        }

        public string command;
    }

    public class P_SetInfo : Command
    {
        public P_SetInfo()
        {
            action = Message.SetInfo;
        }

        public string info;
    }

    public class P_RunTeamViewer : Command
    {
        public P_RunTeamViewer()
        {
            action = Message.RunTeamViewer;
        }
    }

    public class P_GetTeamViewerKey : Command
    {
        public P_GetTeamViewerKey()
        {
            action = Message.GetTeamViewerKey;
        }
    }

    //public class ClientInfo
    //{
    //    public string machine { get; set; }
    //    public string info { get; set; }
    //}

    public class ClientInfo
    {   
        public string machine { get; set; }

        public string info { get; set; }
        
    }

    /// <summary>
    /// SuperUser获取信息
    /// Action ：
    /// 1 登录返回
    /// 2 获取客户列表
    /// 3 客户上线
    /// 4 客户下线
    /// 5 返回执行命令结果
    /// </summary>
    public class P_SuperUserMessage : Message
    {
        public P_SuperUserMessage()
        {
            clients = new List<ClientInfo>();
        }

        public int code;

        public string msg;

        public int value;

        public IList<ClientInfo> clients;

    }
}
