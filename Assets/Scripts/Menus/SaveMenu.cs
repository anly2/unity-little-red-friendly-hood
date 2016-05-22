using UnityEngine;
using UnityEngine.UI;

public class SaveMenu : Menu {

    [HideInInspector]
    public Menu referrer = null;

    public GameObject saveViewPrefab;
    public GameObject saveList;

    public Text saveName;


    public void MakeSave()
    {
        base.Hide(true);

        float ts = Time.timeScale;
        new WaitForSeconds(0)
            .Then(() => SaveManager.Save(saveName.text))
            .Then(() => Time.timeScale = ts)
            .Then(() => Back())
            .Start(this);
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
