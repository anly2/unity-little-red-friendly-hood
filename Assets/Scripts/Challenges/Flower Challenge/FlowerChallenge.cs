using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FlowerChallenge : MonoBehaviour {

    public int startingFlowers = 4;
    public List<Transform> flowerSlots;
    public List<GameObject> flowerPrefabs;

    [HideInInspector]
    public GameObject[] participants;
    public TriggerExtensions.VetoingAction whenPickedUp;

    private Dictionary<GameObject, int> score = new Dictionary<GameObject, int>();
    private List<System.Action> observers = new List<System.Action>();


    public void Begin(params GameObject[] participants)
    {
        this.participants = participants;

        foreach (GameObject participant in participants)
            score[participant] = 0;

        for (int i=0; i<startingFlowers; i++)
            AddFlowerOnRandom();
    }


    void AddFlowerOnRandom()
    {
        GameObject flower = Instantiate(Rand(flowerPrefabs));

        Transform slot;
        do slot = Rand(flowerSlots);
        while (slot.childCount != 0);

        flower.transform.position = slot.position;
        flower.transform.parent = slot;

        var aura = flower.AddComponent<TriggerExtensions.Aura>();
        aura.shouldAffect = o => participants.Contains(o.gameObject);
        aura.onEnter = o =>
        {
            score[o.gameObject]++;

            FlowerPickedUp();

            if (whenPickedUp == null || whenPickedUp())
                AddFlowerOnRandom();

            flower.FadeOut(0.5f)
                .Then(() => Destroy(flower))
                .Start();
        };
    }


    T Rand<T>(List<T> arr)
    {
        return arr[Random.Range(0, arr.Count)];
    }


    public void AddObserver(System.Action observer)
    {
        observers.Add(observer);
    }

    public int GetScore(GameObject participant)
    {
        int s;

        if (!score.TryGetValue(participant, out s))
            return -1;

        return s;
    }

    void FlowerPickedUp()
    {
        foreach (var observer in observers)
            observer();
    }
}
