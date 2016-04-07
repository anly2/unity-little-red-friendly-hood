using UnityEngine;
using System.Collections;


public class Sign : MonoBehaviour {

    [TextArea]
    public string text = null;

    private SpeechBubble popup;

    void Awake() {
        initPopup();
        initAura();
    }

    void initPopup()
    {
        Bounds bounds = GetBounds(gameObject);
        Vector3 popupPosition = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);

        Transform child = transform.Find("Popup");

        if (child != null)
        {
            child.SetParent(DialogueUI.canvas.transform, false);
            (child as RectTransform).anchoredPosition = Camera.main.WorldToScreenPoint(popupPosition);

            popup = child.GetComponent<SpeechBubble>();
            popup.SetTip(SpeechBubble.Side.Bottom, 0.5f);
        }
        else
        {
            popup = gameObject.Say("", -1, popupPosition, SpeechBubble.Side.Bottom, 0.5f);
        }

        popup.name = "Sign Popup";
        popup.anchor = gameObject;
        popup.gameObject.SetActive(false);
    }

    void initAura()
    {
        Bounds bounds = GetBounds(gameObject);
        float radius = Mathf.Max(0.5f, bounds.extents.x, bounds.extents.y) + 0.25f;

        var aura = gameObject.AddAura(radius,
            o => ShowPopup(),
            o => HidePopup(),
            GameObject.FindWithTag("Player"));

        aura.name = "Readable Range";
        aura.gameObject.transform.position = bounds.center;
	}

    private static Bounds GetBounds(GameObject gameObject)
    {
        var r = gameObject.GetComponent<Renderer>();
        if (r != null)
            return r.bounds;

        var c = gameObject.GetComponent<Collider>();
        if (c != null)
            return c.bounds;

        var c2 = gameObject.GetComponent<Collider2D>();
        if (c2 != null)
            return c2.bounds;

        return new Bounds(gameObject.transform.position, Vector3.zero);
    }


    public void ShowPopup()
    {
        if (popup.text != null)
            popup.text = this.text;

        popup.gameObject.SetActive(true);
    }

    public void HidePopup()
    {
        popup.gameObject.SetActive(false);
    }
}
