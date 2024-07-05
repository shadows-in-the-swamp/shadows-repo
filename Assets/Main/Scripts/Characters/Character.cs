using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utils;

[RequireComponent(typeof(Rigidbody))]
public abstract class Character : MonoBehaviour
{
    #region Editable Properties
    #region Movement
    [Header("Movement")]
    [SerializeField] protected float _walkMaxVelocity = 3f;
    [SerializeField] protected float _walkAcceleration = 1f;
    [SerializeField] protected float _crouchMaxVelocity = 2f;
    [SerializeField] protected float _crouchAcceleration = 1f;
    [SerializeField] protected float _runMaxVelocity = 7f;
    [SerializeField] protected float _runAcceleration = 2f;
    #endregion
    #region Sounds
    [Header("Sounds")]
    [SerializeField] protected SoundEmitter _leftFootSoundEmitter;
    [SerializeField] protected SoundEmitter _rightFootSoundEmitter;
    [SerializeField] protected float _stepCrouchSoundFactor = 0.5f;
    [SerializeField] protected float _stepWalkSoundFactor = 1f;
    [SerializeField] protected float _stepRunSoundFactor = 2f;
    #endregion
    #region Pathfinding
    [Header("Pathfinding")]
    [SerializeField] protected float _pathFindingInterval = 0.25f;
    #endregion
    #endregion
    
    #region Inner Properties
    protected CharacterAnimator _animator;
    public virtual CharacterAnimator Animator
    {
        get
        {
            return _animator;
        }
    }
    public virtual bool IsMoving 
    {
        get
        {
            if (_animator != null)
            {
                return _animator.IsMoving;
            }
            return false;
        }
    }
    public virtual bool IsRunning 
    {
        get
        {
            if (_animator != null)
            {
                return _animator.IsRunning;
            }
            return false;
        }
    }
    public virtual bool IsCrouching 
    {
        get
        {
            if (_animator != null)
            {
                return _animator.IsCrouching;
            }
            return false;
        }
    }
    #region Pathfinding
    protected NavMeshAgent _agent;
    protected Transform _target;
    private Vector3 _deltaDirection;
    private Vector3 _lastPosition;
    protected Vector3 _direction;
    protected IEnumerator _pathFindingRoutine;
    #endregion
    #region Senses
    protected Rigidbody _body;
    protected Ears _ears;
    public virtual Ears Ears
    {
        get
        {
            return _ears;
        }
    }
    protected Eyes _eyes;
    public virtual Eyes Eyes
    {
        get
        {
            return _eyes;
        }
    }
    #endregion
    #endregion

    #region Lifecycle Handlers
    protected virtual void Awake() {
        _body = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<CharacterAnimator>();
        _body.constraints = RigidbodyConstraints.FreezeRotation;
        _agent = GetComponent<NavMeshAgent>();
        _lastPosition = transform.position;

        var keyEvents = GetComponentInChildren<CharacterAnimator>();
        keyEvents.OnStep += OnStep;

        _eyes = GetComponentInChildren<Eyes>();
        _eyes.OnFirstSense += OnSight;

        _ears = GetComponentInChildren<Ears>();
        _ears.OnFirstSense += OnHear;
        _ears.OnSense += OnHear;
    }
    protected virtual void Start()
    {
        StartPathFinding();
    }

    protected virtual void FixedUpdate()
    {
        _deltaDirection = transform.position - _lastPosition;
        _deltaDirection.y = 0;
        _lastPosition = transform.position;
    }
    #endregion

    #region Event Listeners
    public abstract void OnHear(PerceptionMark mark);
    public abstract void OnSight(PerceptionMark mark);
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
    #endregion

    #region Pathfinding
    public virtual void SetTarget(Transform target)
    {
        _target = target;
    }
    protected virtual void StartPathFinding()
    {
        StartCoroutine(PathFindingRoutine());
    }

    protected virtual IEnumerator PathFindingRoutine()
    {
        SetDirection(_deltaDirection);
        if (_target != null)
        {
            _agent.SetDestination(_target.position);
        }
        yield return new WaitForSeconds(_pathFindingInterval);
        StartPathFinding();
    }
    #endregion

    #region ArtificialUpdates
    public virtual void SetDirection(Vector3 direction)
    {
        _direction = direction.normalized;
    }
    protected virtual void Move(float maxVelocity, float acceleration, bool isRunning = false, bool isCrouching = false)
    {
        _animator.SetMotion(true, isRunning, isCrouching);
        if (_target == null)
        {
            _animator.SetDirection(_direction.x, _direction.z);
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
            _animator.SetDirection(relativeDirection.x, relativeDirection.z);
        }
    }

    public virtual void Stay()
    {
        _animator.SetDirection(0, 0);
        _animator.SetMotion(false, false, _animator.IsCrouching);
        _agent.isStopped = true;
        _agent.SetDestination(transform.position);
        _body.velocity = Vector3.zero;
    }

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
        _animator.SetCrouching(true);
    }

    public virtual void Stand()
    {
        _animator.SetCrouching(false);
    }

    public virtual bool FaceTo(Vector3 position, float delta)
    {
        Vector2 direction = new Vector2(position.x - transform.position.x, position.z - transform.position.z).normalized;
        Vector2 forward = new Vector2(transform.forward.x, transform.forward.z).normalized;
        bool isFacingTo = forward == direction;
        if (!isFacingTo)
        {
            Vector2 newForward = Vector2.Lerp(forward, direction, delta);
            transform.forward = new(newForward.x, transform.forward.y, newForward.y);
        }
        return isFacingTo;
    }
    #endregion
}
