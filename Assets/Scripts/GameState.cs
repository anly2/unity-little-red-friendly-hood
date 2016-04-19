using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Runtime.Serialization;

public class GameState : MonoBehaviour {

    private static Dictionary<string, Data> wholeState = new Dictionary<string, Data>();

    public string stateFilePath = "worldState.dat";

    [Tooltip("Whether to call Load() at Awake, and Save() at OnDestroy")]
    public bool autoact = false;

    private Dictionary<string, Data> state;
    
	void Awake() {
        if (autoact)
            Load();
	}

    void OnDestroy()
    {
        if (autoact)
            Save();
    }


    public void Load()
    {
        Load(GetStateFilePath());
    }

    public void Save()
    {
        Save(GetStateFilePath());
    }    

    public string GetStateFilePath()
    {
        return Application.persistentDataPath + "/" + stateFilePath;
    }
    

    public void Load(string stateFile)
    {
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(stateFile, FileMode.Open);

            object state = bf.Deserialize(file);

            if (state != null)
                this.state = state as Dictionary<string, Data>;

            StartCoroutine(new WaitForSeconds(0).Then(() =>
            {
                foreach (Stateful component in GetStatefulComponents())
                {
                    Data data;
                    string id = component.GetStatefulID();

                    if (!this.state.TryGetValue(id, out data))
                        data = new Data();

                    if (wholeState.ContainsKey(id))
                        wholeState[id].Merge(data);

                    component.Load(data, this);
                }
            }));

            file.Close();
        }
        catch (FileNotFoundException) {
            this.state = new Dictionary<string, Data>();
        }
    }

    public void Save(string stateFile)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(stateFile, FileMode.OpenOrCreate);

        if (state == null)
            state = new Dictionary<string, Data>();

        foreach (Stateful component in GetStatefulComponents())
        {
            Data data;
            string id = component.GetStatefulID();

            if (!state.TryGetValue(id, out data))
                data = new Data();

            component.Save(data, this);

            this.state[id] = data;

            if (wholeState.ContainsKey(id))
                wholeState[id].Merge(data);
        }

        bf.Serialize(file, state);
        file.Close();
    }

    Stateful[] GetStatefulComponents()
    {
        MonoBehaviour[] scripts = FindObjectsOfType<MonoBehaviour>();
        List<Stateful> components = new List<Stateful>();

        foreach (MonoBehaviour script in scripts)
            if (script is Stateful)
                components.Add(script as Stateful);

        return components.ToArray();
    }

    
    public static Data GetData(Stateful component)
    {
        return wholeState[component.GetStatefulID()];
    }
}

[Serializable]
public class Data : Dictionary<string, object>
{
    public Data() : base() { }

    protected Data(SerializationInfo info, StreamingContext context)
        : base(info, context) {}


    public bool TryGet(string identifier, out object value)
    {
        value = null;
        return base.TryGetValue(identifier, out value);
    }

    public bool TryGet<T>(string identifier, out T value)
    {
        value = default(T);

        object val;
        if (!TryGet(identifier, out val))
            return false;

        if (!(val is T))
            return false;

        value = (T)val;
        return true;
    }

    public object Get(string identifier)
    {
        object data;
        TryGet(identifier, out data);
        return data;
    }

    public T Get<T>(string identifier, T defaultValue)
    {
        object data;
        if (!TryGet(identifier, out data))
            return defaultValue;

        if (data is T)
            return (T)data;

        return defaultValue;
    }

    public T Get<T>(string identifier)
        where T : class
    {
        return Get<T>(identifier, null);
    }


    public bool Put(string identifier, object value, out object existing)
    {
        bool existed = Contains(identifier);
        existing = existed ? Get(identifier) : null;

        base[identifier] = value;
        return existed;
    }

    public object Put(string identifier, object value)
    {
        object existing;
        Put(identifier, value, out existing);
        return existing;
    }


    public bool Contains(string identifier)
    {
        return base.ContainsKey(identifier);
    }

    
    public void Merge(Data other)
    {
        foreach (KeyValuePair<string, object> p in other)
            this[p.Key] = p.Value;
    }
}

public interface Stateful
{
    void Save(Data data, GameState context);
    void Load(Data data, GameState context);
    string GetStatefulID(); //## not hard to imagine a collision or a malicious value, but hey...
}

public static class StatefulExtensions
{
    public static Data GetData(this Stateful component)
    {
        return GameState.GetData(component);
    }
}