
using System;
using UnityEngine;
using Utils;

[CreateAssetMenu(menuName = "Spells/Confine")]
public class Confine : Spell
{
    public override bool CanBeCastedBy(Player player)
    {
        return base.CanBeCastedBy(player) && player.IsCrouching;
    }
    public override void CastedBy(Player player, Action<string> Callback)
    {
        player.Animator.TriggerAction((int)PlayerActionsNames.Confine, Callback);
    }
}
