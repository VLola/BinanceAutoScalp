using BinanceAutoScalp.ConnectDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace BinanceAutoScalp.Binance
{
    public static class NewSocket
    {
        public static Socket Add(string name)
        {
            string path = System.IO.Path.Combine(Environment.CurrentDirectory, "clients");
            string json = File.ReadAllText(path + "\\" + name);
            Client client = JsonConvert.DeserializeObject<Client>(json);
            if (ConnectTrial.Check(client.ClientName))
            {
                return new Socket(client.ApiKey, client.SecretKey);
            }
            return null;
        }
    }
}
