using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> neighbours;

    void Start()
    {
        foreach (var item in neighbours) if (!item.neighbours.Contains(this)) item.neighbours.Add(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f);

        if (neighbours == null)
            return;

        for (int i = 0; i < neighbours.Count; i++)
            if(neighbours[i])
                Gizmos.DrawLine(transform.position, neighbours[i].transform.position);
    }
}

