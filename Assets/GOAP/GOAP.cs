using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GOAP<T> where T : class
{
    T _start;
    IEnumerable<GOAPAction> _actions;
    Func<T, float> _heuristic;
    Preferences<T> _preferences;
    Func<T, IEnumerable<GOAPAction>, IEnumerable<AStar<T>.Arc>> _expand;
    Action<IEnumerable<T>> _callback;

    public GOAP
    (
        T start, 
        IEnumerable<GOAPAction> actions, 
        Func<T, float> heuristic, Preferences<T> preferences, 
        Func<T, IEnumerable<GOAPAction>, IEnumerable<AStar<T>.Arc>> expand,
        Action<IEnumerable<T>> callback
    )
    {
        _start = start;
        _actions = actions;
        _heuristic = heuristic;
        _preferences = preferences;
        _expand = expand;
        _callback = callback;
    }

    public IEnumerator Run()
    {
        return AStar<T>.Run(
            _start,
            current => _heuristic(current),
            current => _preferences.Satisfies(current),
            current => _expand(current, _actions),
            Callback
        );
    }

    void Callback(IEnumerable<T> list)
    {
        Debug.Log("WATCHDOG " + GOAPDefaults.watchdog);
        _callback(list != null ? list.Skip(1) : null);
    }
}
