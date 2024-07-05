using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Utils;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : Character
{
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
    [SerializeField] protected ActionZone _exorcismZone;
    [SerializeField] protected ActionZone _confineZone;
    protected float _alertTime = 0f;
    protected float _searchTime = 0f;
    protected float _randomMaxIdleTime = 0f;
    protected float _idleTime = 0f;
    protected float _boundedTime = 0f;
    protected Transform _currentNode;
    protected IUpdateState<Enemy> _state;
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

    protected override void Awake()
    {
        base.Awake();
        _body.constraints |= RigidbodyConstraints.FreezePosition;
        if (_patrolNodes.Count > 0)
        {
            _currentNode = _patrolNodes[0];
        }
        SetState(Unaware.Instance);
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
        _state?.OnOut(this);
        _state = state;
        _state.OnIn(this);
    }

    public virtual void IdleUpdate()
    {
        Calm();
        Patrol();
    }

    protected virtual void Patrol()
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
            Walk();
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
        var heardDirection = _lastHeard.transform.position - transform.position;
        heardDirection.y = 0;

        transform.LookAt(heardDirection);
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
        var sightDirection = _lastSight.transform.position - transform.position;
        sightDirection.y = 0;
        transform.rotation = Quaternion.LookRotation(sightDirection);
        
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
                        _animator.TriggerAction((int)EnemyActionsNames.Attack, ActionsUtils.Noop1);
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
    }

    public override void OnSight(PerceptionMark mark)
    {
        _lastSight = mark;
    }

    public virtual void ActivateWeakness()
    {
        _confineZone.gameObject.SetActive(true);
    }

    public virtual void DeactivateWeakness()
    {
        _confineZone.gameObject.SetActive(false);
    }

    public virtual void OnBounded()
    {
        _boundedTime = 0f;
        _exorcismZone.Activate();
        // _exorcismZone.GetComponent<ParticleSystem>().Play();
        // _confineZone.Deactivate();
    }

    public virtual void OnUnbounded()
    {
        _boundedTime = 0f;
        _exorcismZone.Deactivate();
        _confineZone.Activate();
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
}
