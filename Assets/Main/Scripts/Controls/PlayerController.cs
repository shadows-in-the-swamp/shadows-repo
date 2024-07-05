using System;
using System.Collections;
using UnityEngine;
using Utils;

[RequireComponent(typeof(Player))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] protected Transform _faceTo;
    #region Editable Properties
    #region Camera
    [Header("Camera")]
    [SerializeField] protected Transform _lookAt;
    [SerializeField] protected Transform _verticalPivot;
    [SerializeField] protected float _sensitivity = 2f;
    [SerializeField] protected float _minVerticalAngle = -75f;
    [SerializeField] protected float _maxVerticalAngle = 75f;
    [SerializeField] protected float _autoAimMinSpeed = 0.1f;
    [SerializeField] protected float _autoAimMaxSpeed = 1f;
    [SerializeField] protected float _autoAimMaxSpeedDistance = 1f;
    [SerializeField] protected float _autoAimMinDistance = 0.1f;
    [SerializeField] protected AnimationCurve _autoAimSpeedCurve = new();
    #endregion

    #region Movement
    [Header("Movement")]
    [SerializeField] protected float _reaction = 0.1f;
    [SerializeField] protected float _autoFacingUpSpeed = 1f;
    #endregion

    #region Spine
    [Header("Spine Movement")]
    [SerializeField] protected Transform _spine;
    [SerializeField] protected float _spineMinVerticalAngle = -50f;
    [SerializeField] protected float _spineMaxVerticalAngle = 50f;
    [SerializeField] protected float _spineCrouchMinVerticalAngle = -50f;
    [SerializeField] protected float _spineCrouchMaxVerticalAngle = 20f;
    #endregion

    #region Actions
    [Header("Actions")]
    [SerializeField] protected LayerMask _actionLayers;
    [SerializeField] protected float _actionDistance = 10f;
    [SerializeField] protected float _actionCheckInterval = 0.3f;
    #endregion
    #endregion

    #region Inner Properties
    protected Player _player;
    protected Camera _camera;
    protected ActionZone _aimedAction;
    protected ActionZone _engagedAction;
    protected Action StateFixedUpdate = ActionsUtils.Noop;
    protected Action StateUpdate = ActionsUtils.Noop;
    protected Action StateLateUpdate = ActionsUtils.Noop;
    protected float _verticalRotation = 0f;
    protected float _spineVerticalRotation = 0f;
    protected bool _successAction = false;
    #endregion

    #region Lifecycle Handlers
    protected virtual void Awake()
    {
        _player = GetComponent<Player>();
        _camera = GetComponentInChildren<Camera>();
    }

    protected virtual void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        StartCoroutine(ActionRoutine());
        ToPlayerControlState();
    }
    protected virtual void FixedUpdate()
    {
        StateFixedUpdate();
    }
    protected virtual void Update()
    {
        StateUpdate();
    }

    protected virtual void LateUpdate()
    {
        StateLateUpdate();
    }

    #endregion
    
    #region Artificial Updates
    protected virtual void CameraLateUpdate()
    {
        if (_player.IsDead)
        {
            return;
        }
        float inputX = Input.GetAxis(InputAxesNames.CameraX.ToString()) * _sensitivity;
        float inputY = Input.GetAxis(InputAxesNames.CameraY.ToString()) * _sensitivity;

        _verticalRotation -= inputY;
        _verticalRotation = Mathf.Clamp(_verticalRotation, _minVerticalAngle, _maxVerticalAngle);
        _verticalPivot.localEulerAngles = Vector3.right * _verticalRotation;

        _spineVerticalRotation = _verticalRotation;
        if (_player.IsCrouching)
        {
            _spineVerticalRotation = Mathf.Clamp(_spineVerticalRotation, _spineCrouchMinVerticalAngle, _spineCrouchMaxVerticalAngle);
            _spine.localEulerAngles = Vector3.right * _spineVerticalRotation;
        }
        else
        {
            _spineVerticalRotation = Mathf.Clamp(_spineVerticalRotation, _spineMinVerticalAngle, _spineMaxVerticalAngle);
            _spine.localEulerAngles = Vector3.right * _spineVerticalRotation;
        }

        _player.transform.Rotate(Vector3.up * inputX);
        _camera.transform.LookAt(_lookAt);
    }

    protected virtual void AimUpdate()
    {
        if (_aimedAction != null)
        {
            if (_aimedAction.Active && _aimedAction.CanBeActionatedBy(_player))
            {
                UIController.Instance.SetHint(_aimedAction.Hint);
                if (Input.GetAxisRaw(_aimedAction.AxisName.ToString()) != 0)
                {
                    ToGoingToActionState();
                }
            }
            else
            {
                UIController.Instance.SetHint(_aimedAction.BlockedHint);
            }
        }
    }

    protected virtual void CommandsUpdate()
    {
        if (Input.GetAxisRaw(InputAxesNames.Lantern.ToString()) != 0)
        {
            _player.ToggleLantern();
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    protected virtual void MoveFixedUpdate()
    {
        var inputX = Input.GetAxis(InputAxesNames.Horizontal.ToString());
        var inputY = Input.GetAxis(InputAxesNames.Vertical.ToString());
        var direction = new Vector3(inputX, 0, inputY);
        if (direction.sqrMagnitude > Mathf.Pow(_reaction, 2))
        {
            _player.SetDirection(direction);
            if (Input.GetAxisRaw(InputAxesNames.Run.ToString()) != 0)
            {
                _player.Run();
            }
            else if (Input.GetAxisRaw(InputAxesNames.Crouch.ToString()) != 0)
            {
                _player.WalkCrouch();
            }
            else
            {
                _player.Walk();
            }
        }
        else
        {
            _player.Stay();
        }
        CrouchingFixedUpdate();
    }

    protected virtual void CrouchingFixedUpdate()
    {
        if (Input.GetAxisRaw(InputAxesNames.Crouch.ToString()) != 0 &&
            !(Input.GetAxisRaw(InputAxesNames.Run.ToString()) != 0))
        {
            _player.Crouch();
        }
        else
        {
            _player.Stand();
        }
    }

    protected virtual bool AutoAimTo(Vector3 position, float delta)
    {
        Vector3 direction = (position - _camera.transform.position).normalized;
        float sqrDistance = (_camera.transform.forward - direction).sqrMagnitude;
        _camera.transform.forward = Vector3.Lerp(_camera.transform.forward, direction, delta).normalized;
        return sqrDistance <= MathF.Pow(_autoAimMinDistance, 2);
    }
    
    #region State Methods
    protected virtual void PlayerControlUpdate()
    {
        if (_player.IsGrabbed || _player.IsDead)
        {
            return;
        }
        AimUpdate();
        CommandsUpdate();

    }
    
    protected virtual void PlayerControlFixedUpdate()
    {
        if (_player.IsGrabbed || _player.IsDead)
        {
            _player.Stand();
            _player.Stay();
            return;
        }
        MoveFixedUpdate();
    }

    protected virtual void GoingToActionFixedUpdate()
    {
        CrouchingFixedUpdate();
        if (!(_engagedAction != null && _engagedAction.CanBeActionatedBy(_player)))
        {
            ToPlayerControlState();
            return;
        }
        if ((_engagedAction.transform.position - transform.position).sqrMagnitude <= Mathf.Pow(_engagedAction.RequiredDistance, 2))
        {
            void ActionCallback(string eventName)
            {
                switch (eventName)
                {
                    case "Success":
                        _successAction = true;
                        break;
                }
            }
            _player.Stay();
            _engagedAction.ActionatedBy(_player, ActionCallback);
            ToActionatingState();
            return;
        }
        if (_player.IsCrouching)
        {
            _player.WalkCrouch();
        }
        else
        {
            _player.Walk();
        }
    }

    protected virtual void ActionatingFixedUpdate()
    {
        CrouchingFixedUpdate();
        bool canBeActionated = _engagedAction.CanBeActionatedBy(_player);
        if (
            !_player.Animator.IsActionating ||
            _engagedAction == null ||
            !_engagedAction.Active ||
            (!canBeActionated && !_successAction) ||
            !canBeActionated
            )
        {
            ToPlayerControlState();
            return;
        }
        if (_engagedAction != null)
        {
            if (_engagedAction.Hold)
            {
                if (Input.GetAxisRaw(_engagedAction.AxisName.ToString()) == 0 || !_engagedAction.BeeingActionatedBy(_player))
                {
                    ToPlayerControlState();
                    return;
                }
            }
        } 
    }

    protected virtual void ActionatingLateUpdate()
    {
        if (_engagedAction == null)
        {
            return;
        }
        _player.FaceTo(_engagedAction.transform.position, Time.deltaTime * _autoFacingUpSpeed);
        Vector3 direction = (_engagedAction.transform.position - _camera.transform.position).normalized;
        float maxSpeedDistance = Mathf.Pow(_autoAimMaxSpeedDistance, 2);
        float currentDistance = 1 - Mathf.Clamp((direction - _camera.transform.forward).magnitude / _autoAimMaxSpeedDistance, 0, 1);
        float speed = Mathf.Clamp(_autoAimSpeedCurve.Evaluate(currentDistance) * _autoAimMaxSpeed, _autoAimMinSpeed, _autoAimMaxSpeed);
        if (AutoAimTo(_engagedAction.transform.position, Time.deltaTime * speed))
        {
            StateLateUpdate = () =>
            {
                if (_engagedAction == null)
                {
                    return;
                }
                _camera.transform.LookAt(_engagedAction.transform.position);
            };
        }
    }
    #endregion
    #endregion

    #region Routines
    protected virtual IEnumerator ActionRoutine()
    {
        if (_player.Eyes.HasActions)
        {
            Ray actionRay = new(_camera.transform.position, _camera.transform.forward);
            if (Physics.Raycast(actionRay, out RaycastHit actionHit, _actionDistance, _actionLayers)
                && actionHit.collider.TryGetComponent(out ActionZone actionZone)
                && (_camera.transform.position - actionZone.transform.position).sqrMagnitude < Mathf.Pow(actionZone.SightDistance, 2))
            {
                _aimedAction = actionZone;
            }
            else
            {
                UIController.Instance.ClearHint();
                _aimedAction = null;
            }
        }
        yield return new WaitForSeconds(_actionCheckInterval);
        StartCoroutine(ActionRoutine());
    }
    #endregion

    #region State Transitions
    protected void ToPlayerControlState()
    {
        _player.Animator.EndAction(true);
        _player.Stay();
        _engagedAction = null;
        _successAction = false;
        _player.SetTarget(null);
        StateUpdate = PlayerControlUpdate;
        StateFixedUpdate = PlayerControlFixedUpdate;
        StateLateUpdate = CameraLateUpdate;
    }

    protected void ToGoingToActionState()
    {
        _successAction = false;
        _engagedAction = _aimedAction;
        _player.SetTarget(_engagedAction.transform);
        StateUpdate = ActionsUtils.Noop;
        StateFixedUpdate = GoingToActionFixedUpdate;
        StateLateUpdate = ActionatingLateUpdate;
    }

    protected void ToActionatingState()
    {
        _successAction = false;
        StateFixedUpdate = ActionatingFixedUpdate;
        StateUpdate = ActionsUtils.Noop;
        StateLateUpdate = ActionatingLateUpdate;
    }
    #endregion
}
