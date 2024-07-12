using System;
using UnityEngine;
using Utils;

public class CharacterAnimator : MonoBehaviour
{
    #region Editable Properties
    [SerializeField] protected GameObject _lantern;
    [SerializeField] protected Transform _leftFoot;
    [SerializeField] protected Transform _rightFoot;
    [SerializeField] protected float _stepsCooldown = 0.3f;
    [SerializeField] protected float _perFootStepsCooldown = 0.5f;
    #region Surface
    [Header("Surface")]
    [SerializeField] protected LayerMask _surfaceMask;
    [SerializeField] protected float _surfaceCheckDistance = 5f;
    #endregion
    #endregion

    #region Inner Properties
    protected Animator _animator;
    protected Vector2 _direction;
    public event Action<string, string> OnStep;
    public event Action<string> OnActionKeyEvent;
    protected float _stepsTimer = 0;
    protected float _leftFootStepsTimer = 0;
    protected float _rightFootStepsTimer = 0;
    protected int _actionIndex = 0;
    protected bool _isMoving = false;
    public virtual bool IsMoving
    {
        get
        {
            return _isMoving;
        }
    }
    protected bool _isRunning = false;
    public virtual bool IsRunning
    {
        get
        {
            return _isRunning;
        }
    }
    protected bool _isCrouching = false;
    public virtual bool IsCrouching 
    {
        get
        {
            return _isCrouching;
        }
    }
    protected bool _equippedLantern = false;
    protected bool _wasActionating = false;
    protected bool _isActionating = false;
    public bool IsActionating
    {
        get
        {
            return _isActionating;
        }
    }
    #endregion

    #region Lifecycle Handlers
    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        OnActionKeyEvent = ActionsUtils.Noop1;
    }

    protected virtual void Update() 
    {
        ParametersUpdate();
        LanternUpdate();
        StepsCooldownUpdate();
    }
    #endregion

    #region Artificial Updates
    protected virtual void LanternUpdate()
    {
        float currentWeight = _animator.GetLayerWeight((int)AnimatorLayerIndexes.Lantern);
        bool equipping = _equippedLantern && currentWeight < 1, unequipping = !_equippedLantern && currentWeight > 0;
        if (equipping || unequipping)
        {
            float next;
            if (equipping)
            {
                next = _animator.GetLayerWeight((int)AnimatorLayerIndexes.Lantern) + Time.deltaTime;
                if (next > 1)
                    next = 1;
            }
            else
            {
                next = _animator.GetLayerWeight((int)AnimatorLayerIndexes.Lantern) - Time.deltaTime;
                if (next < 0)
                    next = 0;
            }
            _animator.SetLayerWeight((int)AnimatorLayerIndexes.Lantern, next);
        }
    }
    protected virtual void StepsCooldownUpdate()
    {
        if (_stepsTimer > 0)
        {
            _stepsTimer -= Time.deltaTime;
        }
        if (_leftFootStepsTimer > 0)
        {
            _leftFootStepsTimer -= Time.deltaTime;
        }
        if (_rightFootStepsTimer > 0)
        {
            _rightFootStepsTimer -= Time.deltaTime;
        }
    }

    protected virtual void ParametersUpdate()
    {
        _animator.SetFloat(AnimatorParametersNames.DirectionX.ToString(), _direction.x);
        _animator.SetFloat(AnimatorParametersNames.DirectionY.ToString(), _direction.y);
        _animator.SetBool(AnimatorParametersNames.IsCrouching.ToString(), _isCrouching);
        _animator.SetBool(AnimatorParametersNames.IsMoving.ToString(), _isMoving);
        _animator.SetBool(AnimatorParametersNames.IsRunning.ToString(), _isRunning);

        if (_isActionating && !_wasActionating)
        {
            _animator.SetTrigger(AnimatorParametersNames.Action.ToString());
        }
        else if (!_isActionating && _wasActionating)
        {
            _animator.SetTrigger(AnimatorParametersNames.ActionEnd.ToString());
        }
        _animator.SetFloat(AnimatorParametersNames.ActionIndex.ToString(), _actionIndex);
        _wasActionating = _isActionating;
    }
    #endregion

    #region KeyEvents Listeners
    public virtual void TriggerOnStep(string foot)
    {
        if (_stepsTimer > 0)
        {
            return;
        }
        _stepsTimer = _stepsCooldown;
        Ray ray = new()
        {
            direction = Vector3.down
        };
        if (StepNames.IsLeft(foot) && _leftFootStepsTimer <= 0)
        {
            _leftFootStepsTimer = _perFootStepsCooldown;
            ray.origin = _leftFoot.position;
        }
        else if (StepNames.IsRight(foot) && _rightFootStepsTimer <= 0)
        {
            _rightFootStepsTimer = _perFootStepsCooldown;
            ray.origin = _rightFoot.position;
        }
        if (Physics.Raycast(ray, out RaycastHit hit, _surfaceCheckDistance, _surfaceMask))
        {
            if (hit.collider.TryGetComponent(out SurfaceSound surface))
            {
                OnStep(foot, surface.SoundReference);
            }
        }
    }

    public virtual void ActionKeyEvent(string eventName)
    {
        OnActionKeyEvent(eventName);
        if (eventName == "Done")
        {
            EndAction();
        }
    }
    #endregion

    #region Triggers
    public virtual void ToggleLantern()
    {
        float currentWeight = _animator.GetLayerWeight((int)AnimatorLayerIndexes.Lantern);
        bool equipping = _equippedLantern && currentWeight < 1, unequipping = !_equippedLantern && currentWeight > 0;
        if (!equipping && !unequipping)
        {
            _equippedLantern = !_equippedLantern;
            if (_equippedLantern)
                _lantern.SetActive(true);
            else
                _lantern.SetActive(false);
        }
    }
    #endregion

    #region Animator Parameters
    public virtual void SetCrouching(bool isCrouching)
    {
        _isCrouching = isCrouching;
    }

    public virtual void SetDirection(float x, float y)
    {
        _direction = new Vector2(x,y);
    }

    public virtual void SetMotion(bool isMoving, bool isRunning = false, bool isCrouching = false)
    {
        if (isRunning && !isMoving)
        {
            isRunning = false;
        }
        _isMoving = isMoving;
        _isRunning = isRunning;
        _isCrouching = isCrouching;
    }

    public virtual void TriggerAction(int actionIndex, Action<string> Callback = null)
    {
        if (actionIndex > 0)
        {
            EndAction(true);
            void DoneCallback(string eventName)
            {
                switch (eventName)
                {
                    case "Done":
                        _isActionating = false;
                        break;
                }
            }
            OnActionKeyEvent = Callback + DoneCallback;
            _actionIndex = actionIndex;
            _isActionating = true;
        }
    }

    public virtual void EndAction(bool interrupt = false)
    {
        if (_actionIndex == 0)
        {
            return;
        }
        if (interrupt)
        {
            OnActionKeyEvent("Interrupted");
        }
        OnActionKeyEvent("Done");
        OnActionKeyEvent = ActionsUtils.Noop1;
        _isActionating = false;
    }
    #endregion
}
