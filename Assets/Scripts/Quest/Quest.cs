using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public delegate bool ActivationDelegate();
public delegate void Activator(ActivationDelegate activateCB);

public class Quest : MonoBehaviour {
    private string _name;
    private string _description;
    //private Activator activator;
    private State activeState;
    private State initialState; //the first State with no Activator; managed by the inner class State
    private Dictionary<string, State> states = new Dictionary<string, State>(); //managed by the inner class State


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


    /* Accessors */

    public string getName()
    {
        return this._name;
    }

    public string getDescription()
    {
        return this._description;
    }


    public State getState(string name)
    {
        State s;
        return (states.TryGetValue(name, out s)) ? s : null;
    }

    public Quest enter(string stateName)
    {
        getState(stateName).enter();
        return this;
    }

    public Quest enter(State state)
    {
        state.enter();
        return this;
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
        //this.activator = activator;
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
        State s = getState(name);

        if (s != null)
            return s;

        return this.state().name(name);
    }


    /** Inner Classes **/

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
        internal List<AbstractScene> scenes = new List<AbstractScene>(); //managed by the inner class Scene


        /* Constructors */

        internal State(Quest quest)
        {
            this.parent = quest;
            parent.states.Add(this.ToString(), this);

            if (parent.initialState == null || parent.initialState.activator != null)
                parent.initialState = this;
        }


        public bool IsActive { get { return parent.activeState == this; } }


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
            activator(() => { this.enter(); return true; });
            return this;
        }


        public State enter()
        {
            if (parent.activeState != null)
                parent.activeState.leave();

            parent.activeState = this;

            if (_before != null)
                _before(this);

            getQuest().StartCoroutine(PlayScenes());

            return this;
        }

        IEnumerator PlayScenes()
        {
            foreach (AbstractScene scene in scenes)
            {
                if (scene.HasActivator)
                    continue;

                IEnumerator act = scene.Play();
                while (act.MoveNext())
                    yield return act.Current;
            }
        }

        public State leave()
        {
            if (_after != null)
                _after(this);

            parent.activeState = null;
            return this;
        }


        /* Sub-Queries */

        public virtual Scene scene()
        {
            return new Scene(this);
        }

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
    }


    /* Abstract Scene Inner Class */

    public abstract class AbstractScene
    {
        public delegate void Fragment(AbstractScene conversation);
        public delegate IEnumerator Action();

        private State parent;
        private Fragment _before;
        private Fragment _after;
        private Activator activator;
        internal List<Action> actions = new List<Action>(); //managed by the ActorQueries


        /* Constructors */

        internal AbstractScene(State state)
        {
            this.parent = state;
            parent.scenes.Add(this);
        }
        

        /* Accessors */

        public bool HasActivator { get { return activator != null; } }


        /* Chainable Accessors */

        public AbstractScene onEnter(Fragment action)
        {
            this._before = action;
            return this;
        }

        public AbstractScene onLeave(Fragment action)
        {
            this._after = action;
            return this;
        }

        public AbstractScene activatedBy(Activator activator)
        {
            this.activator = activator;
            activator(() =>
            {
                try {
                    this.enter();
                    return true;
                }
                catch (InvalidOperationException) {
                    return false;
                }
            });
            return this;
        }


        public AbstractScene enter()
        {
            if (!parent.IsActive)
                throw new InvalidOperationException("The Quest State that this scene belongs to is not active!");

            getQuest().StartCoroutine(Play());
            return this;
        }

        public IEnumerator Play()
        {
            if (_before != null)
                _before(this);

            foreach (Action action in actions)
            {
                IEnumerator act = action();

                while (act.MoveNext())
                    yield return act.Current;
            }

            leave();
        }

        public AbstractScene leave()
        {
            if (_after != null)
                _after(this);

            return this;
        }


        public State transition(State state)
        {
            actions.Add(() => _().Then(() => state.enter()));
            return state;
        }

        public State transition(string stateName)
        {
            actions.Add(() => _().Then(() => getQuest().getState(stateName).enter()));
            return state(stateName);
        }

        private IEnumerator _() { yield break; }


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

        public virtual Scene scene()
        {
            return parent.scene();
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
    }


    /* ActorQuery Inner Class */

    public class ActorQuery
    {
        public delegate void Fragment(ActorQuery actorQuery);

        protected AbstractScene parent;
        protected GameObject _actor;


        /* Constructors */

        internal ActorQuery(AbstractScene scene, GameObject actor)
        {
            this.parent = scene;
            this._actor = actor;
        }


        /* Chainable Accessors */

        public ActorQuery say(string text)
        {
            float duration = text.EstimateReadTime();
            return say(text, duration);
        }

        public ActorQuery say(string text, float duration)
        {
            parent.actions.Add(() => _say(text, duration));
            return this;
        }

        private IEnumerator _say(string text, float duration)
        {
            SpeechBubble speechBubble = _actor.Say(text, duration);
            yield return new WaitForSeconds(duration);
        }


        public ActorQuery think(string text)
        {
            float duration = text.EstimateReadTime();
            return think(text, duration);
        }

        public ActorQuery think(string text, float duration)
        {
            parent.actions.Add(() => _think(text, duration));
            return this;
        }

        private IEnumerator _think(string text, float duration)
        {
            SpeechBubble speechBubble = _actor.Think(text, duration);
            yield return new WaitForSeconds(duration);
        }


        public ActorQuery wait(float time)
        {
            return delay(time);
        }

        public ActorQuery delay(float duration)
        {
            parent.actions.Add(() => _delay(duration));
            return this;
        }

        private IEnumerator _delay(float duration)
        {
            yield return new WaitForSeconds(duration);
        }


        public ActorQuery act(Fragment action, float duration = 0f)
        {
            parent.actions.Add(() => _act(action));

            if (duration > 0)
                wait(duration);

            return this;
        }

        private IEnumerator _act(Fragment action)
        {
            action(this);
            yield break;
        }


        public ActorQuery delay(float delay, Fragment action)
        {
            parent.actions.Add(() =>
            {
                return new WaitForSeconds(delay)
                    .Then(() => action(this));
            });

            return this;
        }


        public ActorQuery move(Vector3 destination, float? travelTime = null)
        {
            return move(() => destination, travelTime);
        }

        public ActorQuery move(GameObject target, float? travelTime = null)
        {
            return move(() => target.transform.position, travelTime);
        }

        public ActorQuery move(Func<Vector3> destinationSupplier, float? travelTime = null)
        {
            parent.actions.Add(() => {
                Vector3 destination = destinationSupplier();
                float duration = travelTime ?? _actor.EstimateTravelTime(destination);
                return _move(destination, duration);
            });
            return this;
        }

        private IEnumerator _move(Vector3 destination, float duration)
        {
            return _actor.MotionTo(destination, duration);
        }


        public delegate void UnfollowCallback();
        public delegate void UnfollowActivator(UnfollowCallback unfollowCallback);

        public ActorQuery follow(GameObject actor, bool displaceTarget = true)
        {
            UnfollowCallback unfollow;
            follow(actor, out unfollow, displaceTarget);

            return this;
        }

        public ActorQuery follow(GameObject actor, UnfollowActivator unfollowActivator, bool displaceTarget = true)
        {
            UnfollowCallback unfollowCB;
            follow(actor, out unfollowCB, displaceTarget);

            unfollowActivator(unfollowCB);

            return this;
        }

        public ActorQuery follow(GameObject actor, out UnfollowCallback unfollowCallback, bool displaceTarget = true)
        {
            GameObject target = null;
            Follower follower = null;

            act(aq =>
            {
                target = new GameObject("Follower Target");
                target.transform.position = actor.transform.position;
                target.transform.parent = actor.transform;

                follower = _actor.AddComponent<Follower>();
                follower.target = target;


                if (displaceTarget)
                {
                    float w = 0;

                    Renderer r = _actor.GetComponent<Renderer>();
                    if (r != null)
                        w = r.bounds.size.x;

                    Collider2D c2 = _actor.GetComponent<Collider2D>();
                    if (c2 != null)
                        w = c2.bounds.size.x;

                    Collider c = _actor.GetComponent<Collider>();
                    if (c != null)
                        w = c.bounds.size.x;


                    target.transform.Translate(new Vector3(w, 0, 0));
                }
            });

            unfollowCallback = () =>
            {
                if (target == null)
                    return;

                Destroy(target);
                Destroy(follower);
            };

            return this;
        }

        public class Follower : MonoBehaviour
        {
            public GameObject target;

            void Update()
            {
                if (target == null)
                    return;

                this.gameObject.transform.position = Vector3.MoveTowards(
                    transform.position, target.transform.position,
                    gameObject.GetSpeed() * Time.deltaTime);
            }
        }


        /* Upwards chainability */

        public AbstractScene up()
        {
            return parent;
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

        public virtual Scene scene()
        {
            return parent.scene();
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
        
        public virtual State transition(State state)
        {
            return parent.transition(state);
        }

        public virtual State transition(string stateName)
        {
            return parent.transition(stateName);
        }
    }


    /* Scene Inner Class */

    public class Scene : AbstractScene
    {
        internal Scene(State state) : base(state) { }


        /* Override Chainable Accessors */

        public new Scene onEnter(Fragment action)
        {
            base.onEnter(action);
            return this;
        }

        public new Scene onLeave(Fragment action)
        {
            base.onLeave(action);
            return this;
        }

        public new Scene activatedBy(Activator activator)
        {
            base.activatedBy(activator);
            return this;
        }


        public new Scene enter()
        {
            base.enter();
            return this;
        }

        public new Scene leave()
        {
            base.leave();
            return this;
        }
        

        /* Sub-Queries */

        public virtual ActorQuery actor(GameObject actor)
        {
            return new ActorQuery(this, actor);
        }


        /* ActorQuery Inner Class */

        public class ActorQuery : Quest.ActorQuery
        {
            /* Constructors */

            internal ActorQuery(Scene scene, GameObject actor)
                : base(scene, actor) {}


            /* Override Chainable Accessors */

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


            public new ActorQuery think(string text)
            {
                base.think(text);
                return this;
            }

            public new ActorQuery think(string text, float duration)
            {
                base.think(text, duration);
                return this;
            }


            public new ActorQuery wait(float time)
            {
                base.wait(time);
                return this;
            }

            public new ActorQuery delay(float duration)
            {
                base.delay(duration);
                return this;
            }


            public new ActorQuery act(Fragment action, float duration = 0f)
            {
                base.act(action, duration);
                return this;
            }

            public new ActorQuery delay(float delay, Fragment action)
            {
                base.delay(delay, action);
                return this;
            }


            public new ActorQuery move(GameObject target, float? travelTime = null)
            {
                base.move(target, travelTime);
                return this;
            }

            public new ActorQuery move(Vector3 destination, float? travelTime = null)
            {
                base.move(destination, travelTime);
                return this;
            }

            public new ActorQuery move(Func<Vector3> destinationSupplier, float? travelTime = null)
            {
                base.move(destinationSupplier, travelTime);
                return this;
            }


            /* Upwards chainability */

            public new virtual Scene up()
            {
                return base.up() as Scene;
            }

            public virtual Scene getScene()
            {
                return up();
            }


            public virtual ActorQuery actor(GameObject actor)
            {
                return getScene().actor(actor);
            }
        }
    }


    /* Conversation Inner Classes */
    
    public class UnspecifiedConversation
    {
        private State parent;

        internal UnspecifiedConversation(State state)
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

    public class Conversation : AbstractScene
    {
        private ActorQuery actorQuery1;
        private ActorQuery actorQuery2;

        internal Conversation(State state, GameObject actor1, GameObject actor2)
            : base(state)
        {

            this.actorQuery1 = actorQuery(actor1);
            this.actorQuery2 = actorQuery(actor2);
        }

        protected virtual ActorQuery actorQuery(GameObject actor)
        {
            return new ActorQuery(this, actor);
        }


        /* Override Chainable Accessors */

        public new Conversation onEnter(Fragment action)
        {
            base.onEnter(action);
            return this;
        }

        public new Conversation onLeave(Fragment action)
        {
            base.onLeave(action);
            return this;
        }

        public new Conversation activatedBy(Activator activator)
        {
            base.activatedBy(activator);
            return this;
        }


        public new Conversation enter()
        {
            base.enter();
            return this;
        }
        
        public new Conversation leave()
        {
            base.leave();
            return this;
        }
        

        /* Sub-Queries */

        public virtual ActorQuery participant1()
        {
            return this.actorQuery1;
        }

        public virtual ActorQuery participant2()
        {
            return this.actorQuery2;
        }

        public virtual ActorQuery participant1(string speech)
        {
            return participant1().say(speech);
        }

        public virtual ActorQuery participant2(string speech)
        {
            return participant2().say(speech);
        }


        /* Inner Classes */

        public class ActorQuery : Quest.ActorQuery
        {
            internal ActorQuery(AbstractScene conversation, GameObject actor)
                : base(conversation, actor)
            { }


            /* Override Chainable Accessors */

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


            public new ActorQuery think(string text)
            {
                base.think(text);
                return this;
            }

            public new ActorQuery think(string text, float duration)
            {
                base.think(text, duration);
                return this;
            }


            public new ActorQuery wait(float time)
            {
                base.wait(time);
                return this;
            }

            public new ActorQuery delay(float duration)
            {
                base.delay(duration);
                return this;
            }


            public new ActorQuery act(Fragment action, float duration = 0f)
            {
                base.act(action, duration);
                return this;
            }

            public new ActorQuery delay(float delay, Fragment action)
            {
                base.delay(delay, action);
                return this;
            }


            public new ActorQuery move(GameObject target, float? travelTime = null)
            {
                base.move(target, travelTime);
                return this;
            }

            public new ActorQuery move(Vector3 destination, float? travelTime = null)
            {
                base.move(destination, travelTime);
                return this;
            }

            public new ActorQuery move(Func<Vector3> destinationSupplier, float? travelTime = null)
            {
                base.move(destinationSupplier, travelTime);
                return this;
            }


            /* Upwards chainability */

            public new Conversation up()
            {
                return base.up() as Conversation;
            }

            public Conversation getConversation()
            {
                return up();
            }


            public virtual ActorQuery participant1()
            {
                return getConversation().participant1();
            }

            public virtual ActorQuery participant2()
            {
                return getConversation().participant2();
            }

            public virtual ActorQuery participant1(string speech)
            {
                return getConversation().participant1(speech);
            }

            public virtual ActorQuery participant2(string speech)
            {
                return getConversation().participant2(speech);
            }
        }
    }

    public class PlayerConversation : Conversation
    {
        internal PlayerConversation(State state, GameObject actor)
            : base(state, GameObject.FindWithTag("Player"), actor)
        { }

        protected override Conversation.ActorQuery actorQuery(GameObject actor)
        {
            if (actor.tag == "Player")
                return new PlayerQuery(this, actor);

            return new ActorQuery(this, actor);
        }


        /* Override Chainable Accessors */

        public new PlayerConversation onEnter(Fragment action)
        {
            base.onEnter(action);
            return this;
        }

        public new PlayerConversation onLeave(Fragment action)
        {
            base.onLeave(action);
            return this;
        }

        public new PlayerConversation activatedBy(Activator activator)
        {
            base.activatedBy(activator);
            return this;
        }


        public new PlayerConversation enter()
        {
            base.enter();
            return this;
        }

        public new PlayerConversation leave()
        {
            base.leave();
            return this;
        }
        

        /* Override Superclass Sub-Queries */

        public new virtual ActorQuery participant1()
        {
            return base.participant1() as ActorQuery;
        }

        public new virtual ActorQuery participant2()
        {
            return base.participant2() as ActorQuery;
        }

        public new virtual ActorQuery participant1(string speech)
        {
            return base.participant1(speech) as ActorQuery;
        }

        public new virtual ActorQuery participant2(string speech)
        {
            return base.participant2(speech) as ActorQuery;
        }


        /* Sub-Queries */

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
            internal ActorQuery(AbstractScene conversation, GameObject actor)
                : base(conversation, actor)
            { }


            /* Override Chainable Accessors */

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


            public new ActorQuery think(string text)
            {
                base.think(text);
                return this;
            }

            public new ActorQuery think(string text, float duration)
            {
                base.think(text, duration);
                return this;
            }


            public new ActorQuery wait(float time)
            {
                base.wait(time);
                return this;
            }

            public new ActorQuery delay(float duration)
            {
                base.delay(duration);
                return this;
            }


            public new ActorQuery act(Fragment action, float duration = 0f)
            {
                base.act(action, duration);
                return this;
            }

            public new ActorQuery delay(float delay, Fragment action)
            {
                base.delay(delay, action);
                return this;
            }


            public new ActorQuery move(GameObject target, float? travelTime = null)
            {
                base.move(target, travelTime);
                return this;
            }

            public new ActorQuery move(Vector3 destination, float? travelTime = null)
            {
                base.move(destination, travelTime);
                return this;
            }

            public new ActorQuery move(Func<Vector3> destinationSupplier, float? travelTime = null)
            {
                base.move(destinationSupplier, travelTime);
                return this;
            }


            /* Upwards chainability */

            public new PlayerConversation getConversation()
            {
                return getConversation() as PlayerConversation;
            }


            public new ActorQuery participant1()
            {
                return getConversation().participant1();
            }

            public new ActorQuery participant2()
            {
                return getConversation().participant2();
            }

            public new ActorQuery participant1(string speech)
            {
                return getConversation().participant1(speech);
            }

            public new ActorQuery participant2(string speech)
            {
                return getConversation().participant2(speech);
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
            internal PlayerQuery(AbstractScene conversation, GameObject actor)
                : base(conversation, actor)
            { }


            /* Override Chainable Accessors */

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


            public new PlayerQuery think(string text)
            {
                base.think(text);
                return this;
            }

            public new PlayerQuery think(string text, float duration)
            {
                base.think(text, duration);
                return this;
            }


            public new PlayerQuery wait(float time)
            {
                base.wait(time);
                return this;
            }

            public new PlayerQuery delay(float duration)
            {
                base.delay(duration);
                return this;
            }


            public new PlayerQuery act(Fragment action, float duration = 0f)
            {
                base.act(action, duration);
                return this;
            }

            public new PlayerQuery delay(float delay, Fragment action)
            {
                base.delay(delay, action);
                return this;
            }


            public new PlayerQuery move(GameObject target, float? travelTime = null)
            {
                base.move(target, travelTime);
                return this;
            }

            public new PlayerQuery move(Vector3 destination, float? travelTime = null)
            {
                base.move(destination, travelTime);
                return this;
            }

            public new PlayerQuery move(Func<Vector3> destinationSupplier, float? travelTime = null)
            {
                base.move(destinationSupplier, travelTime);
                return this;
            }
            

            /* Sub-Queries */

            public Choice choice()
            {
                return new Choice(this);
            }


            /* Choice Inner Class */

            public class Choice
            {
                public delegate void Fragment(Choice choice);
                public delegate void Action();

                private PlayerQuery parent;
                private List<Option> options = new List<Option>();


                /* Constructors */

                internal Choice(PlayerQuery playerQuery)
                {
                    this.parent = playerQuery;
                }


                /* Chainable Accessors */

                public Choice option(string text, Fragment action)
                {
                    options.Add(new Option(text, () => action(this)));
                    return this;
                }

                public Choice option(string text, string nextStateName)
                {
                    return option(text, getQuest().getState(nextStateName));
                }

                public Choice option(string text, State nextState)
                {
                    options.Add(new Option(text, () => nextState.enter()));
                    return this;
                }


                public struct Option
                {
                    public readonly string text;
                    public readonly Action action;

                    internal Option(string text, Action action)
                    {
                        this.text = text;
                        this.action = action;
                    }
                }


                /* Upwards chainability */

                public PlayerQuery up()
                {
                    return parent;
                }

                public PlayerQuery getPlayerQuery()
                {
                    return up();
                }

                public AbstractScene getConversation()
                {
                    return up().up();
                }

                public State getState()
                {
                    return up().up().up();
                }

                public Quest getQuest()
                {
                    return up().up().up().up();
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

                public virtual AbstractScene conversation(GameObject actor1, GameObject actor2)
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

                public virtual ActorQuery participant1(string speech)
                {
                    return parent.participant1(speech);
                }

                public virtual ActorQuery participant2(string speech)
                {
                    return parent.participant2(speech);
                }

                public virtual Choice choice()
                {
                    return parent.choice();
                }
            }
        }
    }

    
    /** Activators **/

    /* Activators - Common */

    public static class Activators
    {
        public static Activator WorldBegin = (activateCB) => {
                new WaitForSeconds(0)
                .Then(() => activateCB())
                .Start();
        };

        public static Activator InRange(GameObject actor1, GameObject actor2, float range)
        {
            return (activationCB) => actor1.WhenInRange(actor2, range,
                new TriggerExtensions.VetoingAction(activationCB));
        }
    }
}