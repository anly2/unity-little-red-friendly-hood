using UnityEngine;
using System.Collections;

public class MainQuest : Quest {

    [Header("Involved actors")]
    public GameObject initialSpawn;
    public GameObject player;
    public GameObject mother;
    public GameObject wolf;
    public GameObject huntsman;

    [Header("Map-specific areas")]
    public Cemetery cemetery;

    //Private state//
    private int friendshipWithWolf;
    private bool hasReasonToThankWolf;


    void Start()
    {
        Build();
    }

    void Build()
    {
        name("Main");
        description("The Quest defining the main plot of the game.");
        startsActive(true);


        state("S_INIT")
            .onEnter(s =>
            {
                player.transform.position = initialSpawn.transform.position;
                enter("S0");
            });

        state("S0")
            .scene()
                .actor(mother)
                    .act(aq => mother.SetActive(true))
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
                .transition("S0 Wander");

        state("S0 Wander")
            .scene()
                .activatedBy(Activators.InRange(player, wolf, 4f))
                .transition("S1");

        state("S1")
            .scene()
                .actor(wolf)
                    .act(aq =>
                    {
                        float s = player.GetSpeed() / 2;
                        wolf.SetSpeed(s);
                        player.SetSpeed(s);
                    })
                    .follow(player)
                    .wait(2f)
                    .delay(3f, aq => {
                        player.MultiplySpeed(2);
                        wolf.MultiplySpeed(2);
                    })

            .conversation().with(wolf)
                .they("Good day, Little Red-Cap")
                .player("Thank you kindly, wolf.")
                .they("Whither away so early, Little Red-Cap?")
                .player().choice()
                    .option("To my grandmother's.", "S2")
                    .option("To the Huntsman's.",
                        c => Die("Your caution roused the wolf and he ate you.",
                            "Little Red Ridding Hood %1 Who was cautious perhaps too much."));

        state("S2")
            .conversation().with(wolf)
                .they("What have you got in that basket?")
                .player("Cake and wine; yesterday was baking-day, so poor sick grandmother is to have something good, to make her stronger.")
                .they("It does smell delicious.")
                .player().choice()
                    .option("Thank you.", "S4")
                    .option("Thank you! Would you like some?",
                        c => { friendshipWithWolf++; c.state("S3").enter(); });

        state("S3")
            .conversation().with(wolf)
                .they().act(aq =>
                {
                    var sb = wolf.Say("I would love a bite of you");
                    new WaitForSeconds(0.75f)
                        .Then(() => sb.text = "I would love a bite of your cake")
                        .Start(aq.getQuest());
                }, 4f)
                .player("Here is some.")
                .transition("S4");

        state("DEAD").scene()
            .actor(null).act(aq => Debug.LogError("You died."))
            .getScene().completeQuest();
    }

    protected void Die(string message, params string[] gravestones)
    {
        foreach (string engraving in gravestones)
            cemetery.AddGrave(FormatEngraving(engraving));

        Debug.Log(message);
        enter("DEAD");
    }


    string FormatEngraving(string engraving)
    {
        int currentYear = cemetery.currentYear + Random.Range((int)60, 100);
        cemetery.currentYear = currentYear;

        int lrrhAge = currentYear - Random.Range((int) 10, 15);
        int grannyAge = currentYear - Random.Range((int) 50, 60);
        return "<b>" + engraving
            .Replace("%1", "</b>\n<size=14>" + lrrhAge + " - " + currentYear + " AD</size>\n<i>")
            .Replace("%2", "</b>\n<size=14>" + grannyAge + " - " + currentYear + " AD</size>\n<i>")
            + "</i>";
    }


    /* Saving and Loading */

    public override void Save(Data data, GameState context)
    {
        base.Save(data, context);

        data["friendship with wolf"] = friendshipWithWolf;
        data["has reason to thank wolf"] = hasReasonToThankWolf;

        data["wolf is following player"] = WolfIsFollowing();
        data["mother is active in scene"] = mother.activeInHierarchy;

        data["position of player"] = player.transform.position.serializable();
        data["position of wolf"] = wolf.transform.position.serializable();
        //data["position of mother"] = mother.transform.position.serializable(); //obsolete in current plot
        //data["position of huntsman"] = huntsman.transform.position.serializable(); //not yet implemented
    }

    public override void Load(Data data, GameState context)
    {
        base.Load(data, context);

        if (completed) return;


        // Restore private state variables
        friendshipWithWolf = data.Get("friendship with wolf", 0);
        hasReasonToThankWolf = data.Get("has reason to thank wolf", false);


        // Restore mother actor state
        mother.SetActive(data.Get("mother is active in scene", false));

        // Restore wolf's following behaviour
        bool wolfShouldFollow = data.Get("wolf is following player", false);
        bool wolfIsFollowing = WolfIsFollowing();

        if (wolfIsFollowing != wolfShouldFollow)
        {
            var prevState = activeState;
            activeState = null; //avoid triggering "onLeave"

            var setupState = state().description("Setup state, to restore saved effects");
            var setupScene = setupState.scene().activatedBy(nothing => { });

            if (wolfShouldFollow)
                setupScene.actor(wolf).follow(player);
            else
                setupScene.actor(wolf).unfollow();

            setupState.enter();
            setupScene.enter();

            activeState = prevState; //avoid triggering "onEnter"
        }


        // Restore positions of actors
        player.transform.position = data.Get("position of player", player.transform.position.serializable()).unwarp();
        wolf.transform.position = data.Get("position of wolf", wolf.transform.position.serializable()).unwarp();
        //mother.transform.position = data.Get("position of mother", mother.transform.position.serializable()).unwrap();
        //huntsman.transform.position = data.Get("position of huntsman", huntsman.transform.position.serializable()).unwrap();
    }

    private bool WolfIsFollowing()
    {
        return (wolf.GetComponent<ActorQuery.Follower>() != null);
    }
}
