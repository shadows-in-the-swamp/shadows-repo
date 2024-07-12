
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

public class Boss : Enemy
{
    [SerializeField] protected int _exorcismsToDefeat = 3;
    [SerializeField] protected float _enrageMaxTime = 15f;
    protected float _enragedTime = 0;
    protected Destroyable _targetToDestroy;
    protected bool _isAttacking = false;
    protected bool _attackDone = false;
    // protected override EnemyState InitialState
    // {
    //     get
    //     {
    //         return Enraged.Instance;
    //     }
    // }

    public override IEnemyState TransitionTo(EnemyState enemyState, IEnemyState to)
    {
        if (_state is Bounded || _state is Chasing || _state is Alert)
        {
            return Enraged.Instance;
        }
        return base.TransitionTo(enemyState, to);
    }

    public virtual bool CheckEnraged()
    {
        if (_targetToDestroy != null && !_targetToDestroy.IsDestroyed())
        {
            FaceTo(_targetToDestroy.transform.position, Time.deltaTime * _autoFacingUpSpeed);
            if ((_targetToDestroy.transform.position - transform.position).sqrMagnitude < Mathf.Pow(_targetToDestroy.RequiredDistance, 2))
            {
                Stay();
                if (!Animator.IsActionating)
                {
                    void AttackCallback(string eventName)
                    {
                        switch (eventName)
                        {
                            case "Hit":
                                _targetToDestroy.ActionatedBy(this);
                                if (!_targetToDestroy.gameObject.activeSelf)
                                {
                                    _targetToDestroy = null;
                                }
                                break;
                        }
                    }
                    EnemyActionsNames[] attacks = new EnemyActionsNames[2] { EnemyActionsNames.Attack1, EnemyActionsNames.Attack2 };
                    Animator.TriggerAction((int)attacks[Random.Range(0, attacks.Length)], AttackCallback);
                }
            }
            else
            {
                Debug.Log("Run");
                _target = _targetToDestroy.transform;
                Run();
            }
        }
        else
        {
            if (_eyes.HasActions)
            {
                List<IActionZone> actions = _eyes.Actions.FindAll(action => !(action as MonoBehaviour).IsDestroyed() && action is Destroyable);
                if (actions.Count > 0)
                {
                    _targetToDestroy = actions[Random.Range(0, actions.Count)] as Destroyable;
                    _target = _targetToDestroy.transform;
                }
            }
            else
            {
                Debug.Log("Patrol(Run)");
                Patrol(Run);
            }
        }
        if (_enragedTime < _enrageMaxTime)
        {
            _enragedTime += Time.deltaTime;
        }
        else
        {
            _enragedTime = 0f;
            return false;
        }
        return true;
    }

    public override void Exorcised()
    {
        Debug.Log(_exorcismsToDefeat--);
        if (_exorcismsToDefeat <= 0)
        {
            base.Exorcised();
        }
        else
        {
            SetState(Enraged.Instance);
        }
    }

    public virtual void OnEnraged()
    {
        _enragedTime = 0f;
        _targetToDestroy = null;
        _target = null;
    }

    internal void OnEnragedEnd()
    {
        _enragedTime = 0f;
        _targetToDestroy = null;
        _target = null;
    }
}
