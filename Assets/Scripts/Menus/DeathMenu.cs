using UnityEngine;
using UnityEngine.UI;

public class DeathMenu : Menu {

    public Text messageBox;
    public Cemetery cemetary;


    void Start()
    {
        if (cemetary == null)
            cemetary = GameObject.FindObjectOfType<Cemetery>();
    }

    
    public void Die(string message, params string[] gravestones)
    {
        Show(message);
        AddGraves(gravestones);
    }

    public void Show(string message = "You died.")
    {
        messageBox.text = message;
        base.Show();
    }

    public void AddGraves(string[] gravestones)
    {
        foreach (string engraving in gravestones)
            AddGrave(engraving);
    }

    public void AddGrave(string engraving)
    {
        if (cemetary == null)
            return;

        cemetary.AddGrave(engraving);
    }


    public void Restart()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    public void Load()
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
