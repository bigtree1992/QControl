using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace QCommon
{
    public class ClientInfo
    {
        public string Info { get; set; }
    }

    public class Config
    {
        public static string ServerIP = "ws://backdoor.forestfantasy.cn:8901";
       

        public static string GetClientInfo()
        {
            try
            {
                var stream = File.Open("ClientInfo.config", FileMode.Open);

                var xs = new XmlSerializer(typeof(ClientInfo));
                var config = xs.Deserialize(stream) as ClientInfo;
            
                stream.Close();
                return config.Info;
            }
            catch
            {
                return "No Info.";
            }
            
        }

        public static void SetClientInfo(string info)
        {
            try
            {

                var stream = File.Open("ClientInfo.config", FileMode.Create);
                var xs = new XmlSerializer(typeof(ClientInfo));
                var config = new ClientInfo();
                config.Info = info;
                xs.Serialize(stream, config);
                stream.Close();
            }
            catch
            {
                
            }
        }
    }
}
