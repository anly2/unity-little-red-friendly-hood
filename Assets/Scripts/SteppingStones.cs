using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SteppingStones : MonoBehaviour {

    public GameObject river;
    public List<Transform> rockSlots;
    public List<GameObject> rockObjects;

    public float neighbourThreshold = 2f;

    [Range(0,1)]
    public float emergeChance = 0.15f;
    public Range emergedDuration = new Range(1, 2);
    public float submergeTime = 1f;


    private bool jumping = false;
    private bool overRiver = false;
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
        // Initialize river
        var riverAura = river.AddComponent<TriggerExtensions.Aura>();
        riverAura.shouldAffect = a => a.tag == "Player";
        riverAura.onEnter = a =>
        {
            overRiver = true;
            if (!jumping)
                Drown();
        };
        riverAura.onExit = a => overRiver = false;


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
            //Instantiate and position
            node.stone = Instantiate(rockObjects[Random.Range(0, rockObjects.Count)]);
            node.stone.transform.parent = rockSlots[node.index];
            node.stone.transform.position = node.stone.transform.parent.position;

            //Add a trigger collider and an aura for tracking purposes
            node.stone.GetComponent<Collider2D>().isTrigger = true; //## Feels weird to do this but...
            var aura = node.stone.AddComponent<TriggerExtensions.Aura>();
            aura.shouldAffect = a => a.tag == "Player";
            aura.onEnter = a =>
            {
                if (!IsStoneSubmerged(node))
                {
                    nodeUnderPlayer = node;
                    node.neighbours[Random.Range(0, node.neighbours.Length)].chance = 1;
                }
            };
            aura.onExit = a =>
            {
                if (nodeUnderPlayer != null && nodeUnderPlayer.index == node.index)
                    nodeUnderPlayer = null;
            };
        }

        GameObject stone = node.stone;

        stone.SetOpacity(0f);

        while (true)
        {
            float p = Random.Range(0f, 1f);
            if (node.chance > p)
            {
                node.chance = emergeChance; //reset any changes

                stone.FadeIn(submergeTime).Start(this);
                yield return new WaitForSeconds(submergeTime);

                float te = Random.Range(emergedDuration.min, emergedDuration.max);
                yield return new WaitForSeconds(te);

                stone.FadeOut(submergeTime).Start(this);
                yield return new WaitForSeconds(submergeTime);

                if (nodeUnderPlayer != null && nodeUnderPlayer.index == node.index)
                {
                    nodeUnderPlayer = null;

                    if (!jumping)
                        Drown();
                }
            }

            float ts = Random.Range(emergedDuration.min, emergedDuration.max);
            yield return new WaitForSeconds(ts);
        }
    }

    private bool IsStoneSubmerged(Node node)
    {
        return node.stone.GetOpacity() <= 0.1;
    }

    void JumpStart()
    {
        jumping = true;
    }

    void JumpLand()
    {
        jumping = false;

        if (!overRiver)
            return;
        
        if (nodeUnderPlayer == null)
            Drown();
    }


    const float drownCinematicDuration = 1f;
    public void Drown()
    {
        gameObject.SetActive(false);
        GameObject player = GameObject.FindWithTag("Player");

        player.MotionTo(player.transform.position
            + new Vector3(-2, 2, 0), drownCinematicDuration).Start();

        player.FadeOut(drownCinematicDuration)
            .Then(() => Die("You drowned!",
                    "Little Red Riding Hood %1 Who was swept by the waters")
            ).Start();
    }
    
    protected void Die(string message, params string[] gravestones)
    {
        //foreach (string engraving in gravestones)
        //    cemetery.AddGrave(FormatEngraving(engraving));

        Debug.LogError("Dead: " + message);
        //messageMenu.showMessage("<b><color=#ff2222><size=34>You are dead!</size></color></b>\n<size=24>" + message + "</size>");
        //enter("DEAD");
        
        new WaitForSeconds(2f).Then(() => Application.LoadLevel(Application.loadedLevel)).Start();
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