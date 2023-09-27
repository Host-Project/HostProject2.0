using HOST.Networking;
using UnityEngine;
using HOST.Networking.Transport;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;

namespace HOST.Networking
{


    [Serializable]
    struct SyncGameObjectData
    {
        public int Id;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    public class HostNetworkManager : MonoBehaviour, IHostNetworkListener
    {
        [SerializeField]
        private GameObject hostNetworkActorGameObject = null;
        private IHostNetworkActor hostNetworkActor = null;

        [SerializeField]
        private List<NetworkGameObject> syncGameObjects = new List<NetworkGameObject>();

        private Dictionary<int, NetworkGameObject> _syncGameObjects = new Dictionary<int, NetworkGameObject>();

        public void OnConnected()
        {
            hostNetworkActor.SendData(HostNetworkMessageType.RPC, "Test data");
            //throw new System.NotImplementedException();
        }

        public void OnDataReceived(int senderId, HostNetworkMessage data)
        {
            switch (data.MessageType)
            {
                case HostNetworkMessageType.RPC:
                    Debug.Log("RPC received: " + data.Data);
                    break;
                case HostNetworkMessageType.Sync:
                    UpdateNetworkGameObject(data.Data);
                    break;
            }
        }

        public void OnDisconnected()
        {
            //throw new System.NotImplementedException();
        }

        // Start is called before the first frame update
        void Start()
        {
            hostNetworkActor = hostNetworkActorGameObject.GetComponent<IHostNetworkActor>();
            if (hostNetworkActor == null) { throw new System.Exception("[HostNetworkManager - Start] HostNetworkActor Interface not in HostNetworkActorGameObject."); }
            hostNetworkActor.RegisterListener(this);

            foreach (NetworkGameObject syncGameObject in syncGameObjects)
            {
                syncGameObject.Id = _syncGameObjects.Count;
                _syncGameObjects.Add(syncGameObject.Id, syncGameObject);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SyncGameObject(NetworkGameObject obj)
        {
            SyncGameObjectData data = new SyncGameObjectData()
            {
                Id = obj.Id,
                Position = obj.transform.position,
                Rotation = obj.transform.rotation
            };

            data.Id = obj.Id;
            data.Position = obj.transform.localPosition;
            data.Rotation = obj.transform.rotation;


            hostNetworkActor.SendData(HostNetworkMessageType.Sync, data);
        }

        private void UpdateNetworkGameObject(string data)
        {
            Debug.Log("[HostNetworkManager - UpdateNetworkGameObject] Received data: " + data);
            SyncGameObjectData syncData = JsonUtility.FromJson<SyncGameObjectData>(data);

            if (_syncGameObjects.ContainsKey(syncData.Id))
            {
                _syncGameObjects[syncData.Id].transform.localPosition = syncData.Position;
                _syncGameObjects[syncData.Id].transform.localRotation = syncData.Rotation;
            }
        }
        private void OnDestroy()
        {
            hostNetworkActor.UnregisterListener(this);
        }
    }
}