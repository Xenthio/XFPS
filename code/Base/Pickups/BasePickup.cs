namespace XBase;

/// <summary>
/// A weapon, or weapons, or ammo, that can be picked up
/// </summary>
public abstract class BasePickup : Component, Component.ITriggerListener
{
	[RequireComponent] public Collider Collider { get; set; }

	[Property] public bool ShouldRespawn { get; set; } = false;
	[Property] public float RespawnTimer { get; set; } = 5;
	[Property] public SoundEvent PickupSound { get; set; }

	[Sync] public bool IsPickupEnabled { get; set; } = true;

	/// <summary>
	/// Give the player the effect of this pickup
	/// </summary>
	/// <returns>Should this object be consumed, eg on successful pickup</returns>
	protected virtual bool OnPickup( Player player, IInventory inventory )
	{
		return true;
	}

	/// <summary>
	/// Called when a gameobject enters the trigger.
	/// </summary>
	void ITriggerListener.OnTriggerEnter( GameObject other )
	{
		if ( !Networking.IsHost ) return;
		if ( GameObject.IsDestroyed ) return;
		if ( !IsPickupEnabled ) return;

		if ( !other.Root.Components.TryGet( out Player player ) )
			return;

		if ( !player.Components.TryGet( out IInventory inventory ) )
			return;

		if ( !OnPickup( player, inventory ) )
			return;

		PlayPickupEffects( player );

		if ( ShouldRespawn )
		{
			Disable();
			Invoke( RespawnTimer, Enable );
		}
		else
		{
			DestroyGameObject();
		}
	}

	[Rpc.Broadcast]
	private void PlayPickupEffects( Player player )
	{
		if ( Application.IsDedicatedServer ) return;

		var snd = GameObject.PlaySound( PickupSound );
		if ( !snd.IsValid() )
			return;

		if ( player.IsLocalPlayer )
		{
			snd.SpacialBlend = 0;
		}
	}


	[Rpc.Broadcast]
	private void PlayRespawnEffects()
	{
		if ( Application.IsDedicatedServer ) return;

		Sound.Play( "items/item_respawn.sound", WorldPosition );
	}

	private void Enable()
	{
		IsPickupEnabled = true;
		Collider.Enabled = true;

		foreach ( var child in GameObject.Children )
		{
			child.Enabled = true;
		}

		Network.Refresh();
		PlayRespawnEffects();
	}

	private void Disable()
	{
		IsPickupEnabled = false;
		Collider.Enabled = false;

		foreach ( var child in GameObject.Children )
		{
			child.Enabled = false;
		}

		Network.Refresh();
	}
}
