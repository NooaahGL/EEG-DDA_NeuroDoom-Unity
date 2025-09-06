using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;



public class UdpMindwaveReceiver : MonoBehaviour
{
    [Serializable]
    public class EsensePacket
    {
        public int attention;
        public int meditation;
        public float signal;
        public string state;
    }

    public class HordeStartPacket
    {
        public string adaptationMode;
        public int horde;
        public long timestamp;
        public float    score;
    }

    [Header("Recepción Python → Unity")]
    public int listenPort = 5005;
    private UdpClient udpRecv;
    private IPEndPoint epRecv;


    [Header("Envío Unity → Python")]
    public string pythonIP = "127.0.0.1";
    public int sendPort = 5006;
    private UdpClient udpSend;


    // Cache de HordeManager para suscribirnos
    private HordeManager hordeManager;

    public event Action<EsensePacket> OnPacketReceived;

    void Start()
    {
        epRecv = new IPEndPoint(IPAddress.Any, listenPort);
        udpRecv = new UdpClient(epRecv);
        // Emisor
        udpSend = new UdpClient();

        hordeManager = FindObjectOfType<HordeManager>();
        if (hordeManager != null)
        {
            hordeManager.OnHordeStarted += HandleHordeStarted;

        }
    }

    void Update()
    {
        // Mientras haya paquetes pendientes, los recibimos y los logueamos
        // 1) Recibir de Python
        while (udpRecv.Available > 0)
        {
            byte[] data = udpRecv.Receive(ref epRecv);
            string json = Encoding.UTF8.GetString(data);
            try
            {
                EsensePacket pkt = JsonUtility.FromJson<EsensePacket>(json);

                OnPacketReceived?.Invoke(pkt);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"JSON parse error: {json}\n{e.Message}");
            }
        }

    }

    void OnEnable()
    {
        PlayerHealth.OnPlayerDied += sendGameOverUDP;
    }

    void OnDisable()
    {
        PlayerHealth.OnPlayerDied -= sendGameOverUDP;
    }

    public void HandleHordeStarted(int hordeNumber)
    {

        Debug.Log($"HandleHordeStarted({hordeNumber}) called at {DateTime.UtcNow:HH:mm:ss.fff}\n{Environment.StackTrace}");


        float _scoreAtHorde = ScoreManager.Instance != null ? ScoreManager.Instance.TotalScore : 0;

        // 2) Enviar a Python (a 5006)
        var outPkt = new HordeStartPacket
        {
            adaptationMode = hordeManager.config.adaptationMode.ToString(),
            horde = hordeNumber,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            score   = _scoreAtHorde
        };

        string jsonOut = JsonUtility.ToJson(outPkt);
        Debug.Log($"[UDP→Py] Enviando: {jsonOut}");

        byte[] bs = Encoding.UTF8.GetBytes(jsonOut);

        udpSend.Send(bs, bs.Length, pythonIP, sendPort);
        Debug.Log($"UDP → Python: ADD #{hordeManager.config.adaptationMode.ToString()}");
    }


    public void sendGameOverUDP()
    {
        float _scoreAtHorde = ScoreManager.Instance != null ? ScoreManager.Instance.TotalScore : 0;
        // 2) Enviar a Python (a 5006)
        var outPkt = new HordeStartPacket
        {
            adaptationMode = "GameOver",
            horde = 0,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            score   = _scoreAtHorde
        };

        string jsonOut = JsonUtility.ToJson(outPkt);
        Debug.Log($"[UDP→Py] Enviando: {jsonOut}");

        byte[] bs = Encoding.UTF8.GetBytes(jsonOut);

        udpSend.Send(bs, bs.Length, pythonIP, sendPort);
        Debug.Log($"UDP → Python: ADD #GameOver");
    }

    void OnDestroy()
    {
        // Limpiar suscripción
        if (hordeManager != null)
            hordeManager.OnHordeStarted -= HandleHordeStarted;


        udpRecv.Close();
        udpSend.Close();
    }
}
