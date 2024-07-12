using System;
using UnityEngine;

public class Door : PlayerAction
{
    [SerializeField] protected Transform _pivot;
    [SerializeField] protected bool _isOpen = false;
    [SerializeField] protected float _closedAngle;
    [SerializeField] protected float _openedAngle;
    [SerializeField] protected float _speed = 1f;
    [SerializeField] protected string _openDescription;
    [SerializeField] protected string _closeDescription;
    [SerializeField] protected float _minAngleDetection = 1f;
    protected float _time;

    protected virtual void Update()
    {
        float angle;
        if (_isOpen)
        {
            _description = _closeDescription;
            angle = Mathf.SmoothDampAngle(_pivot.eulerAngles.y, _openedAngle, ref _time, _speed);
            if (Mathf.Abs(_openedAngle - _pivot.eulerAngles.y) <= _minAngleDetection)
            {
                Activate();
            }
        }
        else
        {
            _description = _openDescription;
            angle = Mathf.SmoothDampAngle(_pivot.eulerAngles.y, _closedAngle, ref _time, _speed);
            if (Mathf.Abs(_closedAngle - _pivot.eulerAngles.y) <= _minAngleDetection)
            {
                Activate();
            }
        }
        _pivot.rotation = Quaternion.Euler(0,angle,0);
    }

    public override bool CanBeActionatedBy(Player player)
    {
        return base.CanBeActionatedBy(player) && player.HasPrimary<SpellsBook>();
    }
    public override void ActionatedBy(Player player, Action<string> Callback = null)
    {
        _isOpen = !_isOpen;
        Deactivate();
    }
}
