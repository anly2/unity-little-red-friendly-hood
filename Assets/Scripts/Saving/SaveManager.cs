using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public class SaveManager
{

    // Save and Load functionality //

    public static void Load(string saveName, bool reloadLevel = true)
    {
        if (reloadLevel)
        {
            Reload(null, () => Load(saveName, false));
            return;
        }

        string filename = GetSavesFolder() + Path.GetFileNameWithoutExtension(saveName) + ".gsv";

        if (!File.Exists(filename))
            throw new FileNotFoundException();


        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filename, FileMode.Open);

        object rawState = bf.Deserialize(file);

        if (rawState == null)
            return;


        var state = rawState as Dictionary<string, Data>;

        foreach (Stateful component in GetStatefulComponents())
        {
            Data data;
            string id = component.GetStatefulID();

            if (!state.TryGetValue(id, out data))
                data = new Data();

            component.Load(data);
        }

        file.Close();
    }

    public static void Save(string saveName)
    {

        string path = GetSavesFolder();

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        string filename = path + "/" + saveName;


        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filename + ".gsv", FileMode.OpenOrCreate);

        var state = new Dictionary<string, Data>();

        foreach (Stateful component in GetStatefulComponents())
        {
            Data data;
            string id = component.GetStatefulID();

            if (!state.TryGetValue(id, out data))
                data = new Data();

            component.Save(data);
            state[id] = data;
        }

        bf.Serialize(file, state);
        file.Close();


        Application.CaptureScreenshot(filename + ".thumb");
    }


    protected static Stateful[] GetStatefulComponents()
    {
        MonoBehaviour[] scripts = GameObject.FindObjectsOfType<MonoBehaviour>();
        List<Stateful> components = new List<Stateful>();

        foreach (MonoBehaviour script in scripts)
            if (script is Stateful)
                components.Add(script as Stateful);

        return components.ToArray();
    }


    // Retrieving saves //

    public static GameSave[] GetSaves()
    {
        try
        {
            string[] files = Directory.GetFiles(GetSavesFolder(), "*.gsv");

            System.Array.Sort(files, (a, b) => // swapped a and b to get DESC
                File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));

            GameSave[] saves = new GameSave[files.Length];

            for (int i = 0; i < saves.Length; i++)
                saves[i] = new GameSave(files[i]);

            return saves;
        }
        catch (DirectoryNotFoundException)
        {
            return new GameSave[0];
        }
    }

    public struct GameSave
    {
        public readonly string name;
        public readonly string path;
        public readonly Sprite thumb;

        public GameSave(string filename)
        {
            this.name = Path.GetFileNameWithoutExtension(filename);
            this.path = filename;

            string thumbPath = GetSavesFolder() + "/" + this.name + ".thumb";
            this.thumb = File.Exists(thumbPath) ? LoadImage(thumbPath) : null;
        }
    }


    // Helper methods //

    public static string GetSavesFolder()
    {
        return Application.persistentDataPath + "/saves/";
    }

    public static Sprite LoadImage(string path)
    {
        try
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] imageData = new byte[fs.Length];
            fs.Read(imageData, 0, (int)fs.Length);

            Texture2D texture = new Texture2D(4, 4);
            texture.LoadImage(imageData);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }


    /* Reloading the World */

    public static void Reload(string sceneName = null, System.Action whenDone = null)
    {
        var observerGO = new GameObject("Scene Change Observer");
        GameObject.DontDestroyOnLoad(observerGO);
        var observer = observerGO.AddComponent<SceneChangeObserver>();

        SceneManager.LoadScene(sceneName ?? SceneManager.GetActiveScene().name);
        //Application.LoadLevel(Application.loadedLevel);

        observer.WaitForSceneToLoad()
            .Then(() => { if (whenDone != null) whenDone(); })
            .Then(() => GameObject.Destroy(observerGO))
            .Start(observer);
    }
}

public class SceneChangeObserver : MonoBehaviour
{
    private int justLoadedLevel = -1;

    public IEnumerator WaitForSceneToLoad(int level = -1)
    {
        while (justLoadedLevel < 0 || (level != -1 && level != justLoadedLevel))
            yield return null;

        justLoadedLevel = -1;
    }

    void OnLevelWasLoaded(int level)
    {
        justLoadedLevel = level;
    }
}




[System.Serializable]
public class Data : Dictionary<string, object>
{
    public Data() : base() { }

    protected Data(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }


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
    void Save(Data data);
    void Load(Data data);
    string GetStatefulID(); //## not hard to imagine a collision or a malicious value, but hey...
}