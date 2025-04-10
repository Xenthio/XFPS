namespace XBase;
public sealed class InventoryPickup : BasePickup
{
	[Property, Group( "Inventory" )] public List<GameObject> Items { get; set; }

	protected override bool OnPickup( Player player, IInventory inventory )
	{
		if ( Items == null ) return false;

		bool consumed = false;
		foreach ( var prefab in Items )
		{
			if ( inventory.AddItemFromPrefab( prefab ) )
			{
				consumed = true;
				player.PlayerData.AddStat( $"pickup.inventory.{prefab.Name}" );
			}
		}

		return consumed;
	}
}
