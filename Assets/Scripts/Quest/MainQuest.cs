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

        int friendship = 0;


        state("S0")
            .onEnter(s =>
            {
                GameObject spawn = GameObject.FindWithTag("Respawn");
                player.transform.position = spawn.transform.position;
            })

            .scene()
                .actor(mother)
                    .act(aq => player.SetSpeed(0.1f))
                    .wait(1.5f)
                    .act(aq => player.SetSpeed(0.25f))
                    .say("Come, Little Red-Cap, here is a piece of cake and a bottle of wine; take them to your grandmother, she is ill and weak, and they will do her good.")
                    .say("Set out before it gets hot, and when you are going, walk nicely and quietly and do not run off the path, or you may fall and break the bottle, and then your grandmother will get nothing.")
                    .say("And when you go into her room, don't forget to say, \"Good morning\", and don't peep into every corner before you do it.")
                .actor(player)
                    .say("I will take great care.")
                    .act(aq => mother.FadeOut(0.5f).Then(() => mother.SetActive(false)).Start(), 0.5f)
                    .act(aq => player.SetSpeed(1.5f))
            .scene()
                .activatedBy(Activators.InRange(player, wolf, 4f))
                .transition("S1");

        state("S1")
            .scene()
                .actor(wolf)
                    .act(aq =>
                    {
                        float s = player.GetSpeed();
                        player.SetSpeed(s / 2);
                    })
                    .follow(player)
                    .wait(2f)
                    .delay(8f, aq => player.MultiplySpeed(2))

            .conversation().with(wolf)
                .they("Good day, Little Red-Cap")
                .player("Thank you kindly, wolf.")
                .they("Whither away so early, Little Red-Cap?")
                .player().choice()
                    .option("To my grandmother's.", "S2")
                    .option("To the Huntsman's.",
                        c => Die("Your caution roused the wolf and he ate you.",
                            "Little Red Ridding Hood $ Who was cautious perhaps too much."));

        state("S2")
            .conversation().with(wolf)
                .they("What have you got in that basket?")
                .player("Cake and wine; yesterday was baking-day, so poor sick grandmother is to have something good, to make her stronger.")
                .they("It does smell delicious.")
                .player().choice()
                    .option("Thank you.", "S4")
                    .option("Thank you! Would you like some?",
                        c => { friendship++; c.state("S3").enter(); });

        state("S3")
            .conversation().with(wolf)
                .they().act(aq =>
                {
                    var sb = wolf.Say("I would love a bite of you");
                    new WaitForSeconds(0.5f)
                        .Then(() => sb.text = "I would love a bite of your cake")
                        .Start(aq.getQuest());
                }, 4f)
                .player("Here is some.")
                .transition("S4");

        state("DEAD")
            .scene().completeQuest();
    }

    protected void Die(string message, params string[] tombstones)
    {
        enter("DEAD");
        Debug.LogError("DEAD: " + message);
    }
}
