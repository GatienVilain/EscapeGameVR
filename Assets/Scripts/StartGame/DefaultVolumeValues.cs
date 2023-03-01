using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DefaultVolumeValues : ScriptableObject
{
    public bool isSetVolume = false;
    public float mainVolume = -20;
    public float musicVolume = -10;
    public float soundVolume = -10;
}
