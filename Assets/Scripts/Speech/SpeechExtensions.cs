﻿using UnityEngine;
using System.Collections;
using System;

public static class SpeechExtensions {

    /* Invocation on Speaker scripts of Actors */

    public static SpeechBubble Say(this Speaker speaker, string text,
        float? timeShown = null,
        Vector3? speechPosition = null,
        SpeechBubble.Side? tipSide = null,
        float? tipPositionOnSide = null)
    {
        useSpeechPivot(speaker, ref speechPosition, ref tipSide, ref tipPositionOnSide);

        return speaker.gameObject.Say(text, timeShown, speechPosition, tipSide.Value, tipPositionOnSide.Value);
    }


    public static SpeechBubble Think(this Speaker speaker, string text,
        float? timeShown = null,
        Vector3? speechPosition = null,
        SpeechBubble.Side? tipSide = null,
        float? tipPositionOnSide = null)
    {
        useSpeechPivot(speaker, ref speechPosition, ref tipSide, ref tipPositionOnSide);

        return speaker.gameObject.Think(text, timeShown, speechPosition, tipSide.Value, tipPositionOnSide.Value);
    }


    /* Invocation on GameObject actors */

    /**
    @param timeShown - null to estimate based on text length; negative to denote 'indefinataly'
    */
    public static SpeechBubble Say(this GameObject speaker, string text,
        float? timeShown = null,
        Vector3? speechPosition = null,
        SpeechBubble.Side tipSide = SpeechBubble.Side.Left,
        float tipPositionOnSide = 0f)
    {
        return ShowSpeechBubble(speaker, SpeechBubble.prefab, text,
            timeShown, speechPosition, tipSide, tipPositionOnSide);
    }

    /**
    @param timeShown - null to estimate based on text length; negative to denote 'indefinataly'
    */
    public static SpeechBubble Think(this GameObject speaker, string text,
        float? timeShown = null,
        Vector3? speechPosition = null,
        SpeechBubble.Side tipSide = SpeechBubble.Side.Left,
        float tipPositionOnSide = 0f)
    {
        return ShowSpeechBubble(speaker, ThoughtBubble.prefab, text,
            timeShown, speechPosition, tipSide, tipPositionOnSide);
    }
    

    /* Private, Actual worker method */

    private static SpeechBubble ShowSpeechBubble(GameObject speaker, GameObject bubblePrefab,
        string text,
        float? timeShown = null,
        Vector3? speechPosition = null,
        SpeechBubble.Side tipSide = SpeechBubble.Side.Left,
        float tipPositionOnSide = 0f)
    {
        { // Get the position without polluting the namespace
            Speaker speakerComponent = speaker.GetComponent<Speaker>();

            if (speakerComponent != null)
                useSpeechPivot(speakerComponent, ref speechPosition);
        }
        
        //Handle default values
        float duration = timeShown.HasValue ? timeShown.Value : text.EstimateReadTime();
        Vector3 position = speechPosition.HasValue ? speechPosition.Value : speaker.transform.position;

        // Initialise the Speech Bubble
        SpeechBubble speechBubble = GameObject.Instantiate(bubblePrefab).GetComponent<SpeechBubble>();
        speechBubble.SetText(text);
        speechBubble.SetTip(tipSide, tipPositionOnSide);

        // Add to a Canvas (the UI canvas in this case)
        speechBubble.transform.SetParent(GameObject.FindWithTag("Canvas").transform, false);

        // Position the Speech Bubble on the UI canvas
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(position);
        (speechBubble.transform as RectTransform).anchoredPosition = screenPosition;

        //Anchor the Speech Bubble to the in-world Game Object
        speechBubble.Anchor(speaker);

        // Invoke Hide after the desired time has passed
        if (duration >= 0)
            speechBubble.Invoke("Hide", duration);

        return speechBubble;
    }


    /* Helper methods */

    private static void useSpeechPivot(Speaker speaker, ref Vector3? position,
        ref SpeechBubble.Side? side, ref float? positionOnSide)
    {
        Vector3 speechPivot = speaker.speechPivot;

        if (!side.HasValue)
            side = (speechPivot.x < 0f) ? SpeechBubble.Side.Right : SpeechBubble.Side.Left;

        if (!positionOnSide.HasValue)
            positionOnSide = Mathf.Lerp(0f, 1f, speechPivot.y / 2 + 0.5f);

        Vector3 extents = speaker.GetComponent<Renderer>().bounds.extents;

        speechPivot.x *= extents.x;
        speechPivot.y *= extents.y;

        if (!position.HasValue)
            position = speaker.transform.position + speechPivot;
    }

    private static void useSpeechPivot(Speaker speaker, ref Vector3? position)
    {
        SpeechBubble.Side? __s = SpeechBubble.Side.Left; //ignored
        float? __t = 1; //ignored
        useSpeechPivot(speaker, ref position, ref __s, ref __t);
    }



    /*todo
    -(unnecessary) create an in-world canvas, rather than using the UI one 
    */
}
