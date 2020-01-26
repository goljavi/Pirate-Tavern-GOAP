using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldState
{
    public int coin;
    public bool alive;
    public float drunkenness;
    public string equipment;

    public bool brokenCabinet;
    public bool angryOwner;
    public bool cabinetOpen;
    public bool playDarts;

    public bool hasEscaped;

    public int steps;
    public GOAPAction generatingAction = null;

    public WorldState Clone()
    {
        return new WorldState()
        {
            coin = coin,
            alive = alive,
            drunkenness = drunkenness,
            equipment = equipment,
            brokenCabinet = brokenCabinet,
            angryOwner = angryOwner,
            cabinetOpen = cabinetOpen,
            hasEscaped = hasEscaped,
            playDarts = playDarts,
            steps = steps,
        };
    }


}
