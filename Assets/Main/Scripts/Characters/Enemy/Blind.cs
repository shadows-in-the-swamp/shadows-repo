
public class Blind : Enemy
{
    public override void OnHear(PerceptionMark mark)
    {
        OnSight(mark);
    }
}
