
using System;
using UnityEngine;
using Utils;

[CreateAssetMenu(menuName = "Spells/Exorcism")]
public class Exorcism : Spell
{
    public override void CastedBy(Player player, Action<string> Callback)
    {
        player.Animator.TriggerAction((int)PlayerActionsNames.Exorcise, Callback);
    }
}
