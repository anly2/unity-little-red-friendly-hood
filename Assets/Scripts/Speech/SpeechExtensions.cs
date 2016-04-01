using UnityEngine;
using System.Collections.Generic;
using System;

public static class SpeechExtensions {

    /* Invocation on Speaker scripts of Actors */

    public static SpeechBubble Say(this Speaker speaker, string text,
        float? timeShown = null,
        Vector3? speechPosition = null,
        SpeechBubble.Side? tipSide = null,
        float? tipPositionOnSide = null)
    {
        return speaker.gameObject.Say(text, timeShown,
            speechPosition, tipSide.Value, tipPositionOnSide.Value);
    }


    public static SpeechBubble Think(this Speaker speaker, string text,
        float? timeShown = null,
        Vector3? speechPosition = null,
        SpeechBubble.Side? tipSide = null,
        float? tipPositionOnSide = null)
    {
        return speaker.gameObject.Think(text, timeShown,
            speechPosition, tipSide.Value, tipPositionOnSide.Value);
    }


    /* Invocation on GameObject actors */

    /**
    @param timeShown - null to estimate based on text length; negative to denote 'indefinataly'
    */
    public static SpeechBubble Say(this GameObject speaker, string text,
        float? timeShown = null,
        Vector3? speechPosition = null,
        SpeechBubble.Side? tipSide = null,
        float? tipPositionOnSide = null)
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
        SpeechBubble.Side? tipSide = null,
        float? tipPositionOnSide = null)
    {
        return ShowSpeechBubble(speaker, ThoughtBubble.prefab, text,
            timeShown, speechPosition, tipSide, tipPositionOnSide);
    }
    

    /* Private, Actual worker method */

    private static SpeechBubble ShowSpeechBubble(GameObject speaker, GameObject bubblePrefab,
        string text,
        float? timeShown = null,
        Vector3? speechPosition = null,
        SpeechBubble.Side? tipSide = null,
        float? tipPositionOnSide = null)
    {
        InferSpeechBubblePositioning(speaker, ref speechPosition, ref tipSide, ref tipPositionOnSide);
        
        //Handle default values
        float duration = timeShown ?? text.EstimateReadTime();
        Vector3 position = speechPosition ?? speaker.transform.position;

        // Initialise the Speech Bubble
        SpeechBubble speechBubble = GameObject.Instantiate(bubblePrefab).GetComponent<SpeechBubble>();
        speechBubble.SetText(text);
        speechBubble.SetTip(tipSide.Value, tipPositionOnSide.Value);

        // Add to a Canvas (the UI canvas in this case)
        speechBubble.transform.SetParent(GameObject.FindWithTag("Canvas").transform, false);

        // Position the Speech Bubble on the UI canvas
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(position);
        (speechBubble.transform as RectTransform).anchoredPosition = screenPosition;

        //Anchor the Speech Bubble to the in-world Game Object
        speechBubble.Anchor(speaker);

        // Invoke Hide after the desired time has passed
        if (duration >= 0)
            speechBubble.Invoke("Hide", duration);

        return speechBubble;
    }


    /* Automatic Positioning */

    public static void InferSpeechBubblePositioning(GameObject speaker,
        ref Vector3? speechPivot, ref SpeechBubble.Side? sbTipSide, ref float? sbTipPositionOnSide,
        Vector2 bubbleSize = default(Vector2))
    {
        List<InterestPoint> interestPoints = GetInterestPoints(speaker);
        List<Vector2> avoided  = GetAvoidedPoints(speaker,
            speechPivot ?? InferSpeechPivot(speaker, sbTipSide, sbTipPositionOnSide));
        
        foreach (InterestPoint poi in interestPoints)
        {
            float desirability = 0;
            Vector2 screenPOI = ToUI(poi.position);

            foreach (Vector2 avoidedPoint in avoided)
                desirability += Vector2.Distance(screenPOI, avoidedPoint);

            poi.desirability = desirability;
        }

        interestPoints.Sort();
        interestPoints.Reverse();


        InterestPoint selectedPOI = interestPoints[0];
        
        /* #! NOT GOOD ENOUGH
        //#! should use the speech PIVOT not POI position
        //#! Rect should be the bounds of the SpeechBubble, not just a Rect
        foreach (InterestPoint poi in interestPoints)
        {
            Rect bounds = new Rect(ToUI(poi.position), bubbleSize);

            if (FullyVisible(bounds)) {
                selectedPOI = poi;
                break;
            }
        }
        //*/
        
        
        // "Return"
        if (!speechPivot.HasValue)
            speechPivot = selectedPOI.position;

        if (!sbTipSide.HasValue)
            sbTipSide = Opposite(selectedPOI.side);

        if (!sbTipPositionOnSide.HasValue)
            sbTipPositionOnSide = selectedPOI.positionOnSide;
    }


    private static List<Vector2> GetAvoidedPoints(GameObject speaker, Vector3? pivot)
    {
        List<Vector2> avoided = new List<Vector2>();

        // Strive away from the Center of the Screen
        avoided.Add(new Vector2(Screen.width / 2, Screen.height / 2));



        // Avoid the opposite side of the speaker bounds, as delimited by the center
        if (pivot.HasValue)
        {
            Bounds bounds = speaker.GetComponent<Renderer>().bounds;

            Vector3 ptc = bounds.center - pivot.Value;
            Vector3 oposed = pivot.Value + 2 * ptc;
            avoided.Add(ToUI(oposed));
        }


        // Avoid all other actors
        List<GameObject> actors = GetAllActors();

        foreach (GameObject actor in actors)
            avoided.Add(ToUI(GetSpeechPivot(actor)));


        return avoided;
    }


    private class InterestPoint : IComparable<InterestPoint>
    {
        public float desirability;
        public readonly Vector3 position;
        public readonly SpeechBubble.Side side;
        public readonly float positionOnSide;

        public InterestPoint(Bounds bounds, SpeechBubble.Side side, SpeechBubble.Side offside)
            : this(bounds, side,
                  (IsVertical(side)? 
                        (offside == SpeechBubble.Side.Left ? 0f : 1f) :
                        (offside == SpeechBubble.Side.Top ? 0f : 1f)) )
        {}

        public InterestPoint(Bounds bounds, SpeechBubble.Side side, float positionOnSide)
        {
            this.desirability = -1;
            this.side = side;
            this.positionOnSide = Mathf.Clamp01(positionOnSide);


            float p = Mathf.Clamp(positionOnSide, 0.1f, 0.9f);
            this.position = CalcPoint(bounds, side, p);
        }

        public static Vector3 CalcPoint(Bounds bounds, SpeechBubble.Side side, float p)
        {
            switch (side)
            {
                case SpeechBubble.Side.Left:
                    return new Vector3(bounds.min.x, bounds.min.y + bounds.size.y * p);

                case SpeechBubble.Side.Right:
                    return new Vector3(bounds.max.x, bounds.min.y + bounds.size.y * p);

                case SpeechBubble.Side.Top:
                    return new Vector3(bounds.min.x + bounds.size.x * p, bounds.max.y);

                case SpeechBubble.Side.Bottom:
                    return new Vector3(bounds.min.x + bounds.size.x * p, bounds.min.y);

                default:
                    return bounds.center;
            }
        }

        public int CompareTo(InterestPoint other)
        {
            return desirability.CompareTo(other.desirability);
        }

        public override string ToString()
        {
            return position.ToString();
        }
    }

    private static bool IsHorizontal(this SpeechBubble.Side side)
    {
        return !IsVertical(side);
    }

    private static bool IsVertical(this SpeechBubble.Side side)
    {
        return (side == SpeechBubble.Side.Top || side == SpeechBubble.Side.Bottom);
    }

    private static SpeechBubble.Side Opposite(this SpeechBubble.Side side)
    {
        switch (side)
        {
            case SpeechBubble.Side.Top:
                return SpeechBubble.Side.Bottom;
            case SpeechBubble.Side.Right:
                return SpeechBubble.Side.Left;
            case SpeechBubble.Side.Bottom:
                return SpeechBubble.Side.Top;
            case SpeechBubble.Side.Left:
                return SpeechBubble.Side.Right;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    private static List<InterestPoint> GetInterestPoints(GameObject speaker)
    {
        List<InterestPoint> interestPoints = new List<InterestPoint>();

        Renderer r = speaker.GetComponent<Renderer>();
        Bounds bounds = r.bounds;

        foreach (SpeechBubble.Side side in Enum.GetValues(typeof(SpeechBubble.Side)))
        {
            interestPoints.Add(new InterestPoint(bounds, side, 0f));
            interestPoints.Add(new InterestPoint(bounds, side, 1f));
        }

        return interestPoints;
    }


    private static Vector3 GetSpeechPivot(this GameObject speaker)
    {
        Speaker s = speaker.GetComponent<Speaker>();
        Renderer r = speaker.GetComponent<Renderer>();
        
        if (r == null)
            return speaker.transform.position;

        if (s == null)
            return r.bounds.center;
        

        Vector3 pivot = s.speechPivot;

        pivot.x *= r.bounds.extents.x;
        pivot.y *= r.bounds.extents.y;

        pivot += r.bounds.center;


        return pivot;
    }

    private static Vector3? InferSpeechPivot(this GameObject speaker, SpeechBubble.Side? side, float? positionOnSide)
    {
        Renderer r = speaker.GetComponent<Renderer>();

        if (r == null)
            return speaker.transform.position;

        if (!side.HasValue)
            return null;


        return InterestPoint.CalcPoint(r.bounds, Opposite(side.Value), positionOnSide ?? 0.5f);
    }


    private static List<GameObject> GetAllActors()
    {
        //#! CURRENTLY NOT OPTIMIZED AT ALL !
        //# Should maintain a list, or use tags or a specific component.
        //# Instead of going through ALL rendered objects.

        List<GameObject> actors = new List<GameObject>();

        Renderer[] rendered = GameObject.FindObjectsOfType<Renderer>();

        foreach (Renderer r in rendered)
        {
            if (r.sortingLayerName == "Actor")
                actors.Add(r.gameObject);
        }

        return actors;
    }


    private static Vector3 ToUI(Vector3 point)
    {
        return Camera.main.WorldToScreenPoint(point);
    }

    private static bool FullyVisible(this Rect box)
    {
        if (box.xMin < 0)
            return false;

        if (box.xMax > Screen.width)
            return false;

        if (box.yMin < 0)
            return false;

        if (box.yMax > Screen.height)
            return false;

        return true;
    }


    /*todo
    -(unnecessary) create an in-world canvas, rather than using the UI one 
    */
}
