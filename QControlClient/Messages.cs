using System;

namespace QCommon
{

    public class Message
    {
        public string action;

        public const string ClientLogin = "client_login";
        
        public const string Heart = "heart";
        public const string CommandResult = "command_result";

        public const string LoginResult = "login_result";
        public const string RunCommand = "run_command";

        public const string RunTeamViewer = "run_teamviewer";
        public const string RunTeamViewerResult = "run_teamviewer_result";

        public const string GetTeamViewerKey = "get_teamviewer_key";

        public const string GetTeamViewerKeyResult = "get_teamviewer_key_result";

        public const string SetInfo = "set_info";
        public const string SetInfoResult = "set_info_result";
    }

    public class Result : Message
    {
        public string result;
    }

    /// <summary>
    /// Client登录服务器
    /// Client发送给Server
    /// </summary>
    public class P_ClientLogin : Message
    {
        public P_ClientLogin()
        {
            action = Message.ClientLogin;
        }

        public string machine;

        public string info;
    }

    /// <summary>
    /// Client心跳包
    /// Client发送给Server
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
    /// 发送命令结果
    /// Client发送给Server
    /// Server发送给Superuser
    /// </summary>
    public class P_CommandResult : Result
    {
        public P_CommandResult()
        {
            action = Message.CommandResult;
        }
    }

    /// <summary>
    /// 运行TeamViewer结果
    /// Client发送给Server
    /// Server发送给Superuser
    /// </summary>
    public class P_RunTeamViewerResult : Result
    {
        public P_RunTeamViewerResult()
        {
            action = Message.RunTeamViewerResult;
        }
    }

    /// <summary>
    /// 发送获取TeamViewerKey结果
    /// Client发送给Server
    /// Server发送给Superuser
    /// </summary>
    public class P_GetTeamViewerKeyResult : Result
    {
        public P_GetTeamViewerKeyResult()
        {
            action = Message.GetTeamViewerKeyResult;
        }
    }

    /// <summary>
    /// 发送设置Info的结果
    /// Client发送给Server
    /// Server发送给Superuser
    /// </summary>
    public class P_SetInfoResult : Result
    {
        public P_SetInfoResult()
        {
            action = Message.SetInfoResult;
        }

        public string machine;
    }


    /// <summary>
    /// Client接收到命令
    /// Action 
    /// 1 login_result
    /// 2 run_command
    /// </summary>
    public class P_ClientMessage : Message
    {
        public int code;
        public string msg;
    }

    //----------------------------------------------------------------



}
