
using System;
using UnityEngine;

public class Destroyable : EnemyAction
{
    [SerializeField] protected int _hitPoints = 1;
    public override void ActionatedBy(Enemy enemy, Action<string> Callback = null)
    {
        _hitPoints--;
        if (_hitPoints <= 0)
        {
            Destroy(gameObject);
        }
    }
}
