using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Menu : MonoBehaviour {

    protected virtual void Awake()
    {
        Hide();
    }


    private float prevTimeScale = 1;

    public virtual void Show(bool doPause = true)
    {
        if (doPause)
        {
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0;
        }

        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(true);
    }

    public virtual void Hide(bool doUnpause = true)
    {
        if (doUnpause)
            Time.timeScale = prevTimeScale;

        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);
    }
}
