using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CG
{

    public class Comp_CharacterController : MonoBehaviour
    {

        private Animator _animator;
        private Comp_PlayerInputs _inputs;
        private Comp_CameraController _cameraController;

        private void Start()
        {

            _animator = GetComponent<Animator>();
            _inputs = GetComponent<Comp_PlayerInputs>();
            _cameraController = GetComponent<Comp_CameraController>();

            _animator.applyRootMotion = false;

        }

    }

}