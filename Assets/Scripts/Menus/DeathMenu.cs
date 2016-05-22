using UnityEngine;
using UnityEngine.UI;

public class DeathMenu : Menu {

    public Text messageBox;
    public Cemetery cemetery;


    void Start()
    {
        if (cemetery == null)
            cemetery = GameObject.FindObjectOfType<Cemetery>();
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

    public void AddGraves(string[] gravestones, bool doFormatEngravings = true)
    {
        foreach (string engraving in gravestones)
            AddGrave(engraving, doFormatEngravings);
    }


    public void AddGrave(string engraving, bool doFormatEngraving = true)
    {
        if (cemetery == null)
            return;

        if (doFormatEngraving)
            engraving = FormatEngraving(engraving);

        cemetery.AddGrave(engraving);
    }
    
    public string FormatEngraving(string engraving)
    {
        int currentYear = cemetery.currentYear + Random.Range((int)60, 100);
        cemetery.currentYear = currentYear;

        int lrrhAge = currentYear - Random.Range((int)10, 15);
        int grannyAge = currentYear - Random.Range((int)50, 60);
        return "<b>" + engraving
            .Replace("%1", "</b>\n<size=14>" + lrrhAge + " - " + currentYear + " AD</size>\n<i>")
            .Replace("%2", "</b>\n<size=14>" + grannyAge + " - " + currentYear + " AD</size>\n<i>")
            + "</i>";
    }


    public void Restart()
    {
        SaveManager.Reload();
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
