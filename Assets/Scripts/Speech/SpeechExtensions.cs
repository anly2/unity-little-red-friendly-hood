using UnityEngine;
using System.Collections;

public static class SpeechExtensions {

    public static SpeechBubble Say(this Speaker speaker, string text)
    {
        return Say(speaker.gameObject, text);
    }

    public static SpeechBubble Say(this GameObject speaker, string text)
    {
        // Initialise the Speech Bubble
        SpeechBubble speechBubble = Object.Instantiate(SpeechBubble.prefab).GetComponent<SpeechBubble>();
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

        return speechBubble;
    }

    /*todo
    reason about Side (quadrant of speech pivot? quadrant of object on screen? explicit param?)
    -(unnecessary) create an in-world canvas, rather than using the UI one 
    */
}
