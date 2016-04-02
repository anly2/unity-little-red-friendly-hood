using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeechBubble : MonoBehaviour {

    public static GameObject prefab = Resources.Load("Speech Bubble") as GameObject;

    protected RectTransform got_Background; //GameObject Transform
    protected Text goc_Text; //GameObject Component
    protected RectTransform got_Tip; //GameObject Transform


    void Awake () {
        this.got_Background  = GetComponent<RectTransform>();
        this.goc_Text = transform.Find("Text").GetComponent<Text>();
        this.got_Tip = transform.Find("Tip") as RectTransform;

        tip = new Tip(0.5f, Side.Left);
    }


    public void Hide()
    {
        Remove();
    }

    public void Remove()
    {
        if (_anchor != null)
            Destroy(_anchor);

        Destroy(gameObject);
    }


    /* Managing the speech Text */

    public void SetText(string text)
    {
        this.goc_Text.text = text;

        float h = this.goc_Text.preferredHeight;
        RectTransform r = this.goc_Text.gameObject.transform as RectTransform;
        h -= r.offsetMax.y;
        h += r.offsetMin.y;

        got_Background.sizeDelta = new Vector2(got_Background.rect.width, h);
        
        SetTip(tip); //re-set it
    }

    public string GetText()
    {
        return this.goc_Text.text;
    }

    public string text { get { return GetText(); } set { SetText(value); } }


    /* Managing the SpeechBubble Tip position */

    public enum Side { Top, Right, Bottom, Left }

    public struct Tip {
        public readonly float position;
        public readonly Side side;
        internal float prevAngle;

        public Tip(float position, Side side)
        {
            this.position = Mathf.Clamp01(position);
            this.side = side;
            this.prevAngle = 0;
        }
    }

    public void SetTip(Side side)
    {
        SetTip(side, 0f);
    }

    public void SetTip(float position, Side side)
    {
        SetTip(side, position);
    }

    public void SetTip(Side side, float position)
    {
        SetTip(new Tip(position, side));
    }

    public virtual void SetTip(Tip tip)
    {
        //Constant?
        Vector2 insets = new Vector2(1.5f, 1.5f); //the insets of the background sprite

        //shorthands
        Vector2 size = got_Background.sizeDelta;
        Vector2 tipSize = got_Tip.sizeDelta;
        Vector2 textOffsetMin = (goc_Text.transform as RectTransform).offsetMin;
        Vector2 textOffsetMax = (goc_Text.transform as RectTransform).offsetMax * -1; //*-1 since its relative to the top right anchor, and we want it orianted towards the origin (lower left)

        //derivable values depending on tip side
        float x, y, a, px, py; //x y coords, z rot angle, pivot x% y%

        switch (tip.side) //ugly, but works
        { 
            case Side.Left:
                x = -insets.x;
                y = textOffsetMax.y + (tip.position * (size.y - textOffsetMin.y - textOffsetMax.y - tipSize.y)) + tipSize.y/2;
                a = -90;
                px = (x - tipSize.x/4) / size.x;
                py = (y) / size.y;
                break;

            case Side.Top:
                x = textOffsetMax.x + (tip.position * (size.x - textOffsetMin.x - textOffsetMax.x - tipSize.x)) + tipSize.x / 2;
                y = size.y + insets.y;
                a = 180;
                px = (x) / size.x;
                py = (y + tipSize.y / 4) / size.y;
                break;

            case Side.Right:
                x = size.x + insets.x;
                y = textOffsetMax.y + (tip.position * (size.y - textOffsetMin.y - textOffsetMax.y - tipSize.y)) + tipSize.y / 2;
                a = +90;
                px = (x + tipSize.x / 4) / size.x;
                py = (y) / size.y;
                break;

            case Side.Bottom:
                x = textOffsetMax.x + (tip.position * (size.x - textOffsetMin.x - textOffsetMax.x - tipSize.x)) + tipSize.x / 2;
                y = -insets.y;
                a = 0;
                px = (x) / size.x;
                py = (y - tipSize.y / 4) / size.y;
                break;

            default:
                return;
        }

        //set the proper things
        got_Tip.anchoredPosition = new Vector2(x, y);
        got_Tip.Rotate(Vector3.forward, a - this.tip.prevAngle);
        got_Background.pivot = new Vector2(px, py);

        tip.prevAngle = a;
        this.tip = tip;
    }

    public Tip GetTip()
    {
        return tip;
    }

    public Tip tip { get; private set; }

    
    /* Managing the SpeechBubble Background Color */

    public void SetBackgroundColor(Color color)
    {
        GetComponent<Image>().color = color;
        got_Tip.GetComponent<Image>().color = color;
    }

    public Color GetBackgroundColor()
    {
        return GetComponent<Image>().color;
    }
    
    public Color backgroundColor { get { return GetBackgroundColor(); } set { SetBackgroundColor(value); } }


    /* Managing the SpeechBubble Text/Foreground Color */

    public void SetTextColor(Color color)
    {
        goc_Text.color = color;
    }

    public Color GetTextColor()
    {
        return goc_Text.color;
    }

    public Color textColor { get { return GetTextColor(); } set { SetTextColor(value); } }

    public Color foregroundColor { get { return GetTextColor(); } set { SetTextColor(value); } }


    /* Anchoring to a World GameObject */

    private Anchor _anchor = null;

    public Anchor Anchor(GameObject anchoring)
    {
        if (this._anchor != null)
            Destroy(this._anchor);

        Anchor anchor = anchoring.AddComponent<Anchor>();
        anchor.anchored = this.gameObject;
        this._anchor = anchor;
        return anchor;
    }

    public GameObject anchor
    {
        get { return _anchor.gameObject; }
        set { Anchor(value); }
    }
}
