using UnityEngine;
using System.Collections;

public class MainQuest : Quest {

    private GameObject player;

	void Start()
    {
        name("Main");
        description("The Quest defining the main plot of the game.");
        startsActive(true);


        GameObject mother = GameObject.Find("Mother");

        state("S0")
            .onEnter(s =>
            {
                player = GameObject.FindWithTag("Player");
                GameObject spawn = GameObject.FindWithTag("Respawn");

                player.transform.position = spawn.transform.position;
            })

            .scene()
                .actor(mother)
                    .act(aq => mother.FadeIn(0.5f), 0.5f)
                    .delay(0.5f)
                    .say("Come, Little Red-Cap, here is a piece of cake and a bottle of wine; take them to your grandmother, she is ill and weak, and they will do her good.")
                    .say("Set out before it gets hot, and when you are going, walk nicely and quietly and do not run off the path, or you may fall and break the bottle, and then your grandmother will get nothing.")
                    .say("And when you go into her room, don't forget to say, \"Good morning\", and don't peep into every corner before you do it.")
                .actor(player)
                    .say("I will take great care.")
                .actor(mother)
                    .act(aq => mother.FadeOut(0.5f))
                .transition("S1");
    }
}
