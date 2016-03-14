using UnityEngine;
using System.Collections;

public static class SpeechExtensions {

    public static GameObject speechBubblePrefab;

    public static void Say(this MonoBehaviour speaker, string text)
    {
        speaker.gameObject.Say(text);
    }

    public static void Say(this GameObject speaker, string text)
    {
        SpeechBubble speechBubble = (Object.Instantiate(Resources.Load("Speech Bubble")) as GameObject).GetComponent<SpeechBubble>();
        speechBubble.SetText(text);
        speechBubble.SetTip(0f, SpeechBubble.Side.Right);

        GameObject canvas = GameObject.FindWithTag("Canvas");

        Vector3 sp = Camera.main.WorldToScreenPoint(speaker.transform.position);
        sp.y -= (canvas.transform as RectTransform).sizeDelta.y;
        
        (speechBubble.transform as RectTransform).anchoredPosition = sp;
        
        speechBubble.transform.SetParent(GameObject.FindWithTag("Canvas").transform, false);
    }

    /*todo
    speech bubble pivot point at tip
    set tip (percent, bool axis)
    Say tries to get renderer.bounds.max
    */

    /*
        SetText("Something terribly long and long and long which would fit on no less than three lines");
        SetBackgroundColor(Color.blue);
        SetTextColor(Color.red);
    //*/
}
