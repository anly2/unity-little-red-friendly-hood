using UnityEngine;
using System.Collections.Generic;

public class SpriteRandomizer : MonoBehaviour {

    public List<Sprite> sprites;

    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Count)];
    }
}
