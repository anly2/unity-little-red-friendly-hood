using UnityEngine;

public class MainQuest : Quest {

    [Header("Involved actors")]
    public GameObject initialSpawn;
    public GameObject player;
    public GameObject mother;
    public GameObject wolf;
    public GameObject huntsman;

    [Header("Wood Whisperers")]
    public GameObject whisperer1;
    public GameObject whisperer2;
    public GameObject whisperer3;
    public GameObject whisperer4;
    public Color whisperBackgroundColor;
    public Color whisperForegroundColor;

    [Header("Map-specific areas")]
    public Cemetery cemetery;
    public FlowerChallenge flowerChallenge;
    public GameObject flowerPath;
    public GameObject grandmasHouse;
    public GameObject grandmaPath;
    public GameObject sideGlade;

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
                    .act(aq => {
                        float t = 1f, m = 0.25f, d = (t-m);
                        new Tween(3f, p => player.SetSpeed(m + d * p)).Start();
                    });


        System.Action<GameObject, string> whisper = (whisperer, text) =>
        {
            SpeechBubble bubble = null;
            var aura = whisperer.AddComponent<TriggerExtensions.Aura>();
            aura.shouldAffect = a => a.tag == "Player";

            aura.onEnter = o =>
            {
                bubble = whisperer.Say(text, -1, whisperer.transform.position);
                bubble.transform.SetAsLastSibling();
                bubble.SetBackgroundColor(whisperBackgroundColor);
                bubble.SetTextColor(whisperForegroundColor);
            };
            aura.onExit = o => {
                if (bubble != null)
                    Destroy(bubble.gameObject);
                Destroy(aura);
            };
        };

        state("S0")
            .scene().actor(null).act(aq =>
            {
                whisper(whisperer1, "Do not get anyone killed!");
                whisper(whisperer2, "Or anything!");
                whisper(whisperer3, "The woods see!");
                whisper(whisperer4, "The woods remember!");
            })
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
                        c => { friendshipWithWolf++; enter("S3"); });

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

        state("S4")
            .conversation().with(wolf)
                .they("Where does your grandmother live, Little Red-Cap?")
                .player().choice()
                    .option("Just past the Huntsman's house.",
                        c => { friendshipWithWolf--; enter("S5"); })
                    .option("A good quarter of a league farther on in the wood.", "S5")
                    .option("A good quarter of a league farther on in the wood. Her house stands under the three large oak-trees, the nut-trees are just below; you surely must know it.",
                        c => { friendshipWithWolf++; enter("S5"); });

        state("S5")
            .scene().onEnter(s =>
            {
                if (friendshipWithWolf < 0)
                    Die("Your caution roused the wolf and he ate you.",
                        "Little Red Ridding Hood %1 Who was a bit too cautious.");

                if (friendshipWithWolf == 0)
                    enter("SAmbush");

                if (friendshipWithWolf > 0)
                    enter("S6");
            });

        state("SAmbush")
            .scene().onEnter(s =>
            {
                Debug.LogWarning("Wolf ambushes you on the way back");
                Die("The wolf ambushed you on the way back");
            });

        state("S6")
            .scene()
            .activatedBy(Activators.Enters(player, flowerPath))
            .onEnter(s =>
            {
                if (friendshipWithWolf > 1)
                    enter("SF8");
                else
                    enter("S8");
            });

        state("S8")
            .conversation().with(wolf)
                .they().act(a => flowerChallenge.Begin(player)) //## logically not here, but should be here
                .they("See, Little Red-Cap, how pretty the flowers are about here — why do you not look round?")
                .they("I believe, too, that you do not hear how sweetly the little birds are singing.")
                .they("You walk gravely along as if you were going to school, while everything else out here in the wood is merry.")
                .player().think("Suppose I take grandmother a fresh nosegay; that would please her too. It is so early in the day that I shall still get there in good time.")
            .scene()
                .actor(wolf)
                    .unfollow()
                    //.act(a => flowerChallenge.Begin(player)) //## moved up
                    .act(a => wolf.SetSpeed(0.75f))
                    .move(() => wolf.transform.position - new Vector3(1.5f, 0))
                    .act(a => wolf.FadeOut(2f).Start())
                    .move(() => wolf.transform.position - new Vector3(3, 0), 2f)
            .scene()
                .activatedBy(Activators.Enters(player, grandmasHouse))
                .onEnter(s =>
                {
                    Die("<s>Your grandma</s> The wolf \n ate you!",
                        "Little Red Riding Hood %1 Who naively walked through the woods.",
                        "Granny %2 Who became the treat for her guest the wolf.");
                });

        state("SF8")
            .conversation().with(wolf)
                .they().act(a => flowerChallenge.Begin(player, wolf))
                .they("See, Little Red-Cap, how pretty the flowers are about here — would their freshness not please your grandmother?")
                .they("I dare say, too, that I can gather the most refreshing of them all, before you do.")
                .player().think("Suppose I take grandmother a fresh nosegay; that would please her too. It is so early in the day that I shall still get there in good time.")
                .they()
                    //.unfollow(player)
                    //#! wolf AI for flower game
                    // .act(a => flowerChallenge.Begin(player, wolf)) //## moved up
                    .act(a => hasReasonToThankWolf = true) //#! set by the outcome of the flower game
            .scene()
                .activatedBy(a => new WaitForSeconds(30f).Then(() => a()).Start()) //#! when challenge is complete
                .transition("SF9");

        state("SF9")
            .scene() //#! the whole Huntsman scene
            .transition("SFW11");

        state("SFW11")
            .conversation().with(wolf)
                .they("Let me escort you to your grandma's, dear girl.")
                .they().follow(player)
            .scene()
                .activatedBy(Activators.Enters(player, grandmaPath))
                .onEnter(s => enter(hasReasonToThankWolf ? "SFWT12" : "SFW12"));

        state("SFW12")
            .conversation().with(wolf)
                .they("I shall leave you now, dear. Goodbye!")
                .player("Goodbye, wolf!")
                .act(a =>
                    End("Your day goes by peacefully. \n But your grandma gets eaten the next day.",
                        "Grandmother %2 Who became the treat to her new neighbour the wolf."));

        state("SFWT12")
            .conversation().with(wolf)
                .they("I shall leave you now, dear. Goodbye!")
                .player("Allow me to thank you for the aid you have given me!")
                .player("Let me treat you to some wine.")
                .they("How lovely...")
            .scene()
                .actor(wolf)
                    // .unfollow() //#! uncomment when dragging the wolf is implemented
                    .act(a =>
                    {
                        Activators.Enters(wolf, sideGlade)(() =>
                        {
                            return true;
                        });
                    });



        state("DEAD").scene()
            .actor(null).act(aq => player.SetSpeed(0))
            .getScene().completeQuest();
    }


    protected void End(string message, params string[] gravestones)
    {
        Menus.Get<DeathMenu>().Die("<size=24>" + message + "</size>", gravestones);

        enter("DEAD");
    }

    protected void Die(string message, params string[] gravestones)
    {
        Menus.Get<DeathMenu>().Die(
            "<b><color=#ff2222><size=34>You are dead!</size></color></b>\n<size=24>" + message + "</size>",
            gravestones);

        enter("DEAD");
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
