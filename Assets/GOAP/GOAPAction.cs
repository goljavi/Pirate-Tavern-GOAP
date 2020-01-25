using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPAction
{
    public List<Func<WorldState, bool>> preconditions { get; private set; }
    public List<Func<WorldState, WorldState>> effects { get; private set; }
    public string name { get; private set; }
    public float cost { get; private set; }

    public GOAPAction(string name)
    {
        this.name = name;
        cost = 1f;
        preconditions = new List<Func<WorldState, bool>>();
        effects = new List<Func<WorldState, WorldState>>();
    }

    public GOAPAction AddCost(float cost)
    {
        this.cost = cost;
        return this;
    }
    public GOAPAction AddPrecondition(Func<WorldState, bool> condition)
    {
        preconditions.Add(condition);
        return this;
    }
    public GOAPAction AddEffect(Func<WorldState, WorldState> effect)
    {
        effects.Add(effect);
        return this;
    }

    public override string ToString() =>  "[GOAPAction]: " + name + " (" + cost + ")";
}
