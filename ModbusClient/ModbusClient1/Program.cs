using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ModbusClient1
{
    internal class Program
    {
        public static TcpClient tcpClient;
        public static Socket client;

        private static void Main(string[] args)
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 502);
            client.Connect(iep);
            byte[] sendData1 = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01 };
            //0  15  0  0  0  4  1  1  1  0  0  0  0  0  0  0  0  0  0  0
            byte[] sendData2 = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x03, 0x00, 0x6c, 0x00, 0x03 };
            client.Send(sendData2);
            ReceiveMsg();
            Console.ReadKey();
        }

        #region HelperFunction

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="rData">结果</param>
        /// <param name="id">设备号</param>
        /// <param name="address">设备地址</param>
        /// <param name="len">长度-多少个设备</param>
        /// <returns>数据读取结果 是否成功</returns>
        public static bool ReceiveData(ref short[] rData, short id, short address, short len)
        {
            try
            {
                short m = Convert.ToInt16(new Random().Next(2, 20));
                rData = null;

                byte[] bs = Receive(m, id, address, len);
                byte[] b = TrimModbus(bs, m, id, len);

                if (b == null) { return false; }

                List<short> data = new List<short>(255);
                for (int i = 0; i < b.Length - 1; i++)
                {
                    if (!Convert.ToBoolean(i & 1))
                    {
                        byte[] temp = new byte[] { b[i + 1], b[i] };
                        data.Add(BitConverter.ToInt16(temp, 0));
                    }
                }
                rData = data.ToArray();

                return true;
            }
            catch (Exception e)
            {
                //LogHelper.WriteLog("返回Modbus数据错误" + e.Message);
                return false;
            }
        }

        public static bool Open(string ip, int port)
        {
            try
            {
                tcpClient = new TcpClient();

                tcpClient.Connect(IPAddress.Parse(ip), port);
                Console.WriteLine("成功连接服务器！");
                return true;
            }
            catch (SocketException e)
            {
                string m = string.Format("modbus Client服务器连接错误:{0},ip:{1},port:{2}", e.Message, ip, port);
                //LogHelper.WriteLog(m);
                return false;
            }
        }

        /// <summary>
        /// 读取Modbus
        /// </summary>
        /// <param name="m">表示</param>
        /// <param name="id">设备码</param>
        /// <param name="address">开始地址</param>
        /// <param name="len">设备数量</param>
        /// <returns></returns>
        private static byte[] Receive(short m, short id, short address, short len)
        {
            try
            {
                if (tcpClient == null || !tcpClient.Connected) { return null; }

                byte[] data = GetSrcData(m, id, address, len);

                //00 00 00 00 00 06 01 03 00 00 00 05
                tcpClient.Client.Send(data, data.Length, SocketFlags.None);

                int size = len * 2 + 9;

                byte[] rData = new byte[size];

                tcpClient.Client.Receive(rData, size, SocketFlags.None);

                //string t1 = TranBytes(rData);

                return rData;
            }
            catch (SocketException e)
            {
                if (e.ErrorCode != 10004)
                {
                    //LogHelper.WriteLog(e.Message);
                }

                if (tcpClient != null)
                {
                    tcpClient.Close();
                    tcpClient = null;
                }

                return null;
            }
        }

        /// <summary>
        /// 发送字节数
        /// </summary>
        /// <param name="m"></param>
        /// <param name="len"></param>
        /// <param name="id"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        private static byte[] GetSrcData(short m, short id, short add, short len)
        {
            List<byte> data = new List<byte>(255);

            data.AddRange(ValueHelper.Instance.GetBytes(m));                     //             00 01
            data.AddRange(new byte[] { 0x00, 0x00 });                            //             00 00
            data.AddRange(ValueHelper.Instance.GetBytes(Convert.ToInt16(6)));    //字节数       00 06
            data.Add(Convert.ToByte(id));                                        //路由码       01
            data.Add(Convert.ToByte(3));                                         //功能码 3-读  03
            data.AddRange(ValueHelper.Instance.GetBytes(add));                   //开始地址     00 00
            data.AddRange(ValueHelper.Instance.GetBytes(len));                   //设备数量     00 05
            return data.ToArray();
        }

        private static byte[] TrimModbus(byte[] d, short m, short id, short len)
        {
            int size = Convert.ToInt32(len) * 2;
            int dLen = size + 9;

            if (d == null || d.Length != dLen || m != Convert.ToInt16(d[1]) || id != Convert.ToInt16(d[6]))
            {
                return null;
            }
            byte[] n = new byte[size];
            Array.Copy(d, 9, n, 0, size);
            return n;
        }

        private static void ReceiveMsg()
        {
            byte[] data = new byte[1024];
            client.Receive(data);
            int length = data[5];
            Console.WriteLine("length:" + length);
            byte[] dataShow = new byte[length + 6];
            //for (int i = 0; i < 20; i++)
            //{
            //    Console.Write(data[i] + "  ");
            //}
            for (int i = 0; i < length + 6; i++)
            {
                dataShow[i] = data[i];
                //Console.WriteLine(dataShow[i]);
            }
            string stringData = BitConverter.ToString(dataShow);
            //Console.WriteLine(stringData);
            if (data[7] == 0x01) { Console.WriteLine(stringData); }
            if (data[7] == 0x02) { Console.WriteLine(stringData); }
            if (data[7] == 0x03) { Console.WriteLine(stringData); }
            if (data[7] == 0x05) { Console.WriteLine(stringData); }
            if (data[7] == 0x06) { Console.WriteLine(stringData); }
            if (data[7] == 0x0F) { Console.WriteLine(stringData); }
            if (data[7] == 0x10) { Console.WriteLine(stringData); }
        }

        #endregion HelperFunction
    }
}