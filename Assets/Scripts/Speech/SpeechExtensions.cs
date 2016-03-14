using UnityEngine;
using System.Collections;

public static class SpeechExtensions {

    public static SpeechBubble Say(this Speaker speaker, string text)
    {
        return Say(speaker.gameObject, text);
    }

    public static SpeechBubble Say(this GameObject speaker, string text)
    {
        // Try to use the Speaker script component
        try
        {
            Vector3 speechPivot = speaker.GetComponent<Speaker>().speechPivot;

            SpeechBubble.Side side = (speechPivot.x < 0f) ? SpeechBubble.Side.Right : SpeechBubble.Side.Left;
            float positionOnSide = Mathf.Lerp(0f, 1f, speechPivot.y / 2 + 0.5f);

            Vector3 extents = speaker.GetComponent<Renderer>().bounds.extents;

            speechPivot.x *= extents.x;
            speechPivot.y *= extents.y;

            Vector3 speechPosition = speaker.transform.position + speechPivot;

            return Say(speaker, text, speechPosition, side, positionOnSide);
        }
        catch (System.NullReferenceException) //if not 'Speaker'
        {
            return Say(speaker, text, speaker.transform.position);
        }
    }

    public static SpeechBubble Say(this GameObject speaker, string text, Vector3 speechPosition)
    {
        return Say(speaker, text, speechPosition, SpeechBubble.Side.Left, 0f);
    }

    public static SpeechBubble Say(this GameObject speaker, string text, Vector3 speechPosition, SpeechBubble.Side tipSide, float tipPositionOnSide)
    {
        return Say(speaker, text, speechPosition, new SpeechBubble.Tip(tipPositionOnSide, tipSide));
    }

    public static SpeechBubble Say(this GameObject speaker, string text, Vector3 speechPosition, SpeechBubble.Tip tip)
    {
        // Initialise the Speech Bubble
        SpeechBubble speechBubble = Object.Instantiate(SpeechBubble.prefab).GetComponent<SpeechBubble>();
        speechBubble.SetText(text);
        speechBubble.SetTip(tip);

        // Add to a Canvas (the UI canvas in this case)
        speechBubble.transform.SetParent(GameObject.FindWithTag("Canvas").transform, false);
        
        // Position the Speech Bubble on the UI canvas
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(speechPosition);
        (speechBubble.transform as RectTransform).anchoredPosition = screenPosition;

        return speechBubble;
    }

    /*todo
    -(unnecessary) create an in-world canvas, rather than using the UI one 
    */
}
