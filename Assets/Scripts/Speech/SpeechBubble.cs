using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeechBubble : MonoBehaviour {

    private RectTransform got_Background; //GameObject Transform
    private Text goc_Text; //GameObject Component
    private RectTransform got_Tip; //GameObject Transform

    void Awake () {
        this.got_Background  = GetComponent<RectTransform>();
        this.goc_Text = transform.Find("Text").GetComponent<Text>();
        this.got_Tip = transform.Find("Tip") as RectTransform;
    }


    public void SetText(string text)
    {
        this.goc_Text.text = text;

        float h = this.goc_Text.preferredHeight;
        RectTransform r = this.goc_Text.gameObject.transform as RectTransform;
        h -= r.offsetMax.y;
        h += r.offsetMin.y;

        got_Background.sizeDelta = new Vector2(got_Background.rect.width, h);
    }

    public string GetText()
    {
        return this.goc_Text.text;
    }

    public string text { get { return GetText(); } set { SetText(value); } }


    public enum Side { Top, Right, Bottom, Left }

    public struct Tip {
        public readonly float position;
        public readonly Side side;

        public Tip(float position, Side side)
        {
            this.position = position;
            this.side = side;
        }
    }
    
    public void SetTip(float position, Side side)
    {
        position = Mathf.Clamp01(position);
        tip = new Tip(position, side);

        //Constant?
        Vector2 insets = new Vector2(2, 2); //the insets of the background sprite

        //shorthands
        Vector2 size = got_Background.sizeDelta;
        Vector2 tipSize = got_Tip.sizeDelta;
        Vector2 textOffsetMin = (goc_Text.transform as RectTransform).offsetMin;
        Vector2 textOffsetMax = (goc_Text.transform as RectTransform).offsetMax * -1; //*-1 since its relative to the top right anchor, and we want it orianted towards the origin (lower left)

        //derivable values depending on tip side
        float x, y, a, px, py; //x y coords, z rot angle, pivot x% y%

        switch (side)
        {
            case Side.Left:
                x = insets.x;
                y = textOffsetMax.y + (position * tipSize.y) + ((1 - position) * (size.y - textOffsetMin.y - textOffsetMax.y));
                a = -90;
                px = (x - tipSize.x/2) / size.x;
                py = (y - tipSize.y/2) / size.y;
                break;

            case Side.Top:
                return;
                break;

            case Side.Right:
                x = size.x - insets.x;
                y = textOffsetMax.y + (position * tipSize.y) + ((1 - position) * (size.y - textOffsetMin.y - textOffsetMax.y));
                a = -90;
                px = (x + tipSize.x / 2) / size.x;
                py = (y - tipSize.y / 2) / size.y;
                break;

            case Side.Bottom:
                return;
                break;

            default:
                return;
        }

        got_Tip.anchoredPosition = new Vector2(x, y);
        got_Tip.Rotate(Vector3.forward, a);
        got_Background.pivot = new Vector2(px, py);
    }

    public Tip tip { get; private set; }



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
}
