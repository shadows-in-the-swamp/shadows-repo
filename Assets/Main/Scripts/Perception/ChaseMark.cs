using System;
using Unity.VisualScripting;
using UnityEngine;

public class ChaseMark : PerceptionMark
{
    [SerializeField] protected float _chaseDuration = 3f;
    protected float _chaseTime = 0f;
    protected GameObject _origin;
    public virtual GameObject Origin
    {
        get
        {
            return _origin;
        }
    }
    protected override void Update()
    {
        if (_chaseTime < _chaseDuration)
        {
            _chaseTime += Time.deltaTime;
            RefreshPosition(_origin);
        }
        else
        {
            if (_origin.TryGetComponent(out Enemy enemy))
            {
                enemy.HideStatus();
            }
            base.Update();
        }
    }

    public override void ResetTime()
    {
        base.ResetTime();
        _chaseTime = 0f;
    }

    public override void RefreshPosition(GameObject origin, Transform point = null)
    {
        if (_origin.TryGetComponent(out Enemy enemy) && !enemy.status.activeSelf)
        {
            enemy.ShowStatus();
        }
        if (_chaseTime < _chaseDuration)
        {
            transform.position = origin.transform.position;
        }
    }

    public override void Initialize(GameObject origin)
    {
        _origin = origin;
        if (origin.TryGetComponent(out Enemy enemy))
        {
            enemy.ShowStatus();
        }
    }

    protected void OnDestroy() {
        if (!_origin.IsDestroyed() && _origin.TryGetComponent(out Enemy enemy))
        {
            enemy.HideStatus();
        }
    }

}
