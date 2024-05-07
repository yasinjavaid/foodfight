using System.Collections;
using System.Collections.Generic;
using Photon.Bolt;
using UnityEngine;

public class ModifierPooler : IPrefabPool
{
    public GameObject LoadPrefab(PrefabId prefabId)
    {
        return PrefabDatabase.Find(prefabId);
    }

    public GameObject Instantiate(PrefabId prefabId, Vector3 position, Quaternion rotation)
    {
        GameObject go;

        go = GameObject.Instantiate(LoadPrefab(prefabId), position, rotation);
        go.GetComponent<BoltEntity>().enabled = true;

        return go;
    }

    public void Destroy(GameObject gameObject)
    {
        GameObject.Destroy(gameObject);
    }
}
