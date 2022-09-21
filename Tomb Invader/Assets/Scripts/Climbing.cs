using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class Climbing : MonoBehaviour
{

    [Header("Climb Settings")]
    [SerializeField] private float _wallAngleMax;
    [SerializeField] private float _groundAngleMax;
    [SerializeField] private float _dropCheckDistance;
    [SerializeField] private LayerMask _layerMaskClimb;


    [Header("Heights")]
    [SerializeField] private float _overpassHeight;
    [SerializeField] private float _hangHeight;
    [SerializeField] private float _climbUpHeight;
    [SerializeField] private float _vaultHeight;
    [SerializeField] private float _stepHeight;

    [Header("Animation Settings")]
    public CrossFadeSettings _standToFreeHandSetting;
    public CrossFadeSettings _climbUpSetting;
    public CrossFadeSettings _vaultSetting;
    public CrossFadeSettings _stepUpSetting;
    public CrossFadeSettings _dropSetting;
    public CrossFadeSettings _dropToAirSetting;

    [Header("Offsets")]
    [SerializeField] private Vector3 _endOffset;
    [SerializeField] private Vector3 _hangOffset;
    [SerializeField] private Vector3 _dropOffset;
    [SerializeField] private Vector3 _climbOriginDown;


    private Animator _animator;
    private Rigidbody _rigidBody;
    private CapsuleCollider _capsule;
    private StarterAssetsInputs _input;
    private ThirdPersonController _TPcontroller;
    private Comp_SMBEventCurrator _eventCurrator;

    private bool _climbing;
    private Vector3 _endPosition;
    private Quaternion _matchTargetRotation;
    private Vector3 _matchTargetPosition;
    private Quaternion _forwardNormalXZRotation;
    private RaycastHit _downRaycastHit;
    private RaycastHit _forwardRaycastHit;
    private MatchTargetWeightMask _weightMask = new MatchTargetWeightMask(Vector3.one, 1);
    private Coroutine _hangRoutine;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody>();
        _capsule = GetComponent<CapsuleCollider>();
        _input = GetComponent<StarterAssetsInputs>();
        _eventCurrator = GetComponent<Comp_SMBEventCurrator>();
        _TPcontroller = GetComponent<ThirdPersonController>();

        _eventCurrator.Event.AddListener(OnSMBEvent);

    }

    // Update is called once per frame
    void Update()
    {
        if (!_climbing)
            if (_input.move.y != 0)
                if (true)
                    if (CanClimb(out _downRaycastHit, out _forwardRaycastHit, out _endPosition))
                        InitiateClimb();


    }
    private void OnAnimatorMove()
    {
        if (_animator.isMatchingTarget)
            _animator.ApplyBuiltinRootMotion();
    }

    private bool CanClimb(out RaycastHit downRaycastHit, out RaycastHit forwardRaycastHit, out Vector3 endPosition)
    {
        endPosition = Vector3.zero;
        downRaycastHit = new RaycastHit();
        forwardRaycastHit = new RaycastHit();

        bool _downHit;
        bool _forwardHit;
        bool _overpassHit;
        float _climbHeight;
        float _groundAngle;
        float _wallAngle;

        RaycastHit _downRaycastHit;
        RaycastHit _forwardRaycastHit;
        RaycastHit _overpassRaycastHit;

        Vector3 _endPosition;
        Vector3 _forwardDirectionXZ;
        Vector3 _forwardNormalXZ;

        Vector3 _downDirection = Vector3.down;
        Vector3 _downOrigin = transform.TransformPoint(_climbOriginDown);
        Comp_ClimbModifier _climbModifier;

        _downHit = Physics.Raycast(_downOrigin, _downDirection, out _downRaycastHit, _climbOriginDown.y - _stepHeight, _layerMaskClimb);
        _climbModifier = _downHit ? _downRaycastHit.collider.GetComponent<Comp_ClimbModifier>() : null;

        if (_downHit)
        {
            if (_climbModifier == null || _climbModifier.Climbable)
            {

                //Forward + overpass cast
                float _forwardDistance = _climbOriginDown.z;
                Vector3 _forwardOrigin = new Vector3(transform.position.x, _downRaycastHit.point.y - 0.1f, transform.position.z);
                Vector3 _overpassOrigin = new Vector3(transform.position.x, _overpassHeight, transform.position.z);

                _forwardDirectionXZ = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                _forwardHit = Physics.Raycast(_forwardOrigin, _forwardDirectionXZ, out _forwardRaycastHit, _forwardDistance, _layerMaskClimb);
                _overpassHit = Physics.Raycast(_overpassOrigin, _forwardDirectionXZ, out _overpassRaycastHit, _forwardDistance, _layerMaskClimb);
                _climbHeight = _downRaycastHit.point.y - transform.position.y;

                if (_forwardHit)
                {
                    if (_overpassHit || _climbHeight < _overpassHeight)
                    {
                        //Angles
                        _forwardNormalXZ = Vector3.ProjectOnPlane(_forwardRaycastHit.normal, Vector3.up);
                        _groundAngle = Vector3.Angle(_downRaycastHit.normal, Vector3.up);
                        _wallAngle = Vector3.Angle(-_forwardNormalXZ, _forwardDirectionXZ);

                        if (_wallAngle <= _wallAngleMax)
                            if (_groundAngle <= _groundAngleMax)
                            {
                                //End offset
                                Vector3 _vectorSurface = Vector3.ProjectOnPlane(_forwardDirectionXZ, _downRaycastHit.normal);
                                _endPosition = _downRaycastHit.point + Quaternion.LookRotation(_vectorSurface, Vector3.up) * _endOffset;

                                //De-penetration
                                Collider _colliderB = _downRaycastHit.collider;
                                bool _penetrationOverlap = Physics.ComputePenetration(
                                    colliderA: _capsule,
                                    positionA: _endPosition,
                                    rotationA: transform.rotation,
                                    colliderB: _colliderB,
                                    positionB: _colliderB.transform.position,
                                    rotationB: _colliderB.transform.rotation,
                                    direction: out Vector3 _penetrationDirection,
                                    distance: out float _penetrationDistance);

                                if (_penetrationOverlap)
                                    _endPosition += _penetrationDirection * _penetrationDistance;

                                //Up Sweep
                                float _inflate = 0.05f;
                                float _upsweepDistance = _downRaycastHit.point.y - transform.position.y;
                                Vector3 _upSweepDirection = transform.up;
                                Vector3 _upSweepOrigin = transform.position;
                                bool _upSweepHit = CharacterSweep(
                                    position: _upSweepOrigin,
                                    rotation: transform.rotation,
                                    direction: _upSweepDirection,
                                    distance: _upsweepDistance,
                                    layerMask: _layerMaskClimb,
                                    inflate: _inflate);
                                //Forward Sweep
                                Vector3 _forwardSweepOrigin = transform.position + _upSweepDirection * _upsweepDistance;
                                Vector3 _forwardSweepVector = _endPosition - _forwardSweepOrigin;
                                bool _forwardSweepHit = CharacterSweep(
                                    position: _forwardSweepOrigin,
                                    rotation: transform.rotation,
                                    direction: _forwardSweepVector.normalized,
                                    distance: _forwardSweepVector.magnitude,
                                    layerMask: _layerMaskClimb,
                                    inflate: _inflate);

                                if (!_upSweepHit && !_forwardSweepHit)
                                {
                                    endPosition = _endPosition;
                                    downRaycastHit = _downRaycastHit;
                                    forwardRaycastHit = _forwardRaycastHit;
                                    return true;
                                }
                            }
                    }
                }
            }
        }
        return false;
    }

    private bool CharacterSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, LayerMask layerMask, float inflate)
    {
        //Assuming capsule is on y axis
        float _heightScale = Mathf.Abs(transform.lossyScale.y);
        float _radiusScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));

        float _radius = _capsule.radius * _radiusScale;
        float _totalHeight = Mathf.Max(_capsule.height * _heightScale, _radius * 2);

        Vector3 _capsuleUp = rotation * Vector3.up; //Assuming y axis
        Vector3 _center = position + rotation * _capsule.center;
        Vector3 _top = _center + _capsuleUp * (_totalHeight / 2 - _radius);
        Vector3 _bottom = _center - _capsuleUp * (_totalHeight / 2 - _radius);

        bool _sweepHit = Physics.CapsuleCast(
            point1: _bottom,
            point2: _top,
            radius: _radius,
            direction: direction,
            maxDistance: distance,
            layerMask: layerMask);

        return _sweepHit;
    }

    private void InitiateClimb()
    {
        _climbing = true;
        _TPcontroller.Stop();
        _animator.SetFloat("Speed", 0);
        _capsule.enabled = false;

        float _climbHeight = _downRaycastHit.point.y - transform.position.y;
        Vector3 _forwardNormalXZ = Vector3.ProjectOnPlane(_forwardRaycastHit.normal, Vector3.up);
        _forwardNormalXZRotation = Quaternion.LookRotation(-_forwardNormalXZ, Vector3.up);

        if (_climbHeight > _hangHeight)
        {
            _matchTargetPosition = _forwardRaycastHit.point + _forwardNormalXZRotation * _hangOffset;
            _matchTargetRotation = _forwardNormalXZRotation;
            _animator.CrossFadeInFixedTime(_standToFreeHandSetting);
        }
        else if (_climbHeight > _climbUpHeight)
        {
            _matchTargetPosition = _endPosition;
            _matchTargetRotation = _forwardNormalXZRotation;
            _animator.CrossFadeInFixedTime(_climbUpSetting);
        }
        else if (_climbHeight > _vaultHeight)
        {
            _matchTargetPosition = _endPosition;
            _matchTargetRotation = _forwardNormalXZRotation;
            _animator.CrossFadeInFixedTime(_vaultSetting);
        }
        else if (_climbHeight > _stepHeight)
        {
            _matchTargetPosition = _endPosition;
            _matchTargetRotation = _forwardNormalXZRotation;
            _animator.CrossFadeInFixedTime(_stepUpSetting);
        }
        else
        {
            _climbing = false;
            _capsule.enabled = true;
            _TPcontroller.Continue();
        }
    }

    private void OnSMBEvent(string eventName)
    {
        switch (eventName)
        {
            case "StandToFreeHangEnter":
                _animator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, 0.3f, 0.65f);
                break;
            case "ClimbUpEnter":
                _animator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, 0, 0.9f);
                break;
            case "VaultEnter":
                _animator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, 0, 0.65f);
                break;
            case "StepUpEnter":
                _animator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, 0.3f, 0.8f);
                break;
            case "DropEnter":
                _animator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, 0.2f, 0.5f);
                break;
            case "StandToFreeHangExit":
                _hangRoutine = StartCoroutine(HangingRoutine());
                break;
            case "ClimbUpExit":
            case "VaultExit":
            case "StepUpExit":
            case "DropExit":
                _climbing = false;
                _capsule.enabled = true;
                _TPcontroller.Continue();
                break;
            case "DropToAir":
                _climbing = false;
                _capsule.enabled = true;
                _TPcontroller.Continue();
                break;
        }
    }

    private IEnumerator HangingRoutine()
    {
        //Wait fot input
        while (_input.move.y == 0)
            yield return null;

        //ClimbUp
        if (_input.move.y > 0)
        {
            _matchTargetPosition = _endPosition;
            _matchTargetRotation = _forwardNormalXZRotation;
            _animator.CrossFadeInFixedTime(_climbUpSetting);
        }
        //Drop Down
        else if (_input.move.y < 0)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit _hittInfo, _dropCheckDistance, _layerMaskClimb))
            {
                _animator.CrossFade(_dropToAirSetting);
            }
            else
            {
                _matchTargetPosition = _hittInfo.point + _forwardNormalXZRotation * _dropOffset;
                _matchTargetRotation = _forwardNormalXZRotation;
                _animator.CrossFadeInFixedTime(_dropSetting);
            }

            /*Physics.Raycast(transform.position, Vector3.down, out RaycastHit _hittInfo, _dropCheckDistance, _layerMaskClimb);
            _matchTargetPosition = _hittInfo.point + _forwardNormalXZRotation * _dropOffset;
            _matchTargetRotation = _forwardNormalXZRotation;
            _animator.CrossFadeInFixedTime(_dropSetting);*/

        }

        _hangRoutine = null;

    }

}
