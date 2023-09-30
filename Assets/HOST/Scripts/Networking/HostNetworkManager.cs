using HOST.Networking;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System.Linq;

namespace HOST.Networking
{


    public class HostNetworkManager : MonoBehaviour
    {

        public static HostNetworkManager instance;

        private void Awake()
        {
            Application.runInBackground = true;
            if (instance == null) instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            HostNetworkObject[] networkObjects = FindObjectsByType<HostNetworkObject>(FindObjectsSortMode.InstanceID);
            FMNetworkManager.instance.NetworkObjects = new GameObject[networkObjects.Length];
            int count = 0;
            foreach (HostNetworkObject networkObject in networkObjects)
            {
                FMNetworkManager.instance.NetworkObjects[count] = networkObject.gameObject;
                networkObject.Id = count++;

            }
            
        }

        // Update is called once per frame
        void Update()
        {

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

        }

        public void HandleObjectSyncMessage(HostNetworkMessage message)
        {
            if (FMNetworkManager.instance.NetworkType == FMNetworkType.Client) return; // Client can't update network objects
            HostNetworkObjectTransform objectTransform = JsonUtility.FromJson<HostNetworkObjectTransform>(message.Data);

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

    }
}