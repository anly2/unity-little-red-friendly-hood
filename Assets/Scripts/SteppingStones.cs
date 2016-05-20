using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SteppingStones : MonoBehaviour {

    public List<Transform> rockSlots;
    public List<GameObject> rockObjects;

    public float neighbourThreshold = 2f;

    [Range(0,1)]
    public float emergeChance = 0.15f;
    public Range emergedDuration = new Range(1, 2);
    public float submergeTime = 1f;


    private Node nodeUnderPlayer;
    private Node[] nodes;

    private class Node
    {
        public int index;
        public float chance;
        public GameObject stone = null;
        public Node[] neighbours;
    }


    void Start() {
        Begin();
	}


    public void Begin() {
        // Initialize nodes
        nodes = new Node[rockSlots.Count];

        for (int i = 0; i < nodes.Length; i++)
        {
            Node node = new Node();
            node.index = i;
            node.chance = emergeChance;
            nodes[i] = node;
        }


        // Link Neighbours
        for (int i = 0; i < nodes.Length; i++)
            nodes[i].neighbours = GetNeighbourSlots(i);


        // Make them behave/act
        foreach (Node node in nodes)
            StartCoroutine(InitStone(node));
	}

    Node[] GetNeighbourSlots(int index)
    {
        Transform slot = rockSlots[index];
        List<Node> neighbours = new List<Node>();

        for (int i = 0; i < rockSlots.Count; i++)
        {
            if (i == index) continue;

            if (Vector3.Distance(rockSlots[i].position, slot.position) < neighbourThreshold)
                neighbours.Add(nodes[i]);
        }

        return neighbours.ToArray();
    }

    IEnumerator InitStone(Node node)
    {
        if (node.stone == null)
        {
            node.stone = Instantiate(rockObjects[Random.Range(0, rockObjects.Count)]);
            node.stone.transform.parent = rockSlots[node.index];
            node.stone.transform.position = node.stone.transform.parent.position;
        }

        GameObject stone = node.stone;

        stone.SetOpacity(0f);

        while (true)
        {
            float p = Random.Range(0f, 1f);
            if (node.chance > p)
            {
                stone.FadeIn(submergeTime).Start(this);
                yield return new WaitForSeconds(submergeTime);

                float te = Random.Range(emergedDuration.min, emergedDuration.max);
                yield return new WaitForSeconds(te);

                stone.FadeOut(submergeTime).Start(this);
                yield return new WaitForSeconds(submergeTime);

                if (nodeUnderPlayer != null && nodeUnderPlayer.index == node.index)
                    Drown();
            }

            float ts = Random.Range(emergedDuration.min, emergedDuration.max);
            yield return new WaitForSeconds(ts);
        }
    }


    public void Drown()
    {
        Debug.LogError("You have drowned");
    }
}


[System.Serializable]
public struct Range
{
    public float min;
    public float max;

    public Range(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}