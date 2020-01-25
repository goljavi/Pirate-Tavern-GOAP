using System;
using System.Collections;
using System.Collections.Generic;

public class Planner
{
    GOAP<WorldState> _goap;

    public Planner(Preferences<WorldState> preferences, Action<IEnumerable<WorldState>> callback)
    {
        _goap = new GOAP<WorldState>(
            GOAPDefaults.DefaultWorldState,
            GOAPDefaults.ActionList,
            GOAPDefaults.Heuristic,
            preferences,
            GOAPDefaults.Expand,
            callback
        );
    }

    public IEnumerator Plan()
    {
        GOAPDefaults.watchdog = 5000;
        return _goap.Run();
    }
}
