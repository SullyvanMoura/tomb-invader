
using UnityEngine;

[System.Serializable]
public class CrossFadeSettings
{

    public string stateName;
    [Min(-1)] public int layer = 0;
    [Min(0)] public float timeOffset;
    [Min(0)] public float transitionDuration;

}
