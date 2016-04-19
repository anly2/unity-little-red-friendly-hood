using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SaveManager : MonoBehaviour {
    
    public static void Save(string name)
    {

        string path = GetSavesFolder();

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        string filename = path + "/" + name;

        GameState saver = new GameObject("Saver").AddComponent<GameState>();
        saver.autoact = false;
        saver.Save(filename + ".gsv");
        Destroy(saver.gameObject);

        Application.CaptureScreenshot(filename + ".thumb");
    }

    public static GameSave[] GetSaves()
    {
        try {
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
        try {
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
}
