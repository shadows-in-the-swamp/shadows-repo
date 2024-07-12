using System;
using UnityEngine;
using Utils;

public abstract class PlayerAction : ActionZone<Player>
{
    [SerializeField] protected InputAxesNames _axisName = InputAxesNames.PrimaryAction;
    [SerializeField] protected string _name;
    [SerializeField] protected string _description;
    [SerializeField] protected string _blockedDescription;
    [SerializeField] protected float _sightDistance = 3f;
    [SerializeField] protected bool _hold = false;

    public virtual bool Hold
    {
        get
        {
            return _hold;
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
}
