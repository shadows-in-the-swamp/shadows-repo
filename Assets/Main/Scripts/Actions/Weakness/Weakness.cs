using UnityEngine;

public abstract class Weakness<T> : PlayerAction where T : Spell
{
    [SerializeField] protected Enemy _enemy;

    public override bool CanBeActionatedBy(Player player)
    {
        return base.CanBeActionatedBy(player) && player.HasSpell<T>() && player.GetSpell<T>().CanBeCastedBy(player);
    }
}
