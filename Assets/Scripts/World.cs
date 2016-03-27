using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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

            Recreate(data as List<WorldAction>);

            file.Close();
        }
        catch (FileNotFoundException) { }
    }
    

    string GetSaveFilePath()
    {
        return Application.persistentDataPath + "/" + saveFilePath;
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
        foreach (WorldAction action in actions.ToArray())
            yield return action.act();
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
