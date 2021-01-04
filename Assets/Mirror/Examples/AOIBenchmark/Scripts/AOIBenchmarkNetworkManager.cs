using Mirror;
using UnityEngine;

public class AOIBenchmarkNetworkManager : NetworkManager
{
    public static AOIBenchmarkNetworkManager AOIInstance;
    public int Range = 1000;
    public GameObject MonsterPrefab;
    public int MonsterCount = 10000;

    public override void Awake()
    {
        if (AOIInstance)
        {
            Destroy(AOIInstance.gameObject);
        }
        AOIInstance = this;
    }

    public override void OnDestroy()
    {
        AOIInstance = null;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        for (int i = 0; i < MonsterCount; i++)
        {
            var go = Instantiate(MonsterPrefab);
            NetworkServer.Spawn(go);
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}
