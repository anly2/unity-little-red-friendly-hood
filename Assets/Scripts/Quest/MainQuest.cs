using UnityEngine;
using System.Collections;

public class MainQuest : Quest {

	internal protected MainQuest() : base("Main")
    {
        description("The Quest defining the main plot of the game.");
        startsActive(true);

        GameObject wolf = null; //#!
        int friendship = 0;

        state("S0")
            .onEnter((s) => { /* play Intro */ });

        state("S1").conversation().with(wolf)
            .they("What are you carrying under the apron?")
            .player("Yesterday was baking day, so I have cake and wine for...")
            .they("It smells delicious.")
            .player().choice()
                .option("Thank you.", "S2")
                .option("Thank you! Would you like some?",
                        (c)=>{ friendship++; c.getQuest().enter("S2"); });

        //defaults:
        //conv.activatedBy(Activators.InRange(player, wolf, 5f))
    }
}
