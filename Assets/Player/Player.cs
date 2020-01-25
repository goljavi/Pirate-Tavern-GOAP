using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


public enum PlayerAction
{
    Idle,
    Walk,
    PickUp,
    ThrowObject,
    PlayBullsEye,
    Success,
    Plan,
    Die
}

public class Player : MonoBehaviour
{
    public GameObject dart;
    public AudioClip objectGrab;
    public AudioClip paySound;
    public AudioClip step;
    public GameObject coinObject;
    public GameObject objectSocket;
    public LayerMask nodeLayerMask;
    public float threshold;
    public float speed = 2;

    WorldState _currentWorldState;
    GameObject _gameObjectObjective;
    EventFSM<PlayerAction> _fsm;
    Owner _owner;
    UIManager _ui;

    Rigidbody _rb;
    Animator _anim;
    AudioSource _as;
    Navigation _nav;

    int _throws;
    int _currentIndex;
    bool _canThrowDarts;
    bool _winsDarts;

    Dictionary<GOAPAction, Action> _goToActions;
    Dictionary<GOAPAction, Action> _afterWalkActions;
    Dictionary<GOAPAction, Action> _afterThrowActions;
    Dictionary<string, GameObject> _inGameItems;

   

    event Action OnCoroutineEnd = delegate { };

    void Start()
    {
        _ui = FindObjectOfType<UIManager>();
        _nav = new Navigation(transform, speed, threshold, nodeLayerMask);
        _inGameItems = GetInGameItems();
        _goToActions = GoapGoToAction();
        _afterWalkActions = GoapAfterWalkActions();
        _afterThrowActions = GoapAfterThrowActions();
        _currentWorldState = new WorldState();
        _owner = _inGameItems["Owner"].GetComponent<Owner>();

        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        _as = GetComponent<AudioSource>();

        _ui.OnRunButtonPressed += OnRunButtonPressed;

        var plan = new State<PlayerAction>("Plan");
        var walk = new State<PlayerAction>("Walk");
        var pickup = new State<PlayerAction>("PickUp");
        var throwObject = new State<PlayerAction>("ThrowObject");
        var success = new State<PlayerAction>("Success");
        var die = new State<PlayerAction>("Die");
        var playBullsEye = new State<PlayerAction>("PlayBullsEye");
        var any = new State<PlayerAction>("<any>");

        plan.OnEnter += a =>
        {
            _anim.Play("Idle");
            _currentWorldState = null;
        };

        plan.OnUpdate += () =>
        {
            if (_currentWorldState == null) return;

            var action = _currentWorldState.generatingAction;
            if (_goToActions.ContainsKey(action))
            {
                _goToActions[action]();
            }
        };

        walk.OnEnter += a =>
        {
            _anim.Play("Walking");
            StartCoroutine(_nav.GetRouteTo(_gameObjectObjective));
            StartCoroutine(_nav.NavCoroutine());
        };

        walk.OnUpdate += () =>
        {
            if (_nav.Path == null) return;

            if (!_nav.PathEnded) _nav.Update();
            else _afterWalkActions[_currentWorldState.generatingAction]();

        };

        walk.OnExit += a =>
        {
            _currentIndex = 0;
        };

        pickup.OnEnter += a =>
        {
            _anim.Play("Taking Item");
            var objPosition = _gameObjectObjective.transform.position;
            transform.LookAt(new Vector3(objPosition.x, transform.position.y, objPosition.z));
            StartCoroutine(PickupObject());
            OnCoroutineEnd += FsmFeedPlan;
        };

        pickup.OnExit += a => OnCoroutineEnd -= FsmFeedPlan;

        throwObject.OnEnter += a =>
        {
            _anim.Play("Throw");
            _afterThrowActions[_currentWorldState.generatingAction]();
            OnCoroutineEnd += FsmFeedPlan;
        };

        throwObject.OnExit += a => OnCoroutineEnd -= FsmFeedPlan;

        success.OnEnter += a =>
        {
            StartCoroutine(Escape());
        };

        die.OnEnter += a =>
        {
            _anim.Play("Death");
            _ui.ShowRestart();
        };

        playBullsEye.OnEnter += a =>
        {
            MoveAside();
            _anim.Play("Idle");
            _owner.OnOwnerFinishedPlaying += CanThrowDarts;
        };

        playBullsEye.OnExit += a =>
        {
            _owner.OnOwnerFinishedPlaying -= CanThrowDarts;
        };

        StateConfigurer.Create(any)
            .SetTransition(PlayerAction.Plan, plan)
            .SetTransition(PlayerAction.Success, success)
            .SetTransition(PlayerAction.Die, die)
            .Done();

        StateConfigurer.Create(plan)
            .SetTransition(PlayerAction.Walk, walk)
            .SetTransition(PlayerAction.PickUp, pickup)
            .Done();

        StateConfigurer.Create(walk)
            .SetTransition(PlayerAction.PickUp, pickup)
            .SetTransition(PlayerAction.ThrowObject, throwObject)
            .SetTransition(PlayerAction.PlayBullsEye, playBullsEye)
            .Done();

        _fsm = new EventFSM<PlayerAction>(plan, any);
    }

    private void Update() => _fsm.Update();

    Dictionary<string, GameObject> GetInGameItems()
    {
        var items = new Dictionary<string, GameObject>();
        items["Glass Cabinet"] = GameObject.Find("Glass Cabinet");
        items["Owner"] = GameObject.Find("Owner");
        items["Bartender"] = GameObject.Find("Bartender");
        items["Elf Blood"] = GameObject.Find("Elf Blood");
        items["Breaking Object"] = GameObject.Find("Breaking Object");
        items["Escape Door"] = GameObject.Find("Escape Door");
        items["Key"] = GameObject.Find("Key");
        items["Bullseye"] = GameObject.Find("Bullseye");
        return items;
    }

    Dictionary<GOAPAction, Action> GoapGoToAction()
    {
        var goToActions = new Dictionary<GOAPAction, Action>();
        goToActions[GOAPDefaults.ActionGetBreakingObject] = () =>
        {
            _gameObjectObjective = _inGameItems["Breaking Object"];
            _fsm.Feed(PlayerAction.Walk);
        };

        goToActions[GOAPDefaults.ActionBreakCabinet] = () =>
        {
            _gameObjectObjective = _inGameItems["Glass Cabinet"];
            _fsm.Feed(PlayerAction.Walk);
        };

        goToActions[GOAPDefaults.ActionStealElfBlood] = () =>
        {
            _gameObjectObjective = _inGameItems["Elf Blood"];
            _fsm.Feed(PlayerAction.PickUp);
        };

        goToActions[GOAPDefaults.ActionEscape] = () =>
        {
            _gameObjectObjective = _inGameItems["Escape Door"];
            _fsm.Feed(PlayerAction.Walk);
        };

        goToActions[GOAPDefaults.ActionStealKey] = () =>
        {
            _gameObjectObjective = _inGameItems["Key"];
            _fsm.Feed(PlayerAction.Walk);
        };

        goToActions[GOAPDefaults.ActionOpenCabinet] = () =>
        {
            _gameObjectObjective = _inGameItems["Glass Cabinet"];
            _fsm.Feed(PlayerAction.Walk);
        };

        goToActions[GOAPDefaults.ActionStealCabinet] = () =>
        {
            _gameObjectObjective = _inGameItems["Glass Cabinet"];
            _fsm.Feed(PlayerAction.Walk);
        };

        goToActions[GOAPDefaults.ActionStealCabinetWithoutDying] = () =>
        {
            _gameObjectObjective = _inGameItems["Glass Cabinet"];
            _fsm.Feed(PlayerAction.Walk);
        };

        goToActions[GOAPDefaults.ActionPayForGrog] = () =>
        {
            _gameObjectObjective = _inGameItems["Bartender"];
            _fsm.Feed(PlayerAction.Walk);
        };

        goToActions[GOAPDefaults.ActionChallengeOwner] = () =>
        {
            _gameObjectObjective = _inGameItems["Owner"];
            _fsm.Feed(PlayerAction.Walk);
        };

        goToActions[GOAPDefaults.ActionPlayDarts] = () =>
        {
            _gameObjectObjective = _inGameItems["Bullseye"];
            _fsm.Feed(PlayerAction.Walk);
        };

        goToActions[GOAPDefaults.ActionStealKeyWithoutAngryOwner] = () =>
        {
            _gameObjectObjective = _inGameItems["Key"];
            _fsm.Feed(PlayerAction.Walk);
        };

        return goToActions;
    }

    void FsmFeedPlan() => _fsm.Feed(PlayerAction.Plan);

    Dictionary<GOAPAction, Action> GoapAfterWalkActions()
    {
        var afterWalkActions = new Dictionary<GOAPAction, Action>();
        afterWalkActions[GOAPDefaults.ActionGetBreakingObject] = () => _fsm.Feed(PlayerAction.PickUp);
        afterWalkActions[GOAPDefaults.ActionBreakCabinet] = () => _fsm.Feed(PlayerAction.ThrowObject);
        afterWalkActions[GOAPDefaults.ActionEscape] = () => _fsm.Feed(PlayerAction.Success);
        afterWalkActions[GOAPDefaults.ActionStealKey] = () => _fsm.Feed(PlayerAction.PickUp);
        afterWalkActions[GOAPDefaults.ActionStealCabinet] = () =>
        {
            _fsm.Feed(PlayerAction.PickUp);
        };
        afterWalkActions[GOAPDefaults.ActionStealCabinetWithoutDying] = () =>
        {
            _fsm.Feed(PlayerAction.PickUp);
        };
        afterWalkActions[GOAPDefaults.ActionOpenCabinet] = () => _fsm.Feed(PlayerAction.ThrowObject);
        afterWalkActions[GOAPDefaults.ActionPayForGrog] = () =>
        {
            var obj = Instantiate(coinObject);
            obj.transform.parent = objectSocket.transform;
            obj.transform.position = objectSocket.transform.position;
            obj.transform.rotation = objectSocket.transform.rotation;
            _owner.isDrunk = true;
            _fsm.Feed(PlayerAction.ThrowObject);
        };
        afterWalkActions[GOAPDefaults.ActionChallengeOwner] = () =>
        {
            _owner.GetComponent<Owner>().Challenge();
            _fsm.Feed(PlayerAction.Plan);
        };
        afterWalkActions[GOAPDefaults.ActionPlayDarts] = () => _fsm.Feed(PlayerAction.PlayBullsEye);
        afterWalkActions[GOAPDefaults.ActionStealKeyWithoutAngryOwner] = () => _fsm.Feed(PlayerAction.PickUp);
        return afterWalkActions;
    }

    Dictionary<GOAPAction, Action> GoapAfterThrowActions()
    {
        var afterThrowActions = new Dictionary<GOAPAction, Action>();
        afterThrowActions[GOAPDefaults.ActionBreakCabinet] = () => StartCoroutine(BreakCabinet());
        afterThrowActions[GOAPDefaults.ActionOpenCabinet] = () => StartCoroutine(OpenCabinet());
        afterThrowActions[GOAPDefaults.ActionPayForGrog] = () => StartCoroutine(PayForGrog());
        return afterThrowActions;
    }

    IEnumerator ActionCoroutine(IEnumerable<WorldState> plan)
    {
        if (plan == null)
        {
            Debug.LogError("Couldn't make a plan");
            _ui.ShowRestart();
        }
        else
        {
            foreach (var action in plan)
            {
                yield return new WaitUntil(() => _currentWorldState == null);
                _currentWorldState = action;
            }
        }
    }

    IEnumerator PickupObject()
    {
        yield return new WaitForSeconds(1.7f);
        if (_currentWorldState.generatingAction.name == "Get breaking object") PayForCurrentObject();
        _as.PlayOneShot(objectGrab);
        _gameObjectObjective.transform.parent = objectSocket.transform;
        _gameObjectObjective.transform.position = objectSocket.transform.position;
        _gameObjectObjective.transform.rotation = objectSocket.transform.rotation;

        if (_currentWorldState.generatingAction.name == "Steal cabinet")
        {
            _gameObjectObjective.transform.localScale *= 0.3f;
            WarnAllNPCs();
        }
        else if (_currentWorldState.generatingAction.name == "Steal cabinet without dying")
        {
            _gameObjectObjective.transform.localScale *= 0.3f;
        }

        yield return new WaitForSeconds(3);
        OnCoroutineEnd();
    }

    IEnumerator BreakCabinet()
    {
        yield return new WaitForSeconds(1);
        _gameObjectObjective.GetComponent<GlassCabinet>().BreakDoors();
        Destroy(objectSocket.transform.GetChild(0).gameObject);
        OnCoroutineEnd();
    }

    IEnumerator OpenCabinet()
    {
        yield return new WaitForSeconds(1);
        _gameObjectObjective.GetComponent<GlassCabinet>().OpenDoors();
        Destroy(objectSocket.transform.GetChild(0).gameObject);
        OnCoroutineEnd();
    }

    IEnumerator PayForGrog()
    {
        _as.PlayOneShot(paySound);
        var objPosition = _gameObjectObjective.transform.position;
        transform.LookAt(new Vector3(objPosition.x, transform.position.y, objPosition.z));
        yield return new WaitForSeconds(2);
        Destroy(objectSocket.transform.GetChild(0).gameObject);
        foreach (var npc in FindObjectsOfType<PirateNPC>()) npc.Drunk();
        OnCoroutineEnd();
    }


    IEnumerator Escape()
    {
        _anim.Play("Taking Item");
        yield return new WaitForSeconds(1.5f);
        _gameObjectObjective.GetComponent<EscapeDoor>().OpenDoor();
        _anim.Play("Jumping");
        _ui.ShowRestart();
        OnCoroutineEnd();
    }

    void PayForCurrentObject()
    {
        var obj = Instantiate(coinObject);
        obj.transform.position = _gameObjectObjective.transform.position;
        obj.transform.localScale *= 3;
        _as.PlayOneShot(paySound);
    }

    void WarnAllNPCs()
    {
        foreach (var npc in FindObjectsOfType<PirateNPC>()) npc.Warn();
    }

    public void Die()
    {
        _fsm.Feed(PlayerAction.Die);
    }

    void MoveAside()
    {
        transform.position += new Vector3(2, 0, 0);
    }

    void CanThrowDarts()
    {
        _canThrowDarts = true;
        _anim.Play("Throw");
    }

    // This is executed by an animation event
    void ThrowDart()
    {
        if (_canThrowDarts)
        {
            _throws++;
            var obj = Instantiate(dart);
            obj.transform.position = objectSocket.transform.position;
            obj.GetComponent<Dart>().bullseyeLocation = _gameObjectObjective.transform.position;
            if (_throws >= 3)
            {
                _anim.Play("Idle");
                _canThrowDarts = false;

                var rand = UnityEngine.Random.value < 0.5;
                if (!DidPlayerWinDarts()) WarnAllNPCs();
                else _fsm.Feed(PlayerAction.Plan);
            }
        }
    }

    bool DidPlayerWinDarts(){
        var rand = UnityEngine.Random.value;
        return _currentWorldState.drunkenness == 0 ? rand < 0.3 : rand < 0.7;
    }

    void OnRunButtonPressed(Preferences<WorldState> preferences) => StartCoroutine(new Planner(preferences, OnFinishedPlanning).Plan());

    void OnFinishedPlanning(IEnumerable<WorldState> plan)
    {
        _ui.Hide();
        StartCoroutine(ActionCoroutine(plan));
    }

    // This is executed by an animation event
    void AnimStep() => _as.PlayOneShot(step);
}
