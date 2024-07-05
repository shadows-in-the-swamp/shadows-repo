using System;
using UnityEngine;
using Utils;

[RequireComponent(typeof(Collider))]
public abstract class ActionZone : MonoBehaviour
{
    [SerializeField] protected InputAxesNames _axisName = InputAxesNames.PrimaryAction;
    [SerializeField] protected string _name;
    [SerializeField] protected string _description;
    [SerializeField] protected string _blockedDescription;
    [SerializeField] protected float _sightDistance = 3f;
    [SerializeField] protected float _requiredDistance = 1f;
    [SerializeField] protected bool _hold = false;
    [SerializeField] protected bool _active = true;

    protected Collider _collider;
    public virtual bool Hold
    {
        get
        {
            return _hold;
        }
    }
    public virtual bool Active
    {
        get
        {
            return _active;
        }
    }
    public virtual string Hint
    {
        get
        {
            return $"[{_name}] {_description}";
        }
    }
    public virtual string BlockedHint
    {
        get
        {
            return $"{_blockedDescription}";
        }
    }
    public InputAxesNames AxisName
    {
        get
        {
            return _axisName;
        }
    }

    public float SightDistance
    {
        get
        {
            return _sightDistance;
        }
    }

    public float RequiredDistance
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
    public abstract void ActionatedBy(Player player, Action<string> Callback = null);
    public virtual bool CanBeActionatedBy(Player player)
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

    public virtual bool BeeingActionatedBy(Player player)
    {
        return false;
    }
}
