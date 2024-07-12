using System;
using UnityEngine;

public interface IActionZone
{
}

[RequireComponent(typeof(Collider))]
public abstract class ActionZone<C> : MonoBehaviour, IActionZone where C : Character
{
    [SerializeField] protected float _requiredDistance = 1f;
    [SerializeField] protected bool _active = true;

    protected Collider _collider;
    public virtual bool Active
    {
        get
        {
            return _active;
        }
    }
    public virtual float RequiredDistance
    {
        get
        {
            return _requiredDistance;
        }
    }

    protected virtual void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    protected virtual void Start()
    {
        _collider.enabled = _active;
    }
    public abstract void ActionatedBy(C character, Action<string> Callback = null);
    public virtual bool CanBeActionatedBy(C character)
    {
        return true;
    }

    public virtual void Activate()
    {
        _active = true;
        _collider.enabled = true;
    }

    public virtual void Deactivate()
    {
        _active = false;
        _collider.enabled = false;
    }

    public virtual bool BeeingActionatedBy(C character)
    {
        return false;
    }
}
