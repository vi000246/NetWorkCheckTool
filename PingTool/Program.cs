using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PingTool
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("網路檢測小幫手\n");
            Console.WriteLine("====================");
            Console.WriteLine("說明:請將要檢測的IP位址或DNS放到目錄下的VpnList.txt中");
            Console.WriteLine("舉例:");
            Console.WriteLine("127.0.0.1");
            Console.WriteLine("192.168.2.1");
            Console.WriteLine("www.google.com");
            Console.WriteLine("====================\n");

            string LocalIp = GetLocalIPAddress();
            Console.WriteLine("Ping 本地IP...");
            PingIp(LocalIp);
            Console.WriteLine("\nPing 預設匣道IP...");

            var GateWay = GetDefaultGateway().ToString();
            PingIp(GateWay);
            Console.WriteLine("\n");
             
            //取得text的文字內容
            List<string> IpList = GetFileText();

            //loop ping ip
            Console.WriteLine("Ping 文字檔內的IP...");
            foreach(var ip in IpList)
                PingIp(ip);

            //檢查網路速度
            CheckSpeed();
            Console.WriteLine("網路檢查完畢 請見記錄檔");
            Console.ReadKey();
        }
         
        #region 測試下載速度
        //測試下載速度
        public  static void CheckSpeed()
        {
            try
            {
                Console.WriteLine("\n開始檢查下載速度 請稍候...\n");
                double[] speeds = new double[5];
                for (int i = 0; i < 5; i++)
                {
                    int jQueryFileSize = 261; //Size of File in KB.
                    WebClient client = new WebClient();
                    DateTime startTime = DateTime.Now;
                    client.DownloadFile("http://ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.js", "Jquery.js");
                    DateTime endTime = DateTime.Now;
                    speeds[i] = Math.Round((jQueryFileSize / (endTime - startTime).TotalSeconds));
                }
                Console.WriteLine(string.Format("你的下載速度:: {0}KB/s", speeds.Average()));
            }
            catch (Exception e) {
                Console.WriteLine("錯誤"+e.Message);
            }
        }
        #endregion
        #region 取得IpList.text裡的文字 存入陣列內
        //取得IpList.text裡的文字 存入陣列內
        public static List<string>  GetFileText(){
            var text = System.IO.File.ReadAllLines(@"IpList.txt");
            List<string> LogList = new List<string>(text);
            return LogList;
        }
        #endregion
        #region ping Ip
        public static void PingIp(string ip) {

            Regex regIp = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            PingReply tReply;
            Ping tPingControl = new Ping();

            tReply = tPingControl.Send(ip);
            if (tReply.Status != IPStatus.Success)
            {
                Console.WriteLine("此IP: " + ip.PadRight(16,' ')+"無法連接");
            }
            else 
            { 
                //第一次的回應毫秒
                long TotalTime = tReply.RoundtripTime;
                for (int i = 0; i < 5; i++)
                {
                    var result = tPingControl.Send(ip);
                    TotalTime += result.RoundtripTime;
                }

                Console.WriteLine("此IP: " + ip.PadRight(16, ' ') + "五次平均回應毫秒:" + (TotalTime/5));
            }
            tPingControl.Dispose();
            

        }
        #endregion
        #region 取得本機IP
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("無法取得Local IP!");
        }
        #endregion 
        #region 取得GateWay
        public static IPAddress GetDefaultGateway()
        {
            IPAddress result = null;
            var cards = NetworkInterface.GetAllNetworkInterfaces().ToList();
            if (cards.Any())
            {
                foreach (var card in cards)
                {
                    var props = card.GetIPProperties();
                    if (props == null)
                        continue;
                    var gateways = props.GatewayAddresses;
                    if (!gateways.Any())
                        continue;

                    var gateway =
                        gateways.FirstOrDefault(g => g.Address.AddressFamily.ToString() == "InterNetwork");
                    if (gateway == null)
                        continue;
                    result = gateway.Address;
                    break;
                };
            }
            return result;
        } 
        #endregion
    }


}
