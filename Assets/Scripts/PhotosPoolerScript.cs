using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhotosPoolerScript : MonoBehaviour
{

    public static PhotosPoolerScript current;
    public GameObject pooledObjectsType;
    //public GameObject UIRoot;
    public int pooledAmount = 20;
    public bool willGrow = true;

    public List<GameObject> pooledObjects;

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        pooledObjects = new List<GameObject>();

        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject obj =
                (GameObject) Instantiate(pooledObjectsType);

            NGUITools.SetActive(obj, false);
            pooledObjects.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        foreach (GameObject pooledObject in pooledObjects)
        {
            if (!pooledObject.activeInHierarchy)
            {
                return pooledObject;
            }
        }

        if (willGrow)
        {
            GameObject obj = (GameObject) Instantiate(pooledObjectsType);
            NGUITools.SetActive(obj, false);

            pooledObjects.Add(obj);
            obj.transform.SetParent(UIRoot.list[0].transform);

            return obj;
        }

        return null;
    }
}