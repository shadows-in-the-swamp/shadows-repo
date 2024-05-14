
using Unity.VisualScripting;

public class PrimaryItem : Item
{
    public override void PickedBy(Player player)
    {
        player.AddPrimaryItem(this);
        gameObject.SetActive(false);
    }
}
