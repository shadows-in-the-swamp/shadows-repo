using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utils;

[RequireComponent(typeof(Rigidbody))]
public abstract class Character : MonoBehaviour
{
    [SerializeField] protected float _walkMaxVelocity = 3f;
    [SerializeField] protected float _walkAcceleration = 1f;
    [SerializeField] protected float _runMaxVelocity = 7f;
    [SerializeField] protected float _runAcceleration = 2f;
    [SerializeField] protected float _crouchMaxVelocity = 2f;
    [SerializeField] protected float _crouchAcceleration = 1f;
    [SerializeField] protected SoundEmitter _leftFootSoundEmitter;
    [SerializeField] protected SoundEmitter _rightFootSoundEmitter;
    [SerializeField] protected float _stepCrouchSoundFactor = 0.5f;
    [SerializeField] protected float _stepWalkSoundFactor = 1f;
    [SerializeField] protected float _stepRunSoundFactor = 2f;
    [SerializeField] protected Eyes _eyes;
    [SerializeField] protected Ears _ears;
    [SerializeField] protected float _pathFindingInterval = 0.25f;
    protected NavMeshAgent _agent;
    protected Transform _target;
    private Vector3 _lastDestination;
    private Vector3 _deltaDirection;
    private Vector3 _lastPosition;

    public virtual Eyes Eyes
    {
        get
        {
            return _eyes;
        }
    }
    protected Vector3 _direction;
    protected Rigidbody _body;
    protected Animator _animator;
    protected bool _isCrouching = false;
    public virtual bool IsCrouching 
    {
        get
        {
            return _isCrouching;
        }
    }

    protected virtual void Awake() {
        _body = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _body.constraints = RigidbodyConstraints.FreezeRotation;
        _agent = GetComponent<NavMeshAgent>();
        _lastPosition = transform.position;
    }
    protected virtual void Start()
    {
        StartCoroutine(PathFindingRoutine());
    }

    protected virtual IEnumerator PathFindingRoutine()
    {
        if (_target != null && !_target.position.Equals(_lastDestination))
        {
            _agent.SetDestination(_target.position);
            _lastDestination = _target.position;
            _deltaDirection = transform.position - _lastPosition;
            _deltaDirection.y = 0;
            _lastPosition = transform.position;
            SetDirection(_deltaDirection);
        }
        yield return new WaitForSeconds(_pathFindingInterval);
        StartCoroutine(PathFindingRoutine());
    }

    public virtual void OnStep(string foot, string soundReference)
    {
        if (foot == StepNames.LeftFootCrouch)
        {
            _leftFootSoundEmitter.Emit(soundReference, false, false, _stepCrouchSoundFactor);
        }
        if (foot == StepNames.LeftFootWalk)
        {
            _leftFootSoundEmitter.Emit(soundReference, false, false, _stepWalkSoundFactor);
        }
        if (foot == StepNames.LeftFootRun)
        {
            _leftFootSoundEmitter.Emit(soundReference, false, false, _stepRunSoundFactor);
        }
        if (foot == StepNames.RightFootCrouch)
        {
            _rightFootSoundEmitter.Emit(soundReference, false, false, _stepCrouchSoundFactor);
        }
        if (foot == StepNames.RightFootWalk)
        {
            _rightFootSoundEmitter.Emit(soundReference, false, false, _stepWalkSoundFactor);
        }
        if (foot == StepNames.RightFootRun)
        {
            _rightFootSoundEmitter.Emit(soundReference, false, false, _stepRunSoundFactor);
        }
    }

    public virtual void SetDirection(Vector3 direction)
    {
        _direction = direction.normalized;
    }

    public virtual void SetTarget(Transform target)
    {
        _target = target;
    }

    protected virtual void Move(float maxVelocity, float acceleration, bool isRunning = false, bool isCrouching = false)
    {
        _animator.SetBool(AnimatorParametersNames.IsMoving, true);
        _animator.SetBool(AnimatorParametersNames.IsRunning, isRunning);
        _animator.SetBool(AnimatorParametersNames.IsCrouching, isCrouching);
        _isCrouching = isCrouching;
        if (!_target)
        {
            _animator.SetFloat(AnimatorParametersNames.DirectionX, _direction.x);
            _animator.SetFloat(AnimatorParametersNames.DirectionY, _direction.z);
            Vector3 relativeDirection = ((transform.forward * _direction.z) + (transform.right * _direction.x)).normalized;
            if (_body.velocity.sqrMagnitude < Math.Pow(maxVelocity, 2))
            {
                _body.velocity += relativeDirection * acceleration;
            }
            else
            {
                _body.velocity = relativeDirection * maxVelocity;
            }
        }
        else
        {
            _agent.isStopped = false;
            _agent.acceleration = maxVelocity;
            _agent.speed = maxVelocity;
            Vector3 relativeDirection = new Vector3(Vector3.Dot(transform.right, _direction),0, Vector3.Dot(transform.forward,_direction)).normalized;
            _animator.SetFloat(AnimatorParametersNames.DirectionX, relativeDirection.x);
            _animator.SetFloat(AnimatorParametersNames.DirectionY, relativeDirection.z);
        }
    }

    public virtual void Stay()
    {
        _animator.SetFloat(AnimatorParametersNames.DirectionY, 0);
        _animator.SetFloat(AnimatorParametersNames.DirectionX, 0);
        _animator.SetBool(AnimatorParametersNames.IsMoving, false);
        _animator.SetBool(AnimatorParametersNames.IsRunning, false);
        _animator.SetBool(AnimatorParametersNames.IsCrouching, false);
        _isCrouching = false;
        DoStay();
    }

    protected abstract void DoStay();

    public virtual void Walk() 
    {
        Move(_walkMaxVelocity, _walkAcceleration);
    }

    public virtual void Run() 
    {
        Move(_runMaxVelocity, _runAcceleration, true);
    }

    public virtual void WalkCrouch() 
    {
        Move(_crouchMaxVelocity, _crouchAcceleration, false, true);
    }

    public virtual void Crouch()
    {
        _animator.SetBool(AnimatorParametersNames.IsCrouching, true);
        _isCrouching = true;
    }

    public virtual void Stand()
    {
        _animator.SetBool(AnimatorParametersNames.IsCrouching, false);
        _isCrouching = false;
    }

    public abstract void OnHear(PerceptionMark mark);
    public abstract void OnSight(PerceptionMark mark);
}
