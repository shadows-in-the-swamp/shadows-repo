using System.Collections.Generic;
using Utils;

public abstract class EnemyState : IEnemyState
{
    private static readonly List<EnemyState> _instances = new();
    protected static T Instance<T>() where T : EnemyState, new()
    {
        var instance = _instances.Find(instance => instance is T);
        if (instance == null)
        {
            instance = new T();
            _instances.Add(instance);
        }
        return (T)instance;
    }

    protected EnemyState()
    {

    }

    public virtual void OnHear(Enemy enemy, PerceptionMark mark)
    {

    }

    public virtual void OnSight(Enemy enemy, PerceptionMark mark)
    {
        
    }

    public virtual void OnIn(Enemy enemy)
    {
    }

    public virtual void OnOut(Enemy enemy)
    {
    }
    public virtual void Update(Enemy enemy)
    {
        if (enemy.LastSight != null)
        {
            SightUpdate(enemy);
        }
        else if (enemy.LastHeard != null)
        {
            HeardUpdate(enemy);
        }
        else
        {
            IdleUpdate(enemy);
        }
    }

    public void FixedUpdate(Enemy enemy)
    {
        if (enemy.LastSight != null)
        {
            SightFixedUpdate(enemy);
        }
        else if (enemy.LastHeard != null)
        {
            HeardFixedUpdate(enemy);
        }
        else
        {
            IdleFixedUpdate(enemy);
        }
    }

    public virtual void HeardFixedUpdate(Enemy enemy)
    {

    }
    public virtual void HeardUpdate(Enemy enemy)
    {

    }
    public virtual void IdleFixedUpdate(Enemy enemy)
    {

    }
    public virtual void IdleUpdate(Enemy enemy)
    {

    }
    public virtual void SightFixedUpdate(Enemy enemy)
    {

    }
    public virtual void SightUpdate(Enemy enemy)
    {

    }

    public IUpdateState<Enemy> TransitionTo(Enemy reference, IUpdateState<Enemy> to)
    {
        return reference.TransitionTo(this, to as IEnemyState);
    }
}
