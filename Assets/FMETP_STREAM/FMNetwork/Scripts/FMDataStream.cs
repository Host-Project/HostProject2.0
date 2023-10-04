using System.Collections;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Threading;

/*
 * + StereoPi Commands(Example):
 * Connect via ssh:
 * ssh root@192.168.xx.xx
 * Pwd: root
 * 
 * Stop Default Stream:
 * /opt/StereoPi/stop.sh
 * 
 * Sending stream from Raspberry:
 * 
 * For UDP:
 * raspivid -t 0 -w 1280 -h 720 -fps 30 -3d sbs -cd MJPEG -o - | nc 192.168.1.10 3001 -u
 *
 * For TCP:
 * raspivid -t 0 -w 1280 -h 720 -fps 30 -3d sbs -cd MJPEG -o - | nc 192.168.1.10 3001
 * 
 * where 192.168.1.10 3001 - IP and port
*/

/*
 * + GStreamer Commands(Example):
 * + Desktop Capture to Unity
 * gst-launch-1.0 gdiscreencapsrc ! queue ! video/x-raw,framerate=60/1,width=1920, height=1080 ! jpegenc ! rndbuffersize max=65000 ! udpsink host=192.168.1.10 port=3001
 * 
 * + Video Stream to Unity
 * gst-launch-1.0 filesrc location="videopath.mp4" ! queue ! decodebin ! videoconvert ! jpegenc ! rndbuffersize max=65000 ! udpsink host=192.168.1.10 port=3001
 */

namespace FMETP
{
    public class FMDataStream
    {
        public class FMDataStreamComponent : MonoBehaviour
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            private int udpSendBufferSize = 1024 * 65; //max 65535
            private int udpReceiveBufferSize = 1024 * 1024 * 4; //max 2147483647
#else
        private int udpSendBufferSize = 1024 * 60; //max 65535
        private int udpReceiveBufferSize = 1024 * 512; //max 2147483647
#endif

            [HideInInspector] public FMNetworkManager Manager;

            public FMDataStreamType DataStreamType = FMDataStreamType.Receiver;
            public FMProtocol Protocol = FMProtocol.UDP;

            public void BroadcastChecker()
            {
                UdpClient BroadcastClient = new UdpClient();
                try
                {
                    BroadcastClient.Client.SendTimeout = 200;
                    BroadcastClient.EnableBroadcast = true;

                    byte[] _byte = new byte[1];
                    BroadcastClient.Send(_byte, _byte.Length, new IPEndPoint(IPAddress.Parse(Manager.ReadBroadcastAddress), ClientListenPort));

                    if (BroadcastClient != null) BroadcastClient.Close();
                }
                catch
                {
                    if (BroadcastClient != null) BroadcastClient.Close();
                }
            }

            //Sender props..
            public string ClientIP { get { return Manager.DataStreamSettings.ClientIP; } }
            public List<string> ClientIPList { get { return Manager.DataStreamSettings.ClientIPList; } }
            public UdpClient udpClient_Sender;
            private ConcurrentQueue<byte[]> _appendSendBytes = new ConcurrentQueue<byte[]>();
            public bool UseMainThreadSender = false;

            public void Action_AddBytes(byte[] inputBytes)
            {
                _appendSendBytes.Enqueue(inputBytes);
            }
            IEnumerator NetworkClientStartUDPSenderCOR()
            {
                stop = false;

                BroadcastChecker();
                yield return null;

                if (UseMainThreadSender)
                {
                    StartCoroutine(MainThreadSenderCOR());
                }
                else
                {
                    //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv Client Sender vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
                    while (Loom.numThreads >= Loom.maxThreads) yield return null;
                    Loom.RunAsync(() =>
                    {
                        //client request
                        while (!stop)
                        {
                            Sender();
                            System.Threading.Thread.Sleep(1);
                        }
                        System.Threading.Thread.Sleep(1);
                    });
                    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Client Sender ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                }
            }

            IEnumerator MainThreadSenderCOR()
            {
                //client request
                while (!stop)
                {
                    yield return null;
                    Sender();
                }
            }

            private void Sender()
            {
                try
                {
                    if (udpClient_Sender == null)
                    {
                        udpClient_Sender = new UdpClient();
                        udpClient_Sender.Client.SendBufferSize = udpSendBufferSize;
                        udpClient_Sender.Client.ReceiveBufferSize = udpReceiveBufferSize;
                        udpClient_Sender.Client.SendTimeout = 500;
                        udpClient_Sender.EnableBroadcast = true;
                        udpClient_Sender.MulticastLoopback = UDPTransferType == FMUDPTransferType.Multicast;
                    }

                    //send to server ip only
                    if (_appendSendBytes.Count > 0)
                    {
                        //limit 30 packet sent in each frame, solved overhead issue on receiver
                        int sendCount = 0;
                        while (_appendSendBytes.Count > 0 && sendCount < 100)
                        {
                            sendCount++;
                            if (_appendSendBytes.TryDequeue(out byte[] _bytes))
                            {
                                if (UDPTransferType == FMUDPTransferType.Broadcast)
                                {
                                    udpClient_Sender.Send(_bytes, _bytes.Length, new IPEndPoint(IPAddress.Parse(Manager.ReadBroadcastAddress), ClientListenPort));
                                }
                                else
                                {
                                    if (UDPTransferType == FMUDPTransferType.MultipleUnicast)
                                    {
                                        if (ClientIPList.Count > 0)
                                        {
                                            for (int i = 0; i < ClientIPList.Count; i++)
                                            {
                                                udpClient_Sender.Send(_bytes, _bytes.Length, new IPEndPoint(IPAddress.Parse(ClientIPList[i]), ClientListenPort));
                                            }
                                        }
                                    }
                                    else if (UDPTransferType == FMUDPTransferType.Unicast)
                                    {
                                        udpClient_Sender.Send(_bytes, _bytes.Length, new IPEndPoint(IPAddress.Parse(ClientIP), ClientListenPort));
                                    }
                                    else if (UDPTransferType == FMUDPTransferType.Multicast)
                                    {
                                        udpClient_Sender.Send(_bytes, _bytes.Length, new IPEndPoint(IPAddress.Parse(MulticastAddress), ClientListenPort));
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    //DebugLog("client sender timeout: " + socketException.ToString());
                    if (udpClient_Sender != null) udpClient_Sender.Close(); udpClient_Sender = null;
                }
            }

            public int ClientListenPort = 3001;

            public FMUDPTransferType UDPTransferType = FMUDPTransferType.Unicast;
            public FMTCPSocketType TCPSocketType = FMTCPSocketType.TCPServer;
            public string MulticastAddress = "239.255.255.255";

            private UdpClient udpClient_Listener;
            private IPEndPoint ServerEp;

            private TcpListener tcpServer_Listener;
            private List<TcpClient> tcpServer_Clients = new List<TcpClient>();
            private List<NetworkStream> tcpServer_Streams = new List<NetworkStream>();
            private bool tcpServerCreated = false;
            public bool IsConnected = false;

            private int EnvironmentTickCountDelta(int currentMS, int lastMS)
            {
                int gap = 0;
                if (currentMS < 0 && lastMS > 0)
                {
                    gap = Mathf.Abs(currentMS - int.MinValue) + (int.MaxValue - lastMS);
                }
                else
                {
                    gap = currentMS - lastMS;
                }
                return gap;
            }

            private long _currentSeenTimeMS = 0;
            public int CurrentSeenTimeMS
            {
                get { return Convert.ToInt32(Interlocked.Read(ref _currentSeenTimeMS)); }
                set { Interlocked.Exchange(ref _currentSeenTimeMS, (long)value); }
            }
            private long _lastReceivedTimeMS = 0;
            public int LastReceivedTimeMS
            {
                get { return Convert.ToInt32(Interlocked.Read(ref _lastReceivedTimeMS)); }
                set { Interlocked.Exchange(ref _lastReceivedTimeMS, (long)value); }
            }
            private long _lastSentTimeMS = 0;
            public int LastSentTimeMS
            {
                get { return (int)Interlocked.Read(ref _lastSentTimeMS); }
                set { Interlocked.Exchange(ref _lastSentTimeMS, (long)value); }
            }

            private long _stop = 0;
            private bool stop
            {
                get { return Interlocked.Read(ref _stop) == 1; }
                set { Interlocked.Exchange(ref _stop, Convert.ToInt64(value)); }
            }

            private ConcurrentQueue<byte[]> _appendQueueReceivedBytes = new ConcurrentQueue<byte[]>();

            private int ReceivedCount = 0;

            #region TCP_Server_Listener
            private IEnumerator NetworkServerStartTCPListenerCOR()
            {
                if (!tcpServerCreated)
                {
                    tcpServerCreated = true;

                    // create tcpServer_Listener
                    tcpServer_Listener = new TcpListener(IPAddress.Any, ClientListenPort);
                    tcpServer_Listener.Start();
                    tcpServer_Listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                    // create LOOM thread, only create on first time, otherwise we will crash max thread limit
                    // Wait for client to connect in another Thread 
                    Loom.RunAsync(() =>
                    {
                        while (!stop)
                        {
                            // Wait for client connection
                            tcpServer_Clients.Add(tcpServer_Listener.AcceptTcpClient());
                            tcpServer_Clients[tcpServer_Clients.Count - 1].NoDelay = true;
                            //IsConnected = true;

                            tcpServer_Streams.Add(tcpServer_Clients[tcpServer_Clients.Count - 1].GetStream());
                            tcpServer_Streams[tcpServer_Streams.Count - 1].WriteTimeout = 500;

                            Loom.QueueOnMainThread(() =>
                            {
                                //IsConnected = true;
                                if (tcpServer_Clients != null)
                                {
                                    if (tcpServer_Clients.Count > 0) StartCoroutine(TCPServer_ReceiverCOR(tcpServer_Clients[tcpServer_Clients.Count - 1], tcpServer_Streams[tcpServer_Streams.Count - 1]));
                                }

                            });
                            System.Threading.Thread.Sleep(1);
                        }
                    });

                    while (!stop)
                    {
                        ReceivedCount = _appendQueueReceivedBytes.Count;
                        while (_appendQueueReceivedBytes.Count > 0)
                        {
                            if (_appendQueueReceivedBytes.TryDequeue(out byte[] receivedBytes))
                            {
                                Manager.OnReceivedByteDataEvent.Invoke(receivedBytes);
                            }
                        }
                        yield return null;
                    }
                }
                yield break;
            }

            private IEnumerator TCPServer_ReceiverCOR(TcpClient _client, NetworkStream _stream)
            {
                bool _break = false;
                _stream.ReadTimeout = 1000;

                Loom.RunAsync(() =>
                {
                    while (!_client.Connected) System.Threading.Thread.Sleep(1);
                    while (!stop && !_break)
                    {
                        _stream.Flush();
                        byte[] bytes = new byte[300000];

                        // Loop to receive all the data sent by the client.
                        int _length;
                        while ((_length = _stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            if (_length > 0)
                            {
                                byte[] _s = new byte[_length];
                                Buffer.BlockCopy(bytes, 0, _s, 0, _length);
                                _appendQueueReceivedBytes.Enqueue(_s);
                                LastReceivedTimeMS = Environment.TickCount;
                            }
                            System.Threading.Thread.Sleep(1);
                        }

                        if (_length == 0)
                        {
                            if (_stream != null)
                            {
                                try { _stream.Close(); }
                                catch (Exception e) { DebugLog(e.Message); }
                            }

                            if (_client != null)
                            {
                                try { _client.Close(); }
                                catch (Exception e) { DebugLog(e.Message); }
                            }

                            for (int i = 0; i < tcpServer_Clients.Count; i++)
                            {
                                if (_client == tcpServer_Clients[i])
                                {
                                    tcpServer_Streams.Remove(tcpServer_Streams[i]);
                                    tcpServer_Clients.Remove(tcpServer_Clients[i]);
                                }
                            }
                            _break = true;
                        }
                    }
                    System.Threading.Thread.Sleep(1);
                });

                while (!stop && !_break) yield return null;
                yield break;
            }
            #endregion

            #region TCP_Client_Receiver
            private bool tcpClientCreated = false;
            private TcpClient udpClient_Receiver;
            private NetworkStream udpClient_Stream;
            private IEnumerator NetworkClientStartTCPReceiverCOR()
            {
                if (!tcpClientCreated)
                {
                    tcpClientCreated = true;

                    //Connect to TCP Server from another Thread
                    Loom.RunAsync(() =>
                    {
                        TCPClient_ConnectAndReceive();
                    });

                    while (!stop)
                    {
                        ReceivedCount = _appendQueueReceivedBytes.Count;
                        while (_appendQueueReceivedBytes.Count > 0)
                        {
                            if (_appendQueueReceivedBytes.TryDequeue(out byte[] receivedBytes))
                            {
                                Manager.OnReceivedByteDataEvent.Invoke(receivedBytes);
                            }
                        }
                        yield return null;
                    }
                }
                yield break;
            }

            private void TCPClient_ConnectAndReceive()
            {
                udpClient_Receiver = new TcpClient();
                IAsyncResult result = udpClient_Receiver.BeginConnect(IPAddress.Parse(ClientIP), ClientListenPort, null, null);
                result.AsyncWaitHandle.WaitOne(1000, true);
                if (!udpClient_Receiver.Connected)
                {
                    //try reconnect
                    udpClient_Receiver.Close();
                    TCPClient_ConnectAndReceive();
                }
                else
                {
                    udpClient_Stream = udpClient_Receiver.GetStream();
                    while (!stop)
                    {
                        //readTCPDataByteArray
                        byte[] bytes = new byte[300000];
                        int _length;
                        while ((_length = udpClient_Stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            if (_length > 0)
                            {
                                byte[] _s = new byte[_length];
                                Buffer.BlockCopy(bytes, 0, _s, 0, _length);
                                _appendQueueReceivedBytes.Enqueue(_s);
                                LastReceivedTimeMS = Environment.TickCount;
                            }
                            System.Threading.Thread.Sleep(1);
                        }
                    }
                }
            }
            #endregion

            #region UDP
            IEnumerator NetworkClientStartUDPListenerCOR()
            {
                LastReceivedTimeMS = Environment.TickCount;

                stop = false;
                yield return new WaitForSeconds(0.5f);

                BroadcastChecker();
                yield return new WaitForSeconds(0.5f);

                //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv Client Receiver vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
                while (Loom.numThreads >= Loom.maxThreads) yield return null;
                Loom.RunAsync(() =>
                {
                    while (!stop)
                    {
                        try
                        {
                            if (udpClient_Listener == null)
                            {
                                udpClient_Listener = new UdpClient(ClientListenPort, AddressFamily.InterNetwork);
                                udpClient_Listener.Client.ReceiveTimeout = 2000;

                                switch (UDPTransferType)
                                {
                                    case FMUDPTransferType.Unicast: break;
                                    case FMUDPTransferType.MultipleUnicast: break;
                                    case FMUDPTransferType.Multicast:
                                        udpClient_Listener.MulticastLoopback = true;
                                        udpClient_Listener.JoinMulticastGroup(IPAddress.Parse(MulticastAddress));
                                        break;
                                    case FMUDPTransferType.Broadcast:
                                        udpClient_Listener.EnableBroadcast = true;
                                        break;
                                }

                                ServerEp = new IPEndPoint(IPAddress.Any, ClientListenPort);
                            }

                            byte[] ReceivedData = udpClient_Listener.Receive(ref ServerEp);
                            LastReceivedTimeMS = Environment.TickCount;

                            //=======================Decode Data=======================
                            _appendQueueReceivedBytes.Enqueue(ReceivedData);
                        }
                        catch
                        {
                            if (udpClient_Listener != null) udpClient_Listener.Close(); udpClient_Listener = null;
                        }
                        //System.Threading.Thread.Sleep(1);
                    }
                    System.Threading.Thread.Sleep(1);
                });
                //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Client Receiver ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

                while (!stop)
                {
                    ReceivedCount = _appendQueueReceivedBytes.Count;
                    while (_appendQueueReceivedBytes.Count > 0)
                    {
                        if (_appendQueueReceivedBytes.TryDequeue(out byte[] receivedBytes))
                        {
                            Manager.OnReceivedByteDataEvent.Invoke(receivedBytes);
                        }
                    }
                    yield return null;
                }
                yield break;
            }
            #endregion

            public void Action_StartClient() { StartAll(); }
            public void Action_StopClient() { StopAll(); }

            private long _initialised = 0;
            private bool initialised
            {
                get { return Interlocked.Read(ref _initialised) == 1; }
                set { Interlocked.Exchange(ref _initialised, Convert.ToInt64(value)); }
            }

            private IEnumerator StartAllDelayCOR(float _delay = 1f)
            {
                yield return new WaitForSecondsRealtime(_delay);
                yield return null;

                StartAll();
            }
            private void StartAll(float _delay = 0f)
            {
                if (_delay > 0f)
                {
                    StartCoroutine(StartAllDelayCOR(1f));
                    return;
                }

                if (initialised) return;
                initialised = true;

                stop = false;

                if (DataStreamType == FMDataStreamType.Receiver)
                {
                    switch (Protocol)
                    {
                        case FMProtocol.UDP: StartCoroutine(NetworkClientStartUDPListenerCOR()); break;
                        case FMProtocol.TCP:
                            if (TCPSocketType == FMTCPSocketType.TCPServer)
                            {
                                StartCoroutine(NetworkServerStartTCPListenerCOR());
                            }
                            else if (TCPSocketType == FMTCPSocketType.TCPClient)
                            {
                                StartCoroutine(NetworkClientStartTCPReceiverCOR());
                            }
                            break;
                    }
                }
                else
                {
                    StartCoroutine(NetworkClientStartUDPSenderCOR());
                }
            }

            private void StopAll()
            {
                initialised = false;

                stop = true;

                if (DataStreamType == FMDataStreamType.Receiver)
                {
                    switch (Protocol)
                    {
                        case FMProtocol.UDP:
                            StopAllCoroutines();
                            if (udpClient_Listener != null)
                            {
                                try { udpClient_Listener.Close(); }
                                catch (Exception e) { DebugLog(e.Message); }
                                udpClient_Listener = null;
                            }
                            break;
                        case FMProtocol.TCP:
                            if (TCPSocketType == FMTCPSocketType.TCPServer)
                            {
                                tcpServerCreated = false; //just in case, for reset
                                foreach (TcpClient client in tcpServer_Clients)
                                {
                                    if (client != null)
                                    {
                                        try { client.Close(); }
                                        catch (Exception e) { DebugLog(e.Message); }
                                    }
                                    IsConnected = false;
                                }
                            }
                            else if (TCPSocketType == FMTCPSocketType.TCPClient)
                            {
                                tcpClientCreated = false; //just in case, for reset
                                if (udpClient_Receiver != null)
                                {
                                    if (udpClient_Stream != null)
                                    {
                                        try { udpClient_Stream.Close(); ; }
                                        catch (Exception e) { DebugLog(e.Message); }
                                    }
                                    if (udpClient_Receiver != null)
                                    {
                                        try { udpClient_Receiver.Close(); ; }
                                        catch (Exception e) { DebugLog(e.Message); }
                                    }
                                }
                                IsConnected = false;
                            }
                            break;
                    }
                }
                else
                {
                    StopAllCoroutines();
                    _appendSendBytes = new ConcurrentQueue<byte[]>();
                }
            }

            // Start is called before the first frame update
            private void Start()
            {
                Application.runInBackground = true;
                StartAll();
            }

            private void Update()
            {
                CurrentSeenTimeMS = Environment.TickCount;
                IsConnected = EnvironmentTickCountDelta(CurrentSeenTimeMS, LastReceivedTimeMS) < 3000;
            }

            public bool ShowLog = true;
            public void DebugLog(string _value) { if (ShowLog) Debug.Log(_value); }

            private void OnApplicationQuit() { StopAll(); }
            private void OnDisable() { StopAll(); }
            private void OnDestroy() { StopAll(); }
            private void OnEnable() { StartAll(1f); } //this may cause error on android

            private bool isPaused = false;
            private bool isPaused_old = false;
            private long _needResetFromPaused = 0;
            private bool needResetFromPaused
            {
                get { return Interlocked.Read(ref _needResetFromPaused) == 1; }
                set { Interlocked.Exchange(ref _needResetFromPaused, Convert.ToInt64(value)); }
            }

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID || WINDOWS_UWP)
            //private void OnApplicationPause(bool pause) { if (pause) StopAll(); }
            //private void OnApplicationFocus(bool focus) { if (focus) StartAll(1f); }

            //try fixing Android/Mobile connection issue after a pause...
            //some devices will trigger OnApplicationPause only, when some devices will trigger both...etc
            private void ResetFromPause()
            {
                if (!needResetFromPaused) return;
                needResetFromPaused = false;

                StopAll();
                StartAll(1f);
            }
            private void OnApplicationPause(bool pause)
            {
                if (!initialised) return; //ignore it if not initialised yet

                isPaused_old = isPaused;
                isPaused = pause;
                if (isPaused && !isPaused_old) needResetFromPaused = true;
                if (!isPaused && isPaused_old) ResetFromPause();
            }
            private void OnApplicationFocus(bool focus)
            {
                if (!initialised) return; //ignore it if not initialised yet

                isPaused_old = isPaused;
                isPaused = !focus;
                if (isPaused && !isPaused_old) needResetFromPaused = true;
                if (!isPaused && isPaused_old) ResetFromPause();
            }
#endif
        }
    }
}