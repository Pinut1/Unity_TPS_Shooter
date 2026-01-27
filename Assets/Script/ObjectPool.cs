using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;
    public GameObject prefab;
    public int poolSize = 20;

    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
    }

    public GameObject SpawnFromPool(Vector3 position, Quaternion rotation)
    {
        if (poolQueue.Count == 0) InitializePool();

        GameObject objectToSpawn = poolQueue.Dequeue();
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolQueue.Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}
