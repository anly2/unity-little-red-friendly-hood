using UnityEngine;
using System.Collections;

public class LoadMenu : Menu {

    [HideInInspector]
    public Menu referrer = null;

    public GameObject saveViewPrefab;
    public GameObject saveList;


    public override void Show(bool doPause=true)
    {
        base.Show(doPause);


        foreach (Transform child in saveList.transform)
            Destroy(child.gameObject);


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

    public override void Hide(bool doUnpause = true)
    {
        base.Hide(doUnpause);
        referrer = null;
    }


    public void Back()
    {
        if (referrer == null)
            return;
        
        base.Hide();
        referrer.Show();
    }
}
