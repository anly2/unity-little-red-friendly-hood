using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Menu : MonoBehaviour {

    protected virtual void Awake()
    {
        Hide();
    }


    public virtual void Show()
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);
    }
}
