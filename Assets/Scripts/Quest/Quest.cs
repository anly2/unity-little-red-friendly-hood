using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void ActivationDelegate();
public delegate void Activator(ActivationDelegate activateCB);

public class Quest : MonoBehaviour {
    private string _name;
    private string _description;
    private Activator activator;
    private State activeState;
    private State initialState; //the first State with no Activator; managed by the inner class State
    private Dictionary<string, State> states; //managed by the inner class State

    /* Constructors */

    public Quest() { }

    public Quest(string name)
    {
        this.name(name);
    }

    public Quest(string name, string description)
    {
        this.name(name);
        this.description(description);
    }


    /* Chainable Accessors */

    public new Quest name(string name)
    {
        this._name = name;
        return this;
    }

    public Quest description(string description)
    {
        this._description = description;
        return this;
    }

    public Quest activate()
    {
        if (initialState != null && activeState == null)
            initialState.enter();

        return this;
    }

    public Quest startsActive(bool flag)
    {
        if (!flag)
            return activatedBy(null);

        return activatedBy(Activators.WorldBegin);
    }

    public Quest activatedBy(Activator activator)
    {
        this.activator = activator;
        activator(() => this.activate());
        return this;
    }


    /* Sub-Queries */

    public virtual State state()
    {
        return new State(this);
    }

    public virtual State state(string name)
    {
        return this.state().name(name);
    }


    /* State Inner Class */

    public class State
    {
        public delegate void Fragment(State state);

        private Quest parent;
        private string _name;
        private string _description;
        private Fragment _before;
        private Fragment _after;
        private Activator activator;
        private List<Conversation> conversations = new List<Conversation>(); //managed by the inner class Conversation


        /* Constructors */

        public State(Quest quest)
        {
            this.parent = quest;
            parent.states.Add(this.ToString(), this);

            if (parent.initialState == null || parent.initialState.activator != null)
                parent.initialState = this;
        }


        /* Chainable Accessors */

        public State name(string name)
        {
            parent.states.Remove(this.ToString());
            this._name = name;
            parent.states.Add(name, this);
            return this;
        }

        public State description(string description)
        {
            this._description = description;
            return this;
        }

        public State onEnter(Fragment action)
        {
            this._before = action;
            return this;
        }

        public State onLeave(Fragment action)
        {
            this._after = action;
            return this;
        }

        public State activatedBy(Activator activator)
        {
            this.activator = activator;
            activator(() => this.enter());
            return this;
        }


        public State enter()
        {
            parent.activeState.leave();
            parent.activeState = this;

            if (_before != null)
                _before(this);

            return this;
        }

        public State leave()
        {
            if (_after != null)
                _after(this);

            parent.activeState = null;
            return this;
        }


        /* Sub-Queries */

        public virtual UnspecifiedConversation conversation()
        {
            return new UnspecifiedConversation(this);
        }

        public virtual PlayerConversation conversation(GameObject actor)
        { //with player
            return this.conversation().with(actor);
        }

        public virtual Conversation conversation(GameObject actor1, GameObject actor2)
        {
            return this.conversation().between(actor1, actor2);
        }


        /* Upwards chainability */

        public Quest up()
        {
            return parent;
        }

        public Quest getQuest()
        {
            return up();
        }


        public virtual State state()
        {
            return parent.state();
        }

        public virtual State state(string name)
        {
            return parent.state(name);
        }


        /* Conversation Inner Class */

        public class UnspecifiedConversation
        {
            private State parent;
            
            public UnspecifiedConversation(State state)
            {
                this.parent = state;
            }

            public PlayerConversation with(GameObject actor)
            {
                return new PlayerConversation(parent, actor);
            }

            public Conversation between(GameObject actor1, GameObject actor2)
            {
                return new Conversation(parent, actor1, actor2);
            }
        }

        public class Conversation
        {
            public delegate void Fragment(Conversation conversation);
            public delegate void ContinueCallback();
            public delegate float Action(Conversation conversation, ContinueCallback continueCB);

            private State parent;
            private ActorQuery actorQuery1;
            private ActorQuery actorQuery2;
            private Fragment _before;
            private Fragment _after;
            private List<Action> actions = new List<Action>();
            private Activator activator;


            /* Constructors */

            internal Conversation(State state, GameObject actor1, GameObject actor2)
            {
                this.parent = state;
                parent.conversations.Add(this);

                this.actorQuery1 = actorQuery(actor1);
                this.actorQuery2 = actorQuery(actor2);
            }

            protected virtual ActorQuery actorQuery(GameObject actor)
            {
                return new ActorQuery(this, actor);
            }


            /* Chainable Accessors */

            public Conversation onEnter(Fragment action)
            {
                this._before = action;
                return this;
            }

            public Conversation onLeave(Fragment action)
            {
                this._after = action;
                return this;
            }

            public Conversation activatedBy(Activator activator)
            {
                this.activator = activator;
                activator(() => this.enter());
                return this;
            }


            public Conversation enter()
            {
                if (_before != null)
                    _before(this);

                getQuest().StartCoroutine(ExecuteActions());
                return this;
            }

            IEnumerator ExecuteActions()
            {
                foreach (Action action in actions)
                {
                    bool completed = false;
                    float actionDuration = action(this, () => completed = true);

                    if (actionDuration < 0) {
                        while (!completed)
                            yield return null;
                    }

                    if (actionDuration > Mathf.Epsilon) {
                        float completionTime = Time.time + actionDuration;
                        while (!completed && completionTime > Time.time)
                            yield return null;

                        //// Same as:
                        //yield return new WaitForSeconds(actionDuration)
                        //// This way, though, the 'completeCB' is able to interrupt the wait
                    }
                }
            }

            public Conversation leave()
            {
                if (_after != null)
                    _after(this);

                return this;
            }


            /* Sub-Queries */

            public virtual ActorQuery participant1()
            {
                return actorQuery1;
            }

            public virtual ActorQuery participant2()
            {
                return actorQuery2;
            }


            public virtual ActorQuery participant1(string speech)
            {
                return participant1().say(speech);
            }

            public virtual ActorQuery participant2(string speech)
            {
                return participant2().say(speech);
            }
            

            /* Upwards chainability */

            public State up()
            {
                return parent;
            }

            public Quest getQuest()
            {
                return up().up();
            }

            public State getState()
            {
                return up();
            }


            public virtual State state()
            {
                return parent.state();
            }

            public virtual State state(string name)
            {
                return parent.state(name);
            }

            public virtual UnspecifiedConversation conversation()
            {
                return parent.conversation();
            }

            public virtual PlayerConversation conversation(GameObject actor)
            {
                return parent.conversation(actor);
            }

            public virtual Conversation conversation(GameObject actor1, GameObject actor2)
            {
                return parent.conversation(actor1, actor2);
            }


            /* ActorQuery Inner Class */

            public class ActorQuery
            {
                public delegate void Fragment(ActorQuery actorQuery);

                private Conversation parent;
                private GameObject actor;


                /* Constructors */

                internal ActorQuery(Conversation conversation, GameObject actor)
                {
                    this.parent = conversation;
                    this.actor = actor;
                }


                /* Chainable Accessors */

                public ActorQuery say(string text)
                {
                    float duration = calculateSpeechDuration(text);
                    return say(text, duration);
                }

                public ActorQuery say(string text, float duration)
                {
                    parent.actions.Add((conversation, continueCB) =>
                    {
                        actor.Say(text);
                        //#! after duration, clear Say
                        return duration;
                    });
                    return this;
                }


                public ActorQuery delay(float duration)
                {
                    parent.actions.Add((conversation, continueCB) =>
                    {
                        return duration;
                    });
                    return this;
                }

                public ActorQuery act(Fragment action)
                {
                    parent.actions.Add((conversation, continueCB) =>
                    {
                        action(this);
                        return 0;
                    });
                    return this;
                }


                public ActorQuery delay(float delay, Fragment action)
                {
                    return this.delay(delay).act(action);
                }

                public ActorQuery act(Fragment action, float duration)
                {
                    return act(action).delay(duration);
                }


                private static float timePerChar = 0.1f;

                public static float calculateSpeechDuration(string text)
                {
                    return text.Length * timePerChar;
                }


                /* Upwards chainability */

                public Conversation up()
                {
                    return parent;
                }

                public Conversation getConversation()
                {
                    return up();
                }

                public State getState()
                {
                    return up().up();
                }

                public Quest getQuest()
                {
                    return up().up().up();
                }


                public virtual State state()
                {
                    return parent.state();
                }

                public virtual State state(string name)
                {
                    return parent.state(name);
                }

                public virtual UnspecifiedConversation conversation()
                {
                    return parent.conversation();
                }

                public virtual PlayerConversation conversation(GameObject actor)
                {
                    return parent.conversation(actor);
                }

                public virtual Conversation conversation(GameObject actor1, GameObject actor2)
                {
                    return parent.conversation(actor1, actor2);
                }

                public virtual ActorQuery participant1()
                {
                    return parent.participant1();
                }

                public virtual ActorQuery participant2()
                {
                    return parent.participant2();
                }

                /*
                public virtual PlayerQuery player()
                {
                    return parent.player();
                }

                public virtual ActorQuery they()
                {
                    return parent.they();
                }
                */
            }
        }

        public class PlayerConversation : Conversation
        {
            internal PlayerConversation(State state, GameObject actor)
                : base(state, GameObject.FindWithTag("Player"), actor) { }

            protected override Conversation.ActorQuery actorQuery(GameObject actor)
            {
                if (actor.tag == "Player")
                    return new PlayerQuery(this, actor);

                return new ActorQuery(this, actor);
            }


            public PlayerQuery player()
            {
                return participant1() as PlayerQuery;
            }

            public ActorQuery they()
            {
                return participant2() as ActorQuery;
            }


            public PlayerQuery player(string speech)
            {
                return player().say(speech);
            }

            public ActorQuery they(string speech)
            {
                return they().say(speech);
            }


            /* Inner Classes */

            public new class ActorQuery : Conversation.ActorQuery
            {
                internal ActorQuery(Conversation conversation, GameObject actor)
                    : base(conversation, actor) { }


                /* Chainable Accessors */

                public new ActorQuery say(string text)
                {
                    base.say(text);
                    return this;
                }

                public new ActorQuery say(string text, float duration)
                {
                    base.say(text, duration);
                    return this;
                }


                public new ActorQuery delay(float duration)
                {
                    base.delay(duration);
                    return this;
                }

                public new ActorQuery act(Fragment action)
                {
                    base.act(action);
                    return this;
                }


                public new ActorQuery delay(float delay, Fragment action)
                {
                    base.delay(delay, action);
                    return this;
                }

                public new ActorQuery act(Fragment action, float duration)
                {
                    base.act(action, duration);
                    return this;
                }


                /* Upwards chainability */

                public new PlayerConversation getConversation()
                {
                    return getConversation() as PlayerConversation;
                }


                public virtual PlayerQuery player()
                {
                    return getConversation().player();
                }

                public virtual PlayerQuery player(string text)
                {
                    return getConversation().player(text);
                }


                public virtual ActorQuery they()
                {
                    return getConversation().they();
                }

                public virtual ActorQuery they(string text)
                {
                    return getConversation().they(text);
                }
            }

            public class PlayerQuery : ActorQuery
            {
                internal PlayerQuery(Conversation conversation, GameObject actor)
                    : base(conversation, actor) { }


                public new PlayerQuery say(string text)
                {
                    base.say(text);
                    return this;
                }

                public new PlayerQuery say(string text, float duration)
                {
                    base.say(text, duration);
                    return this;
                }


                public new PlayerQuery delay(float duration)
                {
                    base.delay(duration);
                    return this;
                }

                public new PlayerQuery act(Fragment action)
                {
                    base.act(action);
                    return this;
                }


                public new PlayerQuery delay(float delay, Fragment action)
                {
                    base.delay(delay, action);
                    return this;
                }

                public new PlayerQuery act(Fragment action, float duration)
                {
                    base.act(action, duration);
                    return this;
                }


            }
        }
    }


    public static class Activators
    {
        public static Activator WorldBegin = (activateCB) => {
            MonoBehaviour m = Instantiate(new GameObject()).AddComponent<MonoBehaviour>();
            m.StartCoroutine(
                new WaitForEndOfFrame()
                .Then(() => activateCB())
                .Then(() => Destroy(m)));
        };
    }
}
