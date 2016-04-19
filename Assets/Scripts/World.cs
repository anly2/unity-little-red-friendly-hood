using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public class World : MonoBehaviour {

    public static World I { get { return instance; } }
    private static World instance;

    private List<WorldAction> history = new List<WorldAction>();

    public string saveFilePath = "world.dat";
    public float fastForwardSpeed = 10;


    /* Keep WorldState persistent */

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);

        Load();
    }

    void OnDestroy()
    {
        if (instance != this)
            return;

        //Save(); //#!PRODUCTION
    }


    /* Saving and Loading */

    public void Save()
    {
        Save(GetSaveFilePath());
    }

    public void Save(string saveFilePath)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(saveFilePath, FileMode.OpenOrCreate);

        bf.Serialize(file, history);

        file.Close();
    }


    public void Load()
    {
        Load(GetSaveFilePath());
    }

    public void Load(string saveFilePath)
    {
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(saveFilePath, FileMode.Open);

            object data = bf.Deserialize(file);

            if (data == null)
                return;

            Load(data as List<WorldAction>);

            file.Close();
        }
        catch (FileNotFoundException) { }
    }
    

    string GetSaveFilePath()
    {
        return Application.persistentDataPath + "/" + saveFilePath;
    }


    /* Reloading the World */

    public void Reload(string sceneName = null, float speed = 100) //#!DEBUG
    {
        List<WorldAction> actions = new List<WorldAction>(history);
        history.Clear();
        Load(actions, sceneName, speed);
    }

    public void Load(List<WorldAction> actions, string sceneName = null, float speed = 100)
    {
        speed = Mathf.Clamp(speed, 0, 100);

        //SceneManager.LoadScene(sceneName ?? SceneManager.GetActiveScene().name);
        Application.LoadLevel(Application.loadedLevel);

        WaitForSceneToLoad()
            .Then(() => Recreate(actions, speed))
            .Start(this);
    }


    private int justLoadedLevel = -1;

    private IEnumerator WaitForSceneToLoad(int level = -1)
    {
        while (justLoadedLevel < 0 || (level != -1 && level != justLoadedLevel))
            yield return null;

        justLoadedLevel = -1;
    }

    void OnLevelWasLoaded(int level)
    {
        justLoadedLevel = level;
    }


    /* Recreate a world state */

    public AttachedCoroutine Recreate() //#!DEBUG
    {
        return Recreate(history, false);
    }

    AttachedCoroutine Recreate(List<WorldAction> actions, bool immediate = true)
    {
        return Recreate(actions, immediate? 100 : fastForwardSpeed);
    }

    AttachedCoroutine Recreate(List<WorldAction> actions, float speed)
    {
        Time.timeScale = speed;

        return PerformActions(actions)
            .Then(() => Time.timeScale = 1)
            .Start(this);
    }

    private IEnumerator PerformActions(List<WorldAction> actions)
    {
        PlayerMovement playerControl = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
        playerControl.enabled = false;

        foreach (WorldAction action in actions.ToArray())
            yield return action.act();

        playerControl.enabled = true;
    }


    /* Performing Actions */

    public void LogAction(WorldAction action)
    {
        this.history.Add(action);
    }

    public WorldAction LogMovement(GameObject actor, Vector3 position)
    {
        WorldAction action = new WorldAction.Move(actor, position);
        LogAction(action);
        return action;
    }
}



[Serializable]
public abstract class WorldAction
{
    public abstract IEnumerator act();

    [Serializable]
    public class Move : WorldAction
    {
        private string actorPath;
        private float x, y, z;


        public Move(GameObject actor, Vector3 position)
        {
            this.actorPath = actor.GetPath();
            this.x = position.x;
            this.y = position.y;
            this.z = position.z;
        }


        public override IEnumerator act()
        {
            GameObject actor = GameObject.Find(actorPath);

            if (actor == null)
                throw new Exception("Failed to find the actor object '" + actorPath + "'");

            return actor.MotionTo(new Vector3(x,y, z), 1f);
        }
    }
}

public static class GameObjectPathExtensions
{
    public static string GetPath(this GameObject gameObject)
    {
        return gameObject.transform.GetPath();
    }

    public static string GetPath(this Transform transform)
    {
        string path = "/" + transform.name;

        while (transform.parent != null)
        {
            transform = transform.parent;
            path = "/" + transform.name + path;
        }

        return path;
    }
}
