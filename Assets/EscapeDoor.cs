using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeDoor : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip openDoor;

    public void OpenDoor()
    {
        audioSource.PlayOneShot(openDoor);
        var child = transform.GetChild(0);
        child.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
    }
}
