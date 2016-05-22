using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;

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

        GameState loader = new GameObject("Loader").AddComponent<GameState>();
        loader.autoact = false;
        loader.Load(filename);
        GameObject.Destroy(loader.gameObject);
    }

    public static void Save(string saveName)
    {

        string path = GetSavesFolder();

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        string filename = path + "/" + saveName;

        GameState saver = new GameObject("Saver").AddComponent<GameState>();
        saver.autoact = false;
        saver.Save(filename + ".gsv");
        GameObject.Destroy(saver.gameObject);

        Application.CaptureScreenshot(filename + ".thumb");
    }


    // Helper methods //

    public static GameSave[] GetSaves()
    {
        try
        {
            string[] files = Directory.GetFiles(GetSavesFolder(), "*.gsv");
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
