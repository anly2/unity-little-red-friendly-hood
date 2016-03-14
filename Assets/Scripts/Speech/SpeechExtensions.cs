using UnityEngine;
using System.Collections;

public static class SpeechExtensions {

    public static GameObject speechBubblePrefab = Resources.Load("Speech Bubble") as GameObject;

    public static void Say(this Speaker speaker, string text)
    {
        Say(speaker.gameObject, text);
    }

    public static void Say(this GameObject speaker, string text)
    {
        // Initialise the Speech Bubble
        SpeechBubble speechBubble = Object.Instantiate(speechBubblePrefab).GetComponent<SpeechBubble>();
        speechBubble.SetText(text);

        // Add to a Canvas (the UI canvas in this case)
        speechBubble.transform.SetParent(GameObject.FindWithTag("Canvas").transform, false);

        // Position the Speech Bubble appropriately
        Vector3 speechPosition;

        //  Try to use the point defined on the Speaker script component
        try
        {
            Vector3 speechPivot = speaker.GetComponent<Speaker>().speechPivot;
            Vector3 extents = speaker.GetComponent<Renderer>().bounds.extents;

            speechPivot.x *= extents.x;
            speechPivot.y *= extents.y;

            speechPosition = speaker.transform.position + speechPivot;
        }
        catch (System.NullReferenceException) //if not 'Speaker'
        {
            speechPosition = speaker.transform.position;
        }
        
        // Position the Speech Bubble on the UI canvas
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(speechPosition);
        (speechBubble.transform as RectTransform).anchoredPosition = screenPosition;
    }

    /*todo
    -(unnecessary) create an in-world canvas, rather than using the UI one 
    */

    /*
        SetText("Something terribly long and long and long which would fit on no less than three lines");
        SetBackgroundColor(Color.blue);
        SetTextColor(Color.red);
    //*/
}
