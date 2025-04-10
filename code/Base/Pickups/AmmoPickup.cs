namespace XBase;

public sealed class AmmoPickup : BasePickup
{
	[Property, Group( "Ammo" )] public AmmoTypeResource AmmoResource { get; set; }
	[Property, Group( "Ammo" )] public int AmmoAmount { get; set; }

	protected override bool OnPickup( Player player, IInventory inventory )
	{
		if ( player.Ammo.GetAmmoCount( AmmoResource ) == AmmoResource.MaxAmount )
			return false;

		player.Ammo.GiveAmmo( AmmoResource, AmmoAmount );
		player.PlayerData.AddStat( $"pickup.ammo.{AmmoResource.AmmoType}" );

		return true;
	}
}
