using UnityEngine;
using Unity.Netcode;
using System.Net.Sockets;
using System.Text;

public class ReceiverTCP : NetworkBehaviour
{
    private static ReceiverTCP instance;

    private ModeManager modeManager;
    private bool isMotionControl = false;
    private bool isPilot;

    public string host = "localhost";
    public int port = 1234;

    private TcpClient client;
    private NetworkStream stream;
    private byte[] buffer = new byte[4096];

    private string newestMessage;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    private void Start()
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

            client = new TcpClient(host, port);
            stream = client.GetStream();

            Debug.Log("TCP client connected as: " + gameObject.tag);
            stream.BeginRead(buffer, 0, buffer.Length, OnReceive, null);
        }
    }

    public string GetNewestMessage()
    {
        return newestMessage;
    }

    private void OnReceive(System.IAsyncResult ar)
    {
        int bytesRead = stream.EndRead(ar);
        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        if (message != null)
        {
            newestMessage = message;
        }

        stream.BeginRead(buffer, 0, buffer.Length, OnReceive, null);
    }

    public override void OnDestroy()
    {
        if (stream != null)
        {
            stream.Close();
        }

        if (client != null)
        {
            client.Close();
        }

        base.OnDestroy();
    }
}