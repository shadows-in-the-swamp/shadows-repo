using System;
using UnityEngine;
using Utils;

public class ToScene : PlayerAction
{
    [SerializeField] protected SceneIndexes _toScene;

    public override void ActionatedBy(Player character, Action<string> Callback = null)
    {
        CargaNivel.NivelCarga((int)_toScene);
    }
}
