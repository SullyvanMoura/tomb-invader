using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Comp_ClimbModifier : MonoBehaviour
{
    [SerializeField] private bool _climbable;

    public bool Climbable {get => _climbable; set => _climbable = value; }

}
