using XMovement;
namespace XBase;

public class Player : Component
{
	[RequireComponent] public PlayerData PlayerData { get; set; }
	[RequireComponent] public IInventory Inventory { get; set; }
	[RequireComponent] public AmmoReserve Ammo { get; set; }

	[RequireComponent]
	public PlayerWalkControllerComplex Controller { get; set; }

	public bool IsBot => false; //Components.Get<PlayerBotController>() != null;
	public bool IsLocalPlayer => !IsProxy && !IsBot;
}
