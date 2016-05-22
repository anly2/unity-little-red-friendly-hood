using UnityEngine;
using UnityEngine.UI;

public class SaveView : MonoBehaviour {

    public Image image;
    public Text text;

    public void Load()
    {
        SaveManager.Load(text.text);
        //Menus.Get<LoadMenu>().Hide();
    }
}
