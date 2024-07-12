
public class Enraged : EnemyState
{
    public static Enraged Instance
    {
        get
        {
            return Instance<Enraged>();
        }
    }

    public override void Update(Enemy enemy)
    {
        if (enemy is Boss && !(enemy as Boss).CheckEnraged())
        {
            enemy.SetState(Unaware.Instance);
            return;
        }
        base.Update(enemy);
    }

    public override void HeardUpdate(Enemy enemy)
    {
        base.HeardUpdate(enemy);
        enemy.SetState(Aware.Instance);
    }

    public override void SightUpdate(Enemy enemy)
    {
        base.SightUpdate(enemy);
        enemy.SetState(Aware.Instance);
    }
}
