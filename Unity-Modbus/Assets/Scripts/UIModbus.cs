using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class UIModbus : MonoBehaviour
{
    private Socket client;
    public Text valueTxt;
    public Text value1Txt;
    public Text value2Txt;
    public Text value3Txt;

    private void Start()
    {
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 502);
        client.Connect(iep);
    }

    public void OnClickGetMsg()
    {
        byte[] sendData = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x03, 0x00, 0x6c, 0x00, 0x03 };
        client.Send(sendData);
        RecvMsg();
    }

    private void RecvMsg()
    {
        byte[] data = new byte[1024];
        client.Receive(data);
        int length = data[5];
        print("length:" + length);
        byte[] dataShow = new byte[length + 6];
        print(dataShow.Length);
        for (int i = 0; i < length + 6; i++)
        {
            dataShow[i] = data[i];
            print(dataShow[i]);
        }
        string stringData = BitConverter.ToString(dataShow);
        valueTxt.text = stringData;
        string[] strArray = stringData.Split('-');
        print(strArray.Length);
        value1Txt.text = "0x" + strArray[9] + strArray[10];
        value2Txt.text = "0x" + strArray[11] + strArray[12];
        value3Txt.text = "0x" + strArray[13] + strArray[14];
        print(dataShow.Length);
        //if (data[7] == 0x01) { print(stringData); }
        //if (data[7] == 0x02) { print(stringData); }
        //if (data[7] == 0x03) { print(stringData); }
        //if (data[7] == 0x05) { print(stringData); }
        //if (data[7] == 0x06) { print(stringData); }
        //if (data[7] == 0x0F) { print(stringData); }
        //if (data[7] == 0x10) { print(stringData); }
    }
}