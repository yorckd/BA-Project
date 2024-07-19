using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ReceiverUDP : MonoBehaviour
{
    private ModeManager modeManager;
    private bool isMotionControl = false;
    private bool isPilot;

    private UdpClient udpClient;
    private Thread receiveThread;
    private string newestMessage = string.Empty;
    private object lockObject = new object();

    public int port = 1234;

    void Start()
    {
        modeManager = GameObject.FindWithTag("ModeManager").GetComponent<ModeManager>();
        isMotionControl = modeManager.IsMotionMode();
        isPilot = modeManager.IsPilot();

        if (isMotionControl)
        {
            if (isPilot)
            {
                gameObject.tag = "ReceiverSpaceship";
            }
            else if (!isPilot)
            {
                gameObject.tag = "ReceiverCrosshair";
            }
            else
            {
                return;
            }

            Debug.Log("TCP client connected as: " + gameObject.tag);

            udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
    }

    void ReceiveData()
    {
        IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
        while (true)
        {
            try
            {
                byte[] data = udpClient.Receive(ref ip);
                string message = Encoding.UTF8.GetString(data);
                lock (lockObject)
                {
                    newestMessage = message;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving UDP data: " + e.Message);
            }
        }
    }

    public string GetNewestMessage()
    {
        lock (lockObject)
        {
            return newestMessage;
        }
    }

    void OnApplicationQuit()
    {
        receiveThread.Abort();
        udpClient.Close();
    }
}