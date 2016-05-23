using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WolfAI : MonoBehaviour {

    public int flowersToCollect = 5;
    public float waitThinking = 0.1f;

    private Transform wolf;
    private List<Transform> flowerSlots;
    
    void Start()
    {
        //## assumes there is only one Flower Challenge
        flowerSlots = GameObject.FindObjectOfType<FlowerChallenge>().flowerSlots;

        wolf = this.transform;

        Begin();
    }


    protected void Begin()
    {
        //#! disable the follower script #? here ?

        StartCoroutine(Collect());
    }

    protected IEnumerator Collect()
    {
        for (int i = 0; i < flowersToCollect; i++)
        {
            var closest = GetClosestFlower();
            float t = Vector3.Distance(wolf.position, closest.position) / wolf.gameObject.GetSpeed();
            wolf.gameObject.MotionTo(closest.position, t).Start(this);
            yield return new WaitForSeconds(t);

            yield return new WaitForSeconds(waitThinking);
        }
    }


    protected Transform GetClosestFlower()
    {
        float distance = float.MaxValue;
        Transform closest = null;

        foreach (Transform flower in GetActiveFlowers())
        {
            float d = Vector3.Distance(wolf.position, flower.position);
            if (d < distance)
            {
                distance = d;
                closest = flower.transform;
            }
        }

        return closest;
    }

    protected Transform[] GetActiveFlowers()
    {
        List<Transform> flowers = new List<Transform>();

        foreach (Transform slot in flowerSlots)
            foreach (Transform flower in slot)
                if (flower.gameObject.GetOpacity() >= (1 - float.Epsilon))
                    flowers.Add(flower);

        return flowers.ToArray();
    }
}
