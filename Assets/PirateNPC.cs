using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateNPC : MonoBehaviour
{
    public AudioClip step;
    public AudioClip punch;

    public StartAnimations startingAnim;
    public enum StartAnimations
    {
        Idle,
        Taunting,
        Sitting
    }

    Player _player;
    AudioSource _as;
    Animator _anim;
    event Action _OnUpdate = delegate { };

    void Start()
    {
        _as = GetComponent<AudioSource>();
        _player = FindObjectOfType<Player>();
        _anim = GetComponent<Animator>();
        switch (startingAnim)
        {
            case StartAnimations.Idle:
                _anim.Play("Idle");
                break;
            case StartAnimations.Taunting:
                _anim.Play("Taunting");
                break;
            case StartAnimations.Sitting:
                _anim.Play("Sitting");
                break;
            default:
                break;
        }
    }

    void Update() => _OnUpdate();

    public void Warn()
    {
        _OnUpdate += Follow;
    }

    public void Drunk()
    {
        _anim.Play("Drunk");
    }

    void Follow()
    {
        _anim.Play("Walk");
        var dir = (_player.transform.position - transform.position).normalized;
        transform.position += dir * Time.deltaTime * 2;
        transform.LookAt(new Vector3(_player.transform.position.x, transform.position.y, _player.transform.position.z));
        if (Vector3.Distance(transform.position, _player.transform.position) <= 2)
        {
            _anim.Play("Punch");
            _player.Die();
            _OnUpdate -= Follow;
        }
    }

    // This is executed by an animation event
    void AnimStep() => _as.PlayOneShot(step);

    // This is executed by an animation event
    void PunchSound() => _as.PlayOneShot(punch);
}
