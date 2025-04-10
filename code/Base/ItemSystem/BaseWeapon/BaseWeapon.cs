using Sandbox.Rendering;
namespace XBase;
public partial class BaseWeapon : BaseCarryable
{
	/// <summary>
	/// How long after deploying a weapon can you not shoot a gun?
	/// </summary>
	[Property] public float DeployTime { get; set; } = 0.5f;

	/// <summary>
	/// How long until we can shoot again
	/// </summary>
	protected TimeUntil TimeUntilNextShotAllowed;

	/// <summary>
	/// Adds a delay, making it so we can't shoot for the specified time
	/// </summary>
	/// <param name="seconds"></param>
	public void AddShootDelay( float seconds )
	{
		TimeUntilNextShotAllowed = seconds;
	}

	/// <summary>
	/// The dry fire sound if we have no ammo
	/// </summary>
	private static SoundEvent DryFireSound = new SoundEvent( "audio/sounds/dry_fire.sound" );

	/// <summary>
	/// Play a dry fire sound. You should only call this on weapons that can't auto reload - if they can, use <see cref="TryAutoReload"/> instead.
	/// </summary>
	public void DryFire()
	{
		if ( HasAmmo() )
			return;

		if ( IsReloading() )
			return;

		if ( TimeUntilNextShotAllowed > 0 )
			return;

		GameObject.PlaySound( DryFireSound );
	}

	/// <summary>
	/// Player has fired an empty gun - play dry fire sound and start reloading. You should only call this on weapons that can reload - if they can't, use <see cref="DryFire"/> instead.
	/// </summary>
	public virtual void TryAutoReload()
	{
		if ( HasAmmo() )
			return;

		if ( IsReloading() )
			return;

		if ( TimeUntilNextShotAllowed > 0 )
			return;

		DryFire();

		AddShootDelay( 0.1f );

		if ( CanReload() )
			OnReloadStart();
		//else
		//SwitchAway();
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		AddShootDelay( DeployTime );
	}

	public override void OnAdded( Player player )
	{
		base.OnAdded( player );

		if ( AmmoResource is not null && StartingAmmo > 0 )
		{
			// When this weapon gets added to a player's inventory, give player some ammo
			player.Ammo.GiveAmmo( AmmoResource, StartingAmmo );
		}
	}

	/// <summary>
	/// Are we allowed to shoot this weapon? Can be overriden per-weapon
	/// </summary>
	/// <returns></returns>
	public virtual bool CanShoot()
	{
		if ( !HasAmmo() ) return false;
		if ( IsReloading() ) return false;
		if ( TimeUntilNextShotAllowed > 0 ) return false;

		return true;
	}

	public override void DrawHud( HudPainter painter, Vector2 crosshair )
	{
		DrawCrosshair( painter, crosshair );
		DrawAmmo( painter, Screen.Size * 0.9f );
	}

	public override void OnPlayerUpdate( Player player )
	{
		if ( player is null ) return;

		if ( player.Controller.CameraMode != XMovement.PlayerWalkControllerComplex.CameraModes.ThirdPerson )
		{
			CreateViewModel();
		}
		else
		{
			DestroyViewModel();
		}

		GameObject.NetworkInterpolation = false;

		if ( !player.IsLocalPlayer )
			return;

		OnControl( player );
	}

	public override void OnControl( Player player )
	{
		bool wantsToCancelReload = Input.Pressed( "Attack1" ) || Input.Pressed( "Attack2" );
		if ( CanCancelReload && IsReloading() && wantsToCancelReload && HasAmmo() )
		{
			CancelReload();
		}

		if ( CanReload() && Input.Pressed( "reload" ) )
		{
			OnReloadStart();
		}
	}

	public virtual void DrawCrosshair( HudPainter hud, Vector2 center )
	{
		Color color = Color.Red;

		hud.DrawLine( center + Vector2.Left * 32, center + Vector2.Left * 15, 3, color );
		hud.DrawLine( center - Vector2.Left * 32, center - Vector2.Left * 15, 3, color );
		hud.DrawLine( center + Vector2.Up * 32, center + Vector2.Up * 15, 3, color );
		hud.DrawLine( center - Vector2.Up * 32, center - Vector2.Up * 15, 3, color );
	}

	public virtual void DrawAmmo( HudPainter hud, Vector2 bottomright )
	{
		Color color = Color.Red;

		var owner = Owner;
		if ( owner is null ) return;

		var str = $"{ClipContents} / {owner.Ammo.GetAmmoCount( AmmoResource )}";
		if ( !UsesClips ) str = $"{owner.Ammo.GetAmmoCount( AmmoResource )}";

		var text = new TextRendering.Scope( str, Color.White, 32 );
		text.TextColor = (ClipContents == 0) ? CrosshairNoShoot : "#f80";
		text.FontName = "Poppins";
		text.FontWeight = 450;
		text.Shadow = new TextRendering.Shadow { Enabled = true, Color = "#f506", Offset = 0, Size = 2 };

		hud.SetBlendMode( BlendMode.Lighten );
		hud.DrawText( text, new Rect( bottomright - 100, 100 ), TextFlag.RightBottom );
	}

	protected Color CrosshairCanShoot => Color.Yellow;
	protected Color CrosshairNoShoot => Color.Red;
}
