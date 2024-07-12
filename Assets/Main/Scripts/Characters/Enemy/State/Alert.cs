public class Alert : EnemyState
{
    public static Alert Instance
    {
        get
        {
            return Instance<Alert>();
        }
    }

    public override void HeardUpdate(Enemy enemy)
    {
        base.HeardUpdate(enemy);
        enemy.SearchHeard();
    }

    public override void SightUpdate(Enemy enemy)
    {
        base.SightUpdate(enemy);
        enemy.SetState(Chasing.Instance);
    }

    public override void IdleUpdate(Enemy enemy)
    {
        base.IdleUpdate(enemy);
        enemy.SetState(Unaware.Instance);
    }

    public override void OnIn(Enemy enemy)
    {
        base.OnIn(enemy);
        enemy.OnAlert();
    }

    public override void OnOut(Enemy enemy)
    {
        base.OnOut(enemy);
        enemy.OnAlertEnd();
    }
}
