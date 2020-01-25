using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dart : MonoBehaviour
{
    public Vector3 bullseyeLocation;

    // Update is called once per frame
    void Update()
    {
        if (bullseyeLocation == Vector3.zero) return;
        var dir = (bullseyeLocation - transform.position).normalized;
        transform.position += dir * Time.deltaTime * 8;
    }

    private void OnCollisionEnter(Collision collision) => Destroy(gameObject);
    private void OnTriggerEnter(Collider other) => Destroy(gameObject);
}
