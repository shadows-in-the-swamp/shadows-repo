using System;
using UnityEngine;

public class Pickable : PlayerAction
{
    [SerializeField] protected Item _item;

    public override void ActionatedBy(Player player, Action<string> Callback)
    {
        _item.PickedBy(player);
        Destroy(gameObject);
    }
}
