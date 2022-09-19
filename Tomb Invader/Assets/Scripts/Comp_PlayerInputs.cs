using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CG
{
    public class Comp_PlayerInputs : MonoBehaviour
    {

        public const string MouseXString = "Mouse X";
        public const string MouseYString = "Mouse Y";
        public const string MouseScrollString = "Mouse ScrollWheel";

        public static float MouseXInput { get => UnityEngine.Input.GetAxis(MouseXString); }
        public static float MouseYInput { get => UnityEngine.Input.GetAxis(MouseYString); }
        public static float MouseScrollInput { get => UnityEngine.Input.GetAxis(MouseScrollString); }
    }

}

