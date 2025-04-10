using Sandbox.Diagnostics;
using System;
using XBase;

namespace Base;

internal class BucketInventory : Component, IInventory
{
	[RequireComponent] public Player Player { get; set; }

	public List<BaseCarryable> Items => GetComponentsInChildren<BaseCarryable>( true ).ToList();

	[Sync] public BaseCarryable CurrentItem { get; private set; }

	public void SetCurrentItem( BaseCarryable item )
	{
		CurrentItem = item;
	}

	public BaseCarryable GetCurrentItem()
	{
		return CurrentItem;
	}

	public bool AddItem( BaseCarryable item )
	{
		if ( !Networking.IsHost )
			return false;


		var existing = Items.Where( x => x.GetType() == item.GetType() ).FirstOrDefault();
		if ( existing.IsValid() )
		{
			// We already have this weapon type

			if ( item is BaseWeapon baseWeapon && baseWeapon.UsesAmmo )
			{
				var ammo = baseWeapon.AmmoResource;
				if ( ammo is null )
					return false;

				if ( Player.Ammo.GetAmmoCount( ammo ) >= ammo.MaxAmount )
					return false;

				Player.Ammo.GiveAmmo( baseWeapon.AmmoResource, baseWeapon.ClipContents );
				OnClientPickup( existing, true );
			}

			item.DestroyGameObject();
			return true;
		}

		item.GameObject.Parent = GameObject;

		if ( Network.Owner is not null )
		{
			item.Network.AssignOwnership( Network.Owner );
		}
		else
		{
			item.Network.DropOwnership();
		}

		//IPlayerEvent.PostToGameObject( GameObject, e => e.OnPickup( item ) );
		OnClientPickup( item );
		return true;
	}

	public bool RemoveItem( BaseCarryable item )
	{
		throw new NotImplementedException();
	}

	public void GiveDefaultWeapons()
	{
		Pickup( "weapons/crowbar/crowbar.prefab" );
		//	Pickup( "weapons/hands/hands.prefab" );
		//	Pickup( "weapons/camera/camera.prefab" );
		Pickup( "weapons/glock/glock.prefab" );

		Player.Ammo.GiveAmmo( ResourceLibrary.Get<AmmoResource>( "weapons/ammo/9mm.ammo" ), 100 );
	}

	public void GiveAll()
	{
		Pickup( "weapons/mp5/mp5.prefab" );
		Pickup( "weapons/gaussgun/gauss.prefab" );
		Pickup( "weapons/python/python.prefab" );
		Pickup( "weapons/handgrenade/hand_grenade.prefab" );
		Pickup( "weapons/tripmine/tripmine.prefab" );
		Pickup( "weapons/satchelcharge/satchelcharge.prefab" );
		Pickup( "weapons/rpg/rpg.prefab" );
		Pickup( "weapons/crossbow/crossbow.prefab" );
		Pickup( "weapons/shotgun/shotgun.prefab" );
		Pickup( "weapons/ratgun/rat_gun.prefab" );
		Pickup( "weapons/gluongun/gluon_gun.prefab" );
		Pickup( "weapons/hornetgun/hornetgun.prefab" );

		var ammo = ResourceLibrary.GetAll<AmmoResource>();
		foreach ( var a in ammo )
		{
			Player.Ammo.GiveAmmo( a, 100 );
		}
	}

	public bool Pickup( string prefabName, bool notice = true )
	{
		if ( !Networking.IsHost )
			return false;

		var prefab = GameObject.GetPrefab( prefabName );
		return Pickup( prefab, notice );
	}

	public bool Pickup( GameObject prefab, bool notice = true )
	{
		if ( !Networking.IsHost )
			return false;

		var baseCarry = prefab.Components.Get<BaseCarryable>( true );
		if ( baseCarry is null )
			return false;

		var existing = Items.Where( x => x.GetType() == baseCarry.GetType() ).FirstOrDefault();
		if ( existing.IsValid() )
		{
			// We already have this weapon type

			if ( baseCarry is BaseWeapon baseWeapon && baseWeapon.UsesAmmo )
			{
				var ammo = baseWeapon.AmmoResource;
				if ( ammo is null )
					return false;

				if ( Player.Ammo.GetAmmoCount( ammo ) >= ammo.MaxAmount )
					return false;

				Player.Ammo.GiveAmmo( ammo, baseWeapon.UsesClips ? baseWeapon.ClipContents : baseWeapon.StartingAmmo );
				OnClientPickup( existing, true );
				return true;
			}

			return false;
		}

		var clone = prefab.Clone( new CloneConfig { Parent = GameObject, StartEnabled = false } );
		clone.NetworkSpawn( false, Network.Owner );

		var weapon = clone.Components.Get<BaseCarryable>( true );
		Assert.NotNull( weapon );

		weapon.OnAdded( Player );

		IPlayerEvent.PostToGameObject( Player.GameObject, e => e.OnPickup( weapon ) );
		OnClientPickup( weapon );
		return true;
	}

	public void Take( BaseCarryable item, bool includeNotices )
	{
		var existing = Items.Where( x => x.GetType() == item.GetType() ).FirstOrDefault();
		if ( existing.IsValid() )
		{
			// We already have this weapon type

			if ( item is BaseWeapon baseWeapon && baseWeapon.UsesAmmo )
			{
				var ammo = baseWeapon.AmmoResource;
				if ( ammo is null )
					return;

				if ( Player.Ammo.GetAmmoCount( ammo ) >= ammo.MaxAmount )
					return;

				Player.Ammo.GiveAmmo( baseWeapon.AmmoResource, baseWeapon.ClipContents );
				OnClientPickup( existing, true );
			}

			item.DestroyGameObject();
			return;
		}

		item.GameObject.Parent = GameObject;

		if ( Network.Owner is not null )
		{
			item.Network.AssignOwnership( Network.Owner );
		}
		else
		{
			item.Network.DropOwnership();
		}

		IPlayerEvent.PostToGameObject( GameObject, e => e.OnPickup( item ) );
		OnClientPickup( item );
	}

	[Rpc.Owner]
	private void OnClientPickup( BaseCarryable weapon, bool justAmmo = false )
	{
		if ( !weapon.IsValid() ) return;

		if ( Player.IsLocalPlayer )
			ILocalPlayerEvent.Post( e => e.OnPickup( weapon ) );
	}


	public void SwitchWeapon( BaseCarryable weapon )
	{
		if ( weapon == CurrentItem ) return;

		if ( CurrentItem.IsValid() )
		{
			CurrentItem.OnHolstered( Player );
			CurrentItem.GameObject.Enabled = false;
		}

		CurrentItem = weapon;

		if ( CurrentItem.IsValid() )
		{
			CurrentItem.OnEquipped( Player );
			CurrentItem.GameObject.Enabled = true;
		}
	}

	protected override void OnUpdate()
	{
		if ( CurrentItem.IsValid() )
		{
			CurrentItem.OnPlayerUpdate( Player );
		}
	}


	public virtual void DropOnDeath()
	{
		if ( !Networking.IsHost )
			return;
	}
}
