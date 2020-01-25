using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassCabinet : MonoBehaviour
{
    public GameObject doors;
    public AudioClip openDoorSound;
    public AudioClip breakDoorSound;
    public AudioSource audioSource;

    public void BreakDoors()
    {
        audioSource.PlayOneShot(breakDoorSound);
        Destroy(doors);
    }

    public void OpenDoors()
    {
        audioSource.PlayOneShot(openDoorSound);
        doors.transform.GetChild(0).rotation = Quaternion.Euler(0, 180, 0);
    }
}
