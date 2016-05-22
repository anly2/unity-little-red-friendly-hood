using UnityEngine;
using System.Collections.Generic;

public class Menus : MonoBehaviour {

    private static Menus instance = null;

    void Awake()
    {
        if (instance != null)
            Debug.LogWarning("You have more than one Menus container");

        instance = this;
    }


    public static MENU Get<MENU>()
    {
        return instance.GetComponentInChildren<MENU>();
    }

    public static GameObject Get(string menuname, bool doTrim = true)
    {
        Transform menu;

        menu = instance.transform.Find(menuname);
        if (menu != null)
            return menu.gameObject;

        menuname = menuname.Trim();
        if (menuname.ToLower().EndsWith("menu"))
            menuname = menuname.Substring(0, menuname.Length - 4);
        menuname = menuname.Trim();

        menu = instance.transform.Find(menuname);
        if (menu != null)
            return menu.gameObject;

        return null;
    }
}
