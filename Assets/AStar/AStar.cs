using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AStar<Node> where Node : class
{
    //Podría guardar ambas cosas en una tupla, pero al crear una clase custom me da mas legibilidad abajo
    public class Arc
    {
        public Node endpoint;
        public float cost;
        public Arc(Node ep, float c)
        {
            endpoint = ep;
            cost = c;
        }
    }

    //expand can return null as "no neighbours"
    public static IEnumerator Run
    (
        Node from,
        Func<Node, float> h,				    // Current -> Heuristic cost
        Func<Node, bool> satisfies,				// Current -> Satisfies
        Func<Node, IEnumerable<Arc>> expand,	// Current -> (Endpoint, Cost)[]
        Action<IEnumerable<Node>> callback      // Node List -> ()
    )
    {
        var initialState = new AStarState<Node>();
        initialState.open.Add(from);
        initialState.gs[from] = 0;
        initialState.fs[from] = h(from);
        initialState.previous[from] = null;
        initialState.current = from;

        var state = initialState;
        while (state.open.Count > 0 && !state.finished)
        {
            state = state.Clone();

            var candidate = state.open.OrderBy(x => state.fs[x]).First();
            state.current = candidate;

            if (satisfies(candidate))
            {
                state.finished = true;
            }
            else
            {
                state.open.Remove(candidate);
                state.closed.Add(candidate);
                var neighbours = expand(candidate);
                if (neighbours == null || !neighbours.Any())
                    continue;

                var gCandidate = state.gs[candidate];

                foreach (var ne in neighbours)
                {
                    if (ne.endpoint.In(state.closed))
                        continue;

                    var gNeighbour = gCandidate + ne.cost;
                    state.open.Add(ne.endpoint);

                    if (gNeighbour > state.gs.DefaultGet(ne.endpoint, () => gNeighbour))
                        continue;

                    state.previous[ne.endpoint] = candidate;
                    state.gs[ne.endpoint] = gNeighbour;
                    state.fs[ne.endpoint] = gNeighbour + h(ne.endpoint);
                }
            }

            yield return null;
        }

        if (!state.finished)
        {
            callback(null);
        }
        else
        {
            //Climb reversed tree.
            var seq =
                AStarUtility.Generate(state.current, n => state.previous[n])
                .TakeWhile(n => n != null)
                .Reverse();

            callback(seq);
        }
    }

}

public class AStarState<Node>
{
    public HashSet<Node> open;
    public HashSet<Node> closed;
    public Dictionary<Node, float> gs;
    public Dictionary<Node, float> fs;
    public Dictionary<Node, Node> previous;
    public Node current;
    public bool finished;

    public AStarState()
    {
        open = new HashSet<Node>();
        closed = new HashSet<Node>();
        gs = new Dictionary<Node, float>();
        fs = new Dictionary<Node, float>();
        previous = new Dictionary<Node, Node>();
        current = default(Node);
        finished = false;
    }

    public AStarState(AStarState<Node> copy)
    {
        open = new HashSet<Node>(copy.open);
        closed = new HashSet<Node>(copy.closed);
        gs = new Dictionary<Node, float>(copy.gs);
        fs = new Dictionary<Node, float>(copy.fs);
        previous = new Dictionary<Node, Node>(copy.previous);
        current = copy.current;
        finished = copy.finished;
    }

    public AStarState<Node> Clone()
    {
        return new AStarState<Node>(this);
    }
}


public static class AStarUtility
{
    public static IEnumerable<T> Generate<T>(T seed, Func<T, T> mutate)
    {
        var accum = seed;
        while (true)
        {
            yield return accum;
            accum = mutate(accum);
        }
    }

    public static bool In<T>(this T x, HashSet<T> set)
    {
        return set.Contains(x);
    }

    public static V DefaultGet<K, V>(
        this Dictionary<K, V> dict,
        K key,
        Func<V> defaultFactory
    )
    {
        V v;
        if (!dict.TryGetValue(key, out v))
            dict[key] = v = defaultFactory();
        return v;
    }
}

