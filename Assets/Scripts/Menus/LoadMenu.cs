using UnityEngine;
using System.Collections;

public class LoadMenu : Menu {

    [HideInInspector]
    public Menu referrer = null;

    public GameObject saveViewPrefab;
    public GameObject saveList;


    public override void Show()
    {
        base.Show();

        var saves = SaveManager.GetSaves();
        foreach (var save in saves)
        {
            var view = Instantiate(saveViewPrefab);
            view.transform.SetParent(saveList.transform, false);

            var parts = view.GetComponent<SaveView>();
            parts.image.sprite = save.thumb;
            parts.text.text = save.name;
        }
    }

    public override void Hide()
    {
        base.Hide();
        referrer = null;
    }


    public void Back()
    {
        if (referrer == null)
            return;

        referrer.Show();
        Hide();
    }
}
