using UnityEngine;

public class MainMenu : Menu {

    public void NewGame()
    {
        SaveManager.Reload();
    }

    public void LoadLast()
    {
        //#!! checks
        SaveManager.Load(SaveManager.GetSaves()[0].name);
    }
    

    public void SaveGame()
    {
        base.Hide();
        var m = Menus.Get<SaveMenu>();
        m.referrer = this;
        m.Show();
    }

    public void LoadGame()
    {
        base.Hide();
        var m = Menus.Get<LoadMenu>();
        m.referrer = this;
        m.Show();
    }


    public void Exit()
    {
        Application.Quit();
    }
}
