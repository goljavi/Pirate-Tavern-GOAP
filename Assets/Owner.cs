using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Owner : MonoBehaviour
{
    enum OwnerAction
    {
        Walk,
        DrunkWalk,
        Idle,
        ThrowDarts,
    }

    public GameObject dart;
    public GameObject objectSocket;
    public GameObject gameObjectObjective;
    public LayerMask nodeLayerMask;
    public Animator animator;
    public AudioClip step;
    public bool isDrunk;

    AudioSource _as;
    Navigation _nav;
    EventFSM<OwnerAction> _fsm;
    int throws;

    public event Action OnOwnerFinishedPlaying = delegate { };

    // Start is called before the first frame update
    void Start()
    {

        _as = GetComponent<AudioSource>();
        _nav = new Navigation(transform, 1.5f, 0.5f, nodeLayerMask);

        var sitting = new State<OwnerAction>("Sitting");
        var walk = new State<OwnerAction>("Walk");
        var drunkWalk = new State<OwnerAction>("DrunkWalk");
        var idle = new State<OwnerAction>("Idle");
        var throwingDarts = new State<OwnerAction>("ThrowingDarts");

        walk.OnEnter += a =>
        {
            animator.Play("Walking");
            StartNavigation();
        };

        walk.OnUpdate += () =>
        {
            if (isDrunk) _fsm.Feed(OwnerAction.DrunkWalk);
            Navigate();
        };


        drunkWalk.OnEnter += a =>
        {
            animator.Play("DrunkWalking");
            StartNavigation();
        };

        drunkWalk.OnUpdate += () => Navigate();

        throwingDarts.OnEnter += a =>
        {
            animator.Play("Throw");
        };

        idle.OnEnter += a =>
        {
            animator.Play("Idle");
            OnOwnerFinishedPlaying();
        };

        StateConfigurer.Create(sitting)
            .SetTransition(OwnerAction.Walk, walk)
            .Done();

        StateConfigurer.Create(walk)
            .SetTransition(OwnerAction.DrunkWalk, drunkWalk)
            .SetTransition(OwnerAction.ThrowDarts, throwingDarts)
            .Done();

        StateConfigurer.Create(drunkWalk)
            .SetTransition(OwnerAction.ThrowDarts, throwingDarts)
            .Done();

        StateConfigurer.Create(throwingDarts)
            .SetTransition(OwnerAction.Idle, idle)
            .Done();

        StateConfigurer.Create(idle)
            .SetTransition(OwnerAction.ThrowDarts, throwingDarts)
            .Done();

        _fsm = new EventFSM<OwnerAction>(sitting);
    }

    void Update() => _fsm.Update();

    public void Challenge() => _fsm.Feed(OwnerAction.Walk);

    void StartNavigation()
    {
        if (_nav.PathEnded)
        {
            StartCoroutine(_nav.GetRouteTo(gameObjectObjective));
            StartCoroutine(_nav.NavCoroutine());
        }
    }

    void Navigate()
    {
        if (_nav.Path == null) return;
        if (!_nav.PathEnded) _nav.Update();
        else _fsm.Feed(OwnerAction.ThrowDarts);
    }

    // This is executed by an animation event
    public void ThrowDart()
    {
        throws++;
        var obj = Instantiate(dart);
        obj.transform.position = objectSocket.transform.position;
        obj.GetComponent<Dart>().bullseyeLocation = gameObjectObjective.transform.position;
        if (throws >= 3) _fsm.Feed(OwnerAction.Idle);
    }

    // This is executed by an animation event
    void AnimStep() => _as.PlayOneShot(step);
}
