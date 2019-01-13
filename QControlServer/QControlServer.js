//-------------------------------------------------------------------------------
var config = 
{
	Host:'192.168.0.52',
	Port:3000,
	Password:'123456'
}
//-------------------------------------------------------------------------------

//-------------------------------------------------------------------------------
var log4js = require('log4js');

var log4js_config = {
     "appenders":
        [
            {
                "category":"log_date",
                "type": "datefile",
                "filename": "./logs/log",
                "alwaysIncludePattern": true,
                "pattern": "-yyyy-MM-dd-hh.log"
            }
        ],
    "replaceConsole": true,
    "levels":
    {
        "log_date":"ALL"
    }
};

log4js.configure(log4js_config);

var logger = log4js.getLogger('log_date');

//-------------------------------------------------------------------------------
var ws = require('ws').Server;
//---------------客户列表的管理---------------
var clients = 
{
    m_clients: {},
	
	AddNewClient : function(ws)
	{
		this.m_clients[ws.machine] = ws;
	},
	
	RemoveClient : function(machine)
	{
		if (machine == null || machine == undefined || machine == "null" || machine == "undefined")
		{
			logger.error('[clients] Try To Remove A Abnormal Client.');
			return;
		}

	    if (this.m_clients.hasOwnProperty(machine))
	    {
	        delete this.m_clients[machine];
	    }
	},

	FindClient : function(machine)
	{
	    return this.m_clients.hasOwnProperty(machine);
	},

	SetClient : function(ws)
	{
		if (ws.machine == null || ws.machine == undefined || ws.machine == "null" || ws.machine == "undefined")
		{
			logger.error('[clients] Try To Set A Abnormal Client.');
			return;
		}

	    this.m_clients[ws.machine] = ws;
	},

	GetClient: function (machine)
	{
	    return this.m_clients[machine];
	},

	GetClientCount : function()
	{
	    return Object.keys(this.m_clients).length;
	},

	GetClientByRange : function(start,end)
	{
		var result = [];
		
		if(end <= start)
		{
			logger.error('[clients] GetClientByRange end <= start.');
			return result;
		}
		
		var keys = Object.keys(this.m_clients);
        
		if (keys == undefined || keys == null)
		{
			logger.error('[clients] this.m_clients keys is null.');
		    return result;
		}

		if (start < 0) {
		    start = 0;
		}

		if(end > keys.length){
			end = keys.length;
		}
		
		for(var i = start;i < end; i++)
		{
		    var key = keys[i];
			if (key != null && key != undefined && key != "null" && key != "undefined")
		    {
		        var data =
			    {
			        machine: this.m_clients[key].machine,
			        info: this.m_clients[key].info
			    };

		        result.push(data);
		    }
			else{
				logger.error('[clients] Find Abnormal Key:' + key);
			}
		}
		return result;
	}

}

//-------------------------------------------------------------------------------
//---------------处理外部发送给服务器的消息---------------
var handler = 
{
	//处理客户登录
	"client_login" : function(ws,msg){
	    ws.machine = msg.machine;
	    ws.info = msg.info;

		logger.info('[handler] Client Login IP:' + ws._socket.remoteAddress + ' machine: ' + ws.machine);

		clients.SetClient(ws);		
		sender.SendClientLogin(ws,100);
	},
	//处理客户心跳
	"heart" : function(ws,msg){},
	//处理客户返回执行命令结果
	"command_result": function (ws, msg) {
		sender.SendCommandResult(msg.result);
	},
	//处理超级用户登录
	"super_login" : function(ws,msg){
		
		if(msg.key == config.Password)
		{
			superuser_ws = ws;
			sender.SendSuperLoginResult("Success.");
			logger.info('[handler] SuperUser Login IP:' +  ws._socket.remoteAddress);
		}
		else{
			sender.SendSuperLoginResult("Failed.");
			logger.info("[handler] SuperUser Login Failed! " + ws._socket.remoteAddress);
		}
	},
	//处理超级用户获取客户列表
	"get_client_list" : function(ws,msg){
		if(msg.key != config.Password)
		{
			logger.warn("[handler] get_client_list SuperUser Key Error! " + ws._socket.remoteAddress);
			return;
		}

		var list = clients.GetClientByRange(msg.start,msg.end);

		sender.SendClientListResult(list);

	},
	//处理超级用户在用户客户端执行命令
	"run_command" : function(ws,msg){
		if(msg.key != config.Password)
		{
			logger.warn("[handler] command SuperUser Key Error!" + ws._socket.remoteAddress);
			return;
		}
		
		if(clients.FindClient(msg.machine) == true)
		{
		    sender.SendCommand(clients.GetClient(msg.machine), msg.command);
		}
		else{
		    logger.error("[handler] run_command No This Machine : " + msg.machine);
		    sender.SendCommandResult("Client Not Found.");
		}
		
	},
    //设置客户端信息
	"set_info": function (ws, msg) {
	    if (msg.key != config.Password) {
	        logger.warn("[handler] command SuperUser Key Error! " + ws._socket.remoteAddress);
	        return;
	    }

	    if (clients.FindClient(msg.machine) == true) {
	        sender.SendSetInfo(clients.GetClient(msg.machine), msg.info);
	    }
	    else {
	        logger.error("[handler] set_info No This Machine : " + msg.machine);
	        sender.SendSetInfoResult('Client Not Found | ' + msg.machine);
	    }
	},
    //返回客户端信息
	"set_info_result": function (ws, msg) {
		if (clients.FindClient(msg.machine) == true) {
	        var client = clients.GetClient(msg.machine);
			client.info = msg.result;
			sender.SendSetInfoResult(msg.result);
	    }
		else{
			logger.error("[handler] set_info_result Client Not Found. " + ws._socket.remoteAddress + ' | ' + msg.machine);
		}
	    
	},
	"run_teamviewer": function (ws, msg) {
	    if (msg.key != config.Password) {
	        logger.error("[handler] command SuperUser Key Error! " + ws._socket.remoteAddress + ' | ' + msg.key);
	        return;
	    }

	    if (clients.FindClient(msg.machine) == true) {
	        sender.SendRunTeamViewer(clients.GetClient(msg.machine));
	    }
	    else {
	        logger.error("[handler] run_teamviewer No This Machine : " + msg.machine);
	        sender.SendRunTeamViewerResult('Client Not Found.');
	    }
	},
	"run_teamviewer_result": function (ws, msg) {
	    sender.SendRunTeamViewerResult(msg.result);
	},
    //设置客户端信息
	"get_teamviewer_key": function (ws, msg) {
	    if (msg.key != config.Password) {
	        console.warn("[handler] command SuperUser Key Error! " + ws._socket.remoteAddress);
	        return;
	    }

	    if (clients.FindClient(msg.machine) == true) {
	        sender.SendGetTeamViewerKey(clients.GetClient(msg.machine));
	    }
	    else {
	        logger.error("[handler] get_teamviewer_key No This Machine : " + msg.machine);
	        sender.SendGetTeamViewerKeyResult("Client Not Found.");
	    }
	},
    //返回客户端信息
	"get_teamviewer_key_result": function (ws, msg) {
	    sender.SendGetTeamViewerKeyResult(msg.result);
	}
};

//超级用户的socket
var superuser_ws = null;
//-------------------------------------------------------------------------------
//---------------处理服务器发出去的消息---------------
var sender = 
{
	//给客户发送登录结果
	SendClientLoginResult : function(ws,code)
	{
		this._SendMsgToClient(ws, "login_result", "login", code);
	},
	//给客户发送命令执行
	SendCommand:function(ws,command)
	{
	    this._SendMsgToClient(ws, "run_command", command, 100);
	},
    //发送设置用户信息的消息
	SendSetInfo:function(ws,info)
	{
	    this._SendMsgToClient(ws, "set_info", info,100);
	},
    //运行TeamViewer
	SendRunTeamViewer:function(ws)
	{
	    this._SendMsgToClient(ws, "run_teamviewer", "teamviewer", 100);
	},
    //获取TeamViewer密钥
	SendGetTeamViewerKey: function (ws)
	{
	    this._SendMsgToClient(ws, "get_teamviewer_key", "teamviewer", 100);
	},
	_SendMsgToClient(ws,action,msg,code)
	{
	    if (msg == null || msg.length <= 0) {
			logger.error("[sender] _SendMsgToClient msg is null.");
	        return;
	    }

	    var data = {
	        "action": action,
	        "code": code,
	        "msg": msg
	    }

	    try {
	        ws.send(JSON.stringify(data));
	    }
	    catch (e) {
	        logger.error("[sender] _SendMsgToClient e: " + e);
	    }
	},
	//给超级用户发送执行命令的结果
	SendCommandResult:function(result)
	{
		this._SendMsgToSuperUser("command_result",result,null,clients.GetClientCount());	
	},
	//给超级用户发送登录结果
	SendSuperLoginResult:function(info)
	{
		this._SendMsgToSuperUser("login_result",info,null,clients.GetClientCount());
	},
	//给超级用户发送客户列表
	SendClientListResult:function(list)
	{
		this._SendMsgToSuperUser("get_client_result","All",list,clients.GetClientCount());	
	},
	//给超级用户发送客户登录
	SendClientLogin:function(ws)
	{
	    if (ws == superuser_ws)
	    {
	        return;
	    }

		var data = 
		{
			machine:ws.machine,
			info:ws.info
		};

		this._SendMsgToSuperUser("client_login","add",[data],1);
	},
	//给超级用户发送客户下线
	SendClientLogout: function (ws)
	{
	    if (ws == superuser_ws) {
	        return;
	    }

		var data = 
		{
			machine:ws.machine,
			info:ws.info
		};
		this._SendMsgToSuperUser("client_logout","remove",[data],1);	
	},
    //发送设置用户信息的消息的结果
	SendSetInfoResult: function (result) {
	    this._SendMsgToSuperUser("set_info_result", result, null,0);
	},
    //运行TeamViewer的结果
	SendRunTeamViewerResult: function (result) {
	    this._SendMsgToSuperUser("run_teamviewer_result", result, null, 0);
	},
    //获取TeamViewer密钥的结果
	SendGetTeamViewerKeyResult: function (result) {
	    this._SendMsgToSuperUser("get_teamviewer_key_result", result, null, 0);
	},

	_SendMsgToSuperUser:function(action,msg,clients,value)
	{
		if(superuser_ws == null ||
		   superuser_ws.readyState != 1)
		{
			logger.error('[sender] superuser_ws state error.');
			return;
		}

		if(msg == null|| msg.length <= 0)
		{
		    logger.error('[sender] superuser_ws msg abnormal.');
			return;
		}

		var data = {
			"action":action,
			"code":100,
			"msg":msg,
			"value":value,
			"clients":clients
		}

		try{
			superuser_ws.send(JSON.stringify(data));
		}
		catch(e){
			logger.error("[Server] _SendMsgToSuperUser e: " + e);
		}
	}
};
//-------------------------------------------------------------------------------
//---------------主服务器逻辑---------------
var server = new ws({host:config.Host,port:config.Port});

server.on('connection',function(ws)
{
	logger.info('[Server] connection IP:' + ws._socket.remoteAddress  + " Port:" + ws._socket.remotePort);
  
  	//处理连接发送过来了消息
	ws.on('message',function(data)
  	{
    	try
    	{
    		var msg = JSON.parse(data);
			
			if(msg.hasOwnProperty('action') &&
			   handler.hasOwnProperty(msg.action))
			{
				handler[msg.action](ws,msg);
			}
			else
			{
				logger.error('[Server] message error: ' + ws._socket.remoteAddress);
			}
    	}
      	catch(e)
		{
			logger.error('[Server] message error: ' + e);
		}
  	});

	//处理连接关闭
  	ws.on('close',function()
	{
		if(ws == superuser_ws)
		{
			superuser_ws = null;
			logger.info('[Server] SuperUser close' + ws._socket.remoteAddress);
		}
		else {
			logger.info('[Server] Client close :' + ws._socket.remoteAddress);
		    clients.RemoveClient(ws.machine);
			sender.SendClientLogout(ws);			
		}

  	});
	
	//处理连接超时
  	ws.on('timeout',function()
	{
		if(ws == superuser_ws)
		{
			superuser_ws = null;
			logger.warn('[Server] SuperUser timeout : ' + ws._socket.remoteAddress);
		}
		else {
			logger.warn('[Server] Client timeout : ' + ws._socket.remoteAddress);
		    clients.RemoveClient(ws.machine);
			sender.SendClientLogout(ws);		
		}
				
		try
		{
			ws.close();
		}
		catch(e)
		{
			logger.error('[Server] Close Timout Socket Failed!');
		}		
  	});

});

logger.info('[Server] server running...');
console.log('[Server] server running...');