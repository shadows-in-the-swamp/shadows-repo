
using UnityEngine;

public class Bounded : EnemyState
{
    public static Bounded Instance
    {
        get
        {
            return Instance<Bounded>();
        }
    }

    public override void Update(Enemy enemy)
    {
        base.Update(enemy);
        if (!enemy.CheckBounded())
        {
            enemy.SetState(Alert.Instance);
        }
    }

    public override void OnSight(Enemy enemy, PerceptionMark mark)
    {
        base.OnSight(enemy, mark);
        mark.Pause();
    }

    public override void OnHear(Enemy enemy, PerceptionMark mark)
    {
        base.OnHear(enemy, mark);
        mark.Pause();
    }

    public override void OnIn(Enemy enemy)
    {
        base.OnIn(enemy);
        enemy.OnBounded();
    }

    public override void OnOut(Enemy enemy)
    {
        base.OnOut(enemy);
        enemy.OnUnbounded();
    }
}
