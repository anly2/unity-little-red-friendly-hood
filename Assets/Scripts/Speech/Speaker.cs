using UnityEngine;
using System.Collections;

public class Speaker : MonoBehaviour {

    [Tooltip("The offset by which the renderer bounds will be shifted when doing Speech Bubble positioning.")]
    public Vector3 speechBorderOffset;
    
    [Tooltip("The coefficients by which the renderer bounds will be scaled when doind Speech Bubble positioning.")]
    public Vector3 speechBorderSizeModifier;
}
