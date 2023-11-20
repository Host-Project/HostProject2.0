using HOST.Networking;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System.Linq;
using FMETP;
using UnityEngine.Events;

namespace HOST.Networking
{
    public class HostNetworkManager : MonoBehaviour
    {
        public UnityEvent<string> OnClientConnection;
        public UnityEvent<string> OnClientDisconnection;
        public static HostNetworkManager instance;

        private void Awake()
        {
            Application.runInBackground = true;
            if (instance == null) instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            RegisterNetworkObjects();
        }



        private void RegisterNetworkObjects()
        {
            HostNetworkObject[] networkObjects = FindObjectsByType<HostNetworkObject>(FindObjectsSortMode.InstanceID);
            FMNetworkManager.instance.NetworkObjects = new GameObject[networkObjects.Length];
            int count = 0;
            foreach (HostNetworkObject networkObject in networkObjects)
            {
                FMNetworkManager.instance.NetworkObjects[count] = networkObject.gameObject;
                networkObject.Id = count++;
            }

            FMNetworkManager.instance.UpdateNumberOfSyncObjects();
        }

        public void HandleConnection(string data)
        {
            OnClientConnection?.Invoke(data);
        }


        public void HandleDisconnection(string data)
        {
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

            HostNetworkRPC.rpcInstances[rpcMessage.InstanceId].HandleRPC(rpcMessage);

        }

        public void HandleObjectSyncMessage(HostNetworkMessage message)
        {
            Debug.Log(message);
            if (!IsServer()) return; // Client can't update network objects
            HostNetworkObjectTransform objectTransform = HostNetworkTools.DeserializeObjectTransform(message.Data);

            FMNetworkManager.instance.NetworkObjects.FirstOrDefault(x => x.GetComponent<HostNetworkObject>().Id == objectTransform.Id)
                .GetComponent<HostNetworkObject>()
                .SetTransform(objectTransform);
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

        public void SendRPC(HostNetworkRPCMessage rpcMessage)
        {
            string data = HostNetworkTools.SerializeRPCMessage(rpcMessage);
            HostNetworkMessage message = new HostNetworkMessage()
            {
                MessageType = HostNetworkMessageType.RPC,
                NetworkTarget = HostNetworkTarget.Server,
                Data = data,
            };

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

    }
}