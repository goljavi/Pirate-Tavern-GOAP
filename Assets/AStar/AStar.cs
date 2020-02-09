using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class AStar<Node> where Node : class
{
    public class NodeCost
    {
        public Node endpoint;
        public float cost;
        public NodeCost(Node ep, float c)
        {
            endpoint = ep;
            cost = c;
        }
    }

    public static IEnumerator Run
    (
        Node start,
        Func<Node, float> heuristic,
        Func<Node, bool> satisfies,
        Func<Node, IEnumerable<NodeCost>> expand,
        Action<IEnumerable<Node>> callback   
    )
    {
        var initialState = new AStarState<Node>();
        initialState.open.Add(start);
        initialState.gs[start] = 0;
        initialState.fs[start] = heuristic(start);
        initialState.previous[start] = null;
        initialState.current = start;

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

                foreach (var neighbour in neighbours)
                {
                    if (neighbour.endpoint.In(state.closed))
                        continue;

                    var gNeighbour = gCandidate + neighbour.cost;
                    state.open.Add(neighbour.endpoint);

                    if (gNeighbour > state.gs.DefaultGet(neighbour.endpoint, () => gNeighbour))
                        continue;

                    state.previous[neighbour.endpoint] = candidate;
                    state.gs[neighbour.endpoint] = gNeighbour;
                    state.fs[neighbour.endpoint] = gNeighbour + heuristic(neighbour.endpoint);
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

