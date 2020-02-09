using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Navigation
{
    Transform _t;
    float _speed;
    float _threshold;
    LayerMask _nodeLayerMask;
    Node _currentNode;
    
    public IEnumerable<Node> Path { get; private set; }
    public bool PathEnded { get; private set; }

    public Navigation(Transform t, float speed, float threshold, LayerMask nodeLayerMask)
    {
        PathEnded = true;
        _t = t;
        _speed = speed;
        _threshold = threshold;
        _nodeLayerMask = nodeLayerMask;
    }

    public IEnumerator GetRouteTo(GameObject obj)
    {
        var finalNode = GetClosestNode(obj.transform.position);
        var startingNode = GetClosestNode(_t.position);
        if (finalNode == null || startingNode == null) return null;

        Path = null;
        PathEnded = false;

        return AStar<Node>.Run(
            startingNode,
            node => Vector3.Distance(node.transform.position, finalNode.transform.position),
            node => finalNode == node,
            node =>
            {
                var list = new List<AStar<Node>.NodeCost>();
                foreach (var item in node.neighbours) list.Add(new AStar<Node>.NodeCost(item, 1));
                return list;
            },
            path => { Path = path.Skip(1); }
        );
    }

    Node GetClosestNode(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, 5, _nodeLayerMask);
        var hitCollidersOrdered = hitColliders.OrderBy(x => Vector3.Distance(x.transform.position, position));

        foreach (var item in hitCollidersOrdered)
        {
            RaycastHit hit;
            if (Physics.Raycast(_t.position, item.transform.position - _t.position, out hit, Mathf.Infinity))
            {
                return item.GetComponent<Node>();
            }
        }

        return null;
    }

    public void Update()
    {
        if (Path == null) return;
        var dir = (_currentNode.transform.position - _t.position).normalized;
        _t.position += dir * Time.deltaTime * _speed;
        _t.LookAt(new Vector3(_currentNode.transform.position.x, _t.position.y, _currentNode.transform.position.z));
    }

    public IEnumerator NavCoroutine()
    {
        while (!PathEnded)
        {
            if (Path != null)
            {
                foreach (var node in Path)
                {
                    _currentNode = node;
                    yield return new WaitUntil(() => Vector3.Distance(_t.position, _currentNode.transform.position) <= _threshold);
                }

                PathEnded = true;
            }
            else
            {
                yield return null;
            }
        }
        
    }


}
