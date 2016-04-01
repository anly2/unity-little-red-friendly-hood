using UnityEngine;
using System.Collections;

public class MainQuest : Quest {

    private GameObject player;

	void Start()
    {
        name("Main");
        description("The Quest defining the main plot of the game.");
        startsActive(true);


        player = GameObject.FindWithTag("Player");
        GameObject mother = GameObject.Find("Mother");
        GameObject wolf = GameObject.Find("Wolf");


        state("S0")
            .onEnter(s =>
            {
                GameObject spawn = GameObject.FindWithTag("Respawn");
                player.transform.position = spawn.transform.position;
            })

            .scene()
                .actor(mother)
                    .wait(1.5f)
                    .say("Come, Little Red-Cap, here is a piece of cake and a bottle of wine; take them to your grandmother, she is ill and weak, and they will do her good.")
                    .say("Set out before it gets hot, and when you are going, walk nicely and quietly and do not run off the path, or you may fall and break the bottle, and then your grandmother will get nothing.")
                    .say("And when you go into her room, don't forget to say, \"Good morning\", and don't peep into every corner before you do it.")
                .actor(player)
                    .say("I will take great care.")
                .actor(mother)
                    .act(aq => mother.FadeOut(0.5f))
            .scene()
                .activatedBy(Activators.InRange(player, wolf, 4f))
                .transition("S1");

        state("S1")
            .scene()
                .actor(wolf)
                    .act((aq) =>
                        aq.getQuest().StartCoroutine(
                            wolf.MotionTo(
                                Vector3.MoveTowards(
                                    wolf.transform.position,
                                    player.transform.position, 2f),
                                1f)), 1f)
                    .say("Hello");
    }
}
