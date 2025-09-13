using Unity.Netcode;
using UnityEngine;

public class CountersSpawner : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnObjects;
    [SerializeField] private float spawnTimerMax = 3f;
    private float spawnTimer = 3f;

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            spawnTimer -= Time.deltaTime;
            if(spawnTimer < 0)
            {
                spawnTimer = spawnTimerMax;
                for(int i = 0; i < spawnObjects.Length; i++)
                {
                    GameObject t = Instantiate(spawnObjects[i].gameObject);
                    foreach(Transform child in t.transform)
                    {
                        child.GetComponent<NetworkObject>().Spawn();
                    }
                    break;
                }
            }
        }
    }
}
