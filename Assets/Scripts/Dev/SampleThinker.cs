using UnityEngine;
using System.Collections;

public class SampleThinker : MonoBehaviour
{

    public string thought;

    void Start()
    {
        gameObject.Think(thought);
    }
}
