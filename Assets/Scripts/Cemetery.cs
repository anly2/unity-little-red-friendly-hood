using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Cemetery : MonoBehaviour {

    public string saveFile = "cemetery.dat";
    public List<Transform> graveSlots = new List<Transform>();
    public List<GameObject> graveObjects = new List<GameObject>();

    public int currentYear = 100;

    private GraveRingBuffer graves;

    
    void Start()
    {
        Load();
        Populate();
    }

    void OnDestroy()
    {
        Save();
    }

    
    [Serializable]
    private struct Info
    {
        public int currentYear;
        public GraveRingBuffer graves;

        public Info(GraveRingBuffer graves, int currentYear)
        {
            this.currentYear = currentYear;
            this.graves = graves;
        }
    }

    void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(GetSaveFilePath(), FileMode.OpenOrCreate);

        bf.Serialize(file, new Info(graves, currentYear));

        file.Close();
    }

    void Load()
    {
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(GetSaveFilePath(), FileMode.Open);

            object data = bf.Deserialize(file);

            if (data == null || !(data is Info))
                throw new FileNotFoundException();

            file.Close();


            Info info = (Info) data;

            this.currentYear = info.currentYear;
            this.graves = info.graves;
        }
        catch (FileNotFoundException) {
            this.graves = new GraveRingBuffer(graveSlots.Count);
        }
    }

    string GetSaveFilePath()
    {
        return Application.persistentDataPath + "/" + saveFile;
    }


    GameObject[] Populate()
    {
        int l = graves.Count;
        GameObject[] result = new GameObject[l];

        int i = 0;
        foreach (Grave grave in graves.kernel)
        {
            result[i] = CreateGrave(grave, i);
            i++;
        }

        return result;
    }

    public GameObject AddGrave(string engraving)
    {
        Grave grave = new Grave(engraving);
        grave.prefabIndex = UnityEngine.Random.Range(0, graveObjects.Count);

        GameObject instance = CreateGrave(grave, graves.offset);

        Grave existing = graves.Add(grave); //returns the overwritten grave
        if (existing != null)
            Destroy(existing.gameObject);

        return instance;
    }

    GameObject CreateGrave(Grave grave, int slotIndex)
    {
        GameObject instance = Instantiate(graveObjects[grave.prefabIndex]);

        Transform slot = graveSlots[slotIndex];
        instance.transform.parent = slot;
        instance.transform.position = slot.position;

        Sign sign = instance.GetComponent<Sign>();
        sign.text = grave.engraving;

        grave.gameObject = instance;
        return instance;
    }


    [Serializable]
    private class Grave
    {
        public int prefabIndex;
        public string engraving;

        [NonSerialized]
        public GameObject gameObject = null;

        public Grave(string engraving)
        {
            this.engraving = engraving;
        }
    }


    /** VERY SPECIFIC implementation
    So DO NOT use it elsewhere!!!
    */
    [Serializable]
    private class GraveRingBuffer
    {
        public readonly List<Grave> kernel;
        public int offset = 0;

        public GraveRingBuffer(int capacity)
        {
            kernel = new List<Grave>(capacity);
        }
        
        public int Count { get { return kernel.Count; } }


        public Grave Add(Grave element)
        {
            int i = offset;
            offset = (offset + 1) % kernel.Capacity;

            if (i < kernel.Count)
            {
                Grave dropout = kernel[i];
                kernel[i] = element;
                return dropout;
            }

            kernel.Add(element);
            return null;
        }
    }
}