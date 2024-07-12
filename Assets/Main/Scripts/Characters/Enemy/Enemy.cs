using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Utils;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : Character
{
    [Header("Movement")]
    [SerializeField] protected float _autoFacingUpSpeed = 8f;
    [Header("Patrol")]
    [SerializeField] protected List<Transform> _patrolNodes = new();
    [SerializeField] protected bool _randomPatrol = false;
    [SerializeField] protected float _patrolMinIdleTime = 2f;
    [SerializeField] protected float _patrolMaxIdleTime = 5f;
    [SerializeField] protected float _patrolNodeChangeDistance = 0.1f;
    [Header("States")]
    [Header("Alert")]
    [SerializeField] protected float _alertSightMaxTime = 1f;
    [SerializeField] protected float _alertHeardMaxTime = 3f;
    [SerializeField] protected float _searchMaxTime = 10f;
    [Header("Chasing")]
    [SerializeField] protected float _chaseMinDistance = 2f;
    [Header("Bounded")]
    [SerializeField] protected float _boundedMaxTime = 15f;
    [SerializeField] protected ExorcismWeakness _exorcismZone;
    [SerializeField] protected ConfineWeakness _confineZone;
    protected float _alertTime = 0f;
    protected float _searchTime = 0f;
    protected float _randomMaxIdleTime = 0f;
    protected float _idleTime = 0f;
    protected float _boundedTime = 0f;
    protected Transform _currentNode;
    protected IEnemyState _state;
    protected PerceptionMark _lastHeard;
    public virtual PerceptionMark LastHeard
    {
        get
        {
            return _lastHeard;
        }
    }
    protected PerceptionMark _lastSight;
    public virtual PerceptionMark LastSight
    {
        get
        {
            return _lastSight;
        }
    }
    protected virtual EnemyState InitialState
    {
        get
        {
            return Unaware.Instance;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _body.constraints |= RigidbodyConstraints.FreezePosition;
        if (_patrolNodes.Count > 0)
        {
            _currentNode = _patrolNodes[0];
        }
        SetState(InitialState);
    }

    protected virtual void Update()
    {
        _state.Update(this);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        _state.FixedUpdate(this);
    }

    protected virtual void LateUpdate()
    {
        MarksLateUpdate();    
    }
    protected virtual void MarksLateUpdate()
    {
        if (_lastHeard != null && _lastHeard.IsDestroyed())
        {
            _lastHeard = null;
        }
        if (_lastSight != null && _lastSight.IsDestroyed())
        {
            _lastSight = null;
        }
    }

    public virtual void SetState(EnemyState state)
    {
        if (state == _state)
        {
            return;
        }
        IEnemyState previousState = _state;
        if (previousState != null)
        {
            previousState.OnOut(this);
            _state = previousState.TransitionTo(this, state) as IEnemyState;
        }
        else
        {
            _state = state;
        }
        _state.OnIn(this);
    }

    public virtual void IdleUpdate()
    {
        Calm();
        Patrol();
    }

    protected virtual void Patrol(System.Action Move = null)
    {
        if (_patrolNodes.Count == 0)
        {
            return;
        }
        if (_target != _currentNode)
        {
            _target = _currentNode;
        }

        if ((_currentNode.position - transform.position).sqrMagnitude < Mathf.Pow(_patrolNodeChangeDistance, 2)) {
            if (_randomMaxIdleTime == 0)
            {
                _randomMaxIdleTime = Random.Range(_patrolMinIdleTime, _patrolMaxIdleTime);
            }
            if (_idleTime < _randomMaxIdleTime) {
                _idleTime += Time.deltaTime;
                Stay();
            }
            else
            {
                _randomMaxIdleTime = 0f;
                _idleTime = 0f;
                Transform nextNode = NextPatrolNode();
                if (nextNode != null)
                {
                    _currentNode = nextNode;
                    _target = nextNode;
                }
            }
        }
        else
        {
            if (Move != null)
            {
                Move();
            }
            else
            {
                Walk();
            }
        }
    }

    protected virtual Transform NextPatrolNode()
    {
        if (_patrolNodes.Count == 1)
        {
            return _patrolNodes[0];
        }
        if (_randomPatrol)
        {
            Transform nextNode;
            do
            {
                nextNode = _patrolNodes[Random.Range(0, _patrolNodes.Count)];
            }
            while (_currentNode == nextNode);
            return nextNode;
        }
        else if (_patrolNodes.Contains(_currentNode))
        {
            int nextIndex = _patrolNodes.IndexOf(_currentNode) + 1;
            if (nextIndex < _patrolNodes.Count)
            {
                return _patrolNodes[nextIndex];
            }
        }
        if (_patrolNodes.Count > 0)
        {
            return _patrolNodes[0];
        }
        return null;
    }

    public virtual bool CheckHeard()
    {
        if (_lastHeard.IsDestroyed())
        {
            _alertTime = 0;
            return false;
        }
        Stay();
        
        FaceTo(_lastHeard.transform.position, Time.deltaTime * _autoFacingUpSpeed);
        _alertTime += Time.deltaTime;
        return _alertTime >= _alertHeardMaxTime;
    }

    public virtual bool CheckSight()
    {
        if (_lastSight.IsDestroyed())
        {
            _alertTime = 0;
            return false;
        }
        Stay();
        FaceTo(_lastSight.transform.position, Time.deltaTime * _autoFacingUpSpeed);
        
        Eyes.transform.LookAt(_lastSight.transform);
        _alertTime += Time.deltaTime;
        return _alertTime >= _alertSightMaxTime;
    }

    protected virtual void Calm()
    {
        if (_alertTime > 0)
        {
            _alertTime -= Time.deltaTime;
            if (_alertTime < 0)
            {
                _alertTime = 0;
            }
        }
    }

    public virtual void SearchHeard()
    {
        if (_lastHeard.IsDestroyed())
        {
            return;
        }
        _target = _lastHeard.transform;
        if ((_target.position - transform.position).sqrMagnitude < Mathf.Pow(1, 2))
        {
            Stay();
            if (_searchTime < _searchMaxTime)
            {
                _searchTime += Time.deltaTime;
            }
            else
            {
                _lastHeard.gameObject.SetActive(false);
                _searchTime = 0;
            }
        }
        else
        {
            _searchTime = 0;
            Walk();
        }
    }

    public virtual void Chase()
    {
        if (_lastSight.IsDestroyed())
        {
            Stay();
            return;
        }
        _target = _lastSight.transform;
        if ((_target.position - transform.position).sqrMagnitude < Mathf.Pow(_chaseMinDistance, 2))
        {
            Stay();
            if (
                _lastSight is ChaseMark mark &&
                mark.Origin.TryGetComponent(out Player player) &&
                (player.transform.position - transform.position).sqrMagnitude < Mathf.Pow(_chaseMinDistance, 2))
            {
                player.GrabbedBy(this);
                if (player.SuccessfullyGrabbedBy(this))
                {
                    transform.forward = player.transform.position - transform.position;
                    if (!player.IsDead)
                    {
                        _animator.TriggerAction((int)EnemyActionsNames.Kill, ActionsUtils.Noop1);
                        player.Killed();
                    }
                }
            }
        }
        else
        {
            Run();
        }
    }

    public override void OnHear(PerceptionMark mark)
    {
        _lastHeard = mark;
        _state.OnHear(this, mark);
    }

    public override void OnSight(PerceptionMark mark)
    {
        _lastSight = mark;
        _state.OnSight(this, mark);
    }

    public virtual void ActivateWeakness()
    {
        _confineZone.gameObject.SetActive(true);
    }

    public virtual void DeactivateWeakness()
    {
        _confineZone.gameObject.SetActive(false);
    }

    public virtual void OnAlert()
    {
        _lastHeard?.Pause();
    }

    public virtual void OnAlertEnd()
    {
        _lastHeard?.Resume();
    }

    public virtual void OnBounded()
    {
        _boundedTime = 0f;
        _exorcismZone.Activate();
    }

    public virtual void OnUnbounded()
    {
        _boundedTime = 0f;
        _exorcismZone.Deactivate();
        _confineZone.Activate();
        _lastSight?.Resume();
        _lastHeard?.Resume();
    }
    public virtual bool CheckBounded()
    {
        if (_boundedTime < _boundedMaxTime)
        {
            _boundedTime += Time.deltaTime;
            Stay();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + (Vector3.up * 2), transform.position + _direction + (Vector3.up * 2));
        Gizmos.DrawSphere(transform.position + _direction * 1.5f + (Vector3.up * 2), 0.1f);
    }

    public virtual IEnemyState TransitionTo(EnemyState enemyState, IEnemyState to)
    {
        return to;
    }

    public virtual void Exorcised()
    {
        gameObject.SetActive(false);
    }
}
