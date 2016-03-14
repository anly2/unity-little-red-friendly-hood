using UnityEngine;
using System.Collections;

public class Speaker : MonoBehaviour {

    [Tooltip("An identifier as recognised by the StoryManager")]
    public string speakerName; //#! will probably change with the Quest/Dialogue Manager/Engine

    [Tooltip("The pivot point at which the speech bubble tip will appear.\nThe values are interpreted as fractions of the game object's extents (half the size). In other words (assuming pivot==center), (0,0) is the center, (1,1) is the top right corner, (-1,-1) is the bottom left corner.")]
    public Vector2 speechPivot;
}
