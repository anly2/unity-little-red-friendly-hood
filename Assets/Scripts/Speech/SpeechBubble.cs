using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeechBubble : MonoBehaviour {

    private RectTransform bg;
    private Text txt;
    private RectTransform tip;

	void Awake () {
        this.bg  = GetComponent<RectTransform>();
        this.txt = transform.Find("Text").GetComponent<Text>();
        this.tip = transform.Find("Tip") as RectTransform;
    }


    public void SetText(string text)
    {
        this.txt.text = text;

        float h = this.txt.preferredHeight;
        RectTransform r = this.txt.gameObject.transform as RectTransform;
        h -= r.offsetMax.y;
        h += r.offsetMin.y;

        bg.sizeDelta = new Vector2(bg.rect.width, h);
    }

    public string GetText()
    {
        return this.txt.text;
    }

    public string text { get { return GetText(); } set { SetText(value); } }



    public void SetBackgroundColor(Color color)
    {
        GetComponent<Image>().color = color;
        tip.GetComponent<Image>().color = color;
    }

    public Color GetBackgroundColor()
    {
        return GetComponent<Image>().color;
    }
    
    public Color backgroundColor { get { return GetBackgroundColor(); } set { SetBackgroundColor(value); } }


    public void SetTextColor(Color color)
    {
        txt.color = color;
    }

    public Color GetTextColor()
    {
        return txt.color;
    }

    public Color textColor { get { return GetTextColor(); } set { SetTextColor(value); } }

    public Color foregroundColor { get { return GetTextColor(); } set { SetTextColor(value); } }
}
