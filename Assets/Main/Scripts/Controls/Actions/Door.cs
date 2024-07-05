using System;
using UnityEngine;

public class Door : ActionZone
{
    [SerializeField] protected Transform _pivot;
    [SerializeField] protected bool _isOpen = false;
    [SerializeField] protected float _closedAngle;
    [SerializeField] protected float _openedAngle;
    [SerializeField] protected float _speed = 1f;
    protected float _minAngleDifference = 0.1f;
    protected float _time;

    protected virtual void Update()
    {
        float angle;
        if (_isOpen)
        {
            angle = Mathf.SmoothDampAngle(_pivot.eulerAngles.y, _openedAngle, ref _time, _speed);
            if ((_openedAngle - angle) <= _minAngleDifference)
            {
                Activate();
                _pivot.rotation = Quaternion.Euler(0,_openedAngle,0);
                return;
            }
        }
        else
        {
            angle = Mathf.SmoothDampAngle(_pivot.eulerAngles.y, _closedAngle, ref _time, _speed);
            if ((_closedAngle - angle) <= _minAngleDifference)
            {
                Activate();
                _pivot.rotation = Quaternion.Euler(0,_closedAngle,0);
                return;
            }
        }
        _pivot.rotation = Quaternion.Euler(0,angle,0);
    }
    public override void ActionatedBy(Player player, Action<string> Callback = null)
    {
        _isOpen = !_isOpen;
        Deactivate();
    }
}
