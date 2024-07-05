
using System;
using Utils;

public class ConfineWeakness : Weakness<Confine>
{
    public override void ActionatedBy(Player player, Action<string> Callback = null)
    {
        bool success = false;
        void ConfineCallback(string eventName)
        {
            switch (eventName)
            {
                case "Success":
                    success = true;
                    player.ThrowSalt();
                    _enemy.SetState(Bounded.Instance);
                    break;
                case "Done":
                    if (success)
                    {
                        Deactivate();
                    }
                    break;
            }
        }
        player.CastSpell<Confine>(Callback + ConfineCallback);
    }
}
