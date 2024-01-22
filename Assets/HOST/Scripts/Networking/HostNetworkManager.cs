using HOST.Networking;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System.Linq;
using FMETP;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HOST.Networking
{
    struct ClientDevice
    {
        public string IP;
        public bool IsReady;
        public bool IsSceneLoaded;
    }
    public class HostNetworkManager : HostNetworkRPC
    {
        public UnityEvent<string> OnClientConnection;
        public UnityEvent<string> OnClientDisconnection;
        public static HostNetworkManager instance;

        private List<ClientDevice> connectedsIP = new List<ClientDevice>();

        private void Awake()
        {
            Application.runInBackground = true;
            if (instance == null) instance = this;


        }

        public void RequestRegisterNetworkObjects()
        {
            if(IsServer())
            {
                SendRPC(new HostNetworkRPCMessage()
                {
                    InstanceId = this.InstanceId,
                    MethodName = "RegisterNetworkObjects",
                    Parameters = new object[] { },
                });
                RegisterNetworkObjects();
            }
        }
        private void RegisterNetworkObjects()
        {
            Debug.Log("RegisterNetworkObjects");
            HostNetworkObject[] networkObjects = FindObjectsByType<HostNetworkObject>(FindObjectsSortMode.InstanceID);
            FMNetworkManager.instance.NetworkObjects = new GameObject[networkObjects.Length];
            int count = 0;
            foreach (HostNetworkObject networkObject in networkObjects)
            {
                Debug.Log(networkObject.gameObject.name);
                FMNetworkManager.instance.NetworkObjects[count++] = networkObject.gameObject;
            }

            FMNetworkManager.instance.UpdateNumberOfSyncObjects();

        }

       

        public void HandleConnection(string data)
        {
            connectedsIP.Add(new ClientDevice()
            {
                IP = data,
                IsReady = false,
                IsSceneLoaded = false,
            });
            OnClientConnection?.Invoke(data);
        }


        public void HandleDisconnection(string data)
        {
            connectedsIP.Remove(connectedsIP.Find(x => x.IP == data));
            OnClientDisconnection?.Invoke(data);
        }
        public void HandleStringDataEvent(string data)
        {
            HostNetworkMessage message = JsonUtility.FromJson<HostNetworkMessage>(data);
            switch (message.MessageType)
            {
                case HostNetworkMessageType.RPC:
                    HandleRPCMessage(message);
                    break;
                case HostNetworkMessageType.ObjectSync:
                    HandleObjectSyncMessage(message);
                    break;
            }
        }

        public void HandleRPCMessage(HostNetworkMessage message)
        {
            HostNetworkRPCMessage rpcMessage = HostNetworkTools.DeserializeRPCMessage(message.Data);
            HostNetworkRPC.GetRPCInstance(rpcMessage.InstanceId).HandleRPC(rpcMessage);
        }

        public void HandleObjectSyncMessage(HostNetworkMessage message)
        {
            if (!IsServer()) return; // Client can't update network objects

            try
            {
                HostNetworkObjectTransform objectTransform = HostNetworkTools.DeserializeObjectTransform(message.Data);

                FMNetworkManager.instance.NetworkObjects.FirstOrDefault(x => x.GetComponent<HostNetworkObject>().Id == objectTransform.Id)
                    .GetComponent<HostNetworkObject>()
                    .SetTransform(objectTransform);
            }catch(Exception e)
            {
                // Sometimes sync message bug but if it miss one, it will be resync later
                Debug.LogError(e.Message);
            }
        }

        public void RequestObjectSync(HostNetworkObjectTransform objectTransform)
        {
            string data = HostNetworkTools.SerializeObjectTransform(objectTransform);
            HostNetworkMessage message = new HostNetworkMessage()
            {
                MessageType = HostNetworkMessageType.ObjectSync,
                NetworkTarget = HostNetworkTarget.Server,
                Data = data,
            };
            FMNetworkManager.instance.SendToServer(HostNetworkTools.SerializeMessage(message));
        }

        public void SendRPC(HostNetworkRPCMessage rpcMessage, string targetIP = null)
        {
            string data = HostNetworkTools.SerializeRPCMessage(rpcMessage);
            Debug.Log(data);
            HostNetworkMessage message = new HostNetworkMessage()
            {
                MessageType = HostNetworkMessageType.RPC,
                NetworkTarget = HostNetworkTarget.Server,
                Data = data,
            };

            if (targetIP != null)
            {
                FMNetworkManager.instance.SendToTarget(HostNetworkTools.SerializeMessage(message), targetIP);
                return;
            }

            if (IsServer())
            {
                FMNetworkManager.instance.SendToOthers(HostNetworkTools.SerializeMessage(message));
            }
            else
            {
                FMNetworkManager.instance.SendToServer(HostNetworkTools.SerializeMessage(message));
            }

        }

        public bool IsServer()
        {
            return FMNetworkManager.instance.NetworkType == FMNetworkType.Server;
        }


        public void LoadStreams()
        {
            foreach(ClientDevice client in connectedsIP)
            {
                MonitorManager.instance.RegisterNewStream(client.IP );
            }
        }
    }
}