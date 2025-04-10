using Sandbox.Rendering;
using System;
using XBase;

public class Mp5Weapon : BaseGun
{
	[Property] public float TimeBetweenShots { get; set; } = 0.1f;
	[Property] public float Damage { get; set; } = 12.0f;
	public override void OnControl( Player player )
	{
		base.OnControl( player );

		if ( Input.Down( "attack1" ) )
		{
			ShootBullet( player );
		}
	}

	/// <summary>
	/// How long until we can shoot again
	/// </summary>
	protected TimeUntil TimeUntilNextSecondaryShotAllowed;

	/// <summary>
	/// Adds a delay, making it so we can't shoot for the specified time
	/// </summary>
	/// <param name="seconds"></param>
	public void AddSecondaryShootDelay( float seconds )
	{
		TimeUntilNextSecondaryShotAllowed = seconds;
	}

	public void ShootBullet( Player player )
	{
		if ( !CanShoot() )
		{
			TryAutoReload();
			return;
		}

		if ( !TakeAmmo( 1 ) )
		{
			AddShootDelay( 0.2f );
			return;
		}

		AddShootDelay( TimeBetweenShots );

		var aimConeAmount = GetAimConeAmount();
		var forward = player.Controller.AimRay.Forward;
		var bulletRadius = 2 * 1;

		var tr = Scene.Trace.Ray( player.Controller.AimRay with { Forward = forward }, 4096 )
							.IgnoreGameObjectHierarchy( player.GameObject )
							.WithCollisionRules( "bullet" )
							.Radius( bulletRadius )
							.UseHitboxes()
							.Run();

		Log.Info( "shoot" );
		ShootEffects( tr.EndPosition, tr.Hit, tr.Normal, tr.GameObject, tr.Surface );
		TraceAttack( TraceAttackInfo.From( tr, Damage ) );
		TimeSinceShoot = 0;

		player.Controller.EyeAngles += new Angles( Random.Shared.Float( -0.1f, -0.3f ), Random.Shared.Float( -0.1f, 0.1f ), 0 );

		/*		if ( player.Controller.CameraMode != XMovement.PlayerWalkControllerComplex.CameraModes.ThirdPerson )
				{
					new Sandbox.CameraNoise.Recoil( 1.0f, 1 );
				}*/
	}

	// returns 0 for no aim spread, 1 for full aim cone
	float GetAimConeAmount()
	{
		return TimeSinceShoot.Relative.Remap( 0, 0.2f, 1, 0 );
	}

	public override void DrawCrosshair( HudPainter hud, Vector2 center )
	{
		var gap = 6 + GetAimConeAmount() * 32;
		var len = 6;
		var w = 2;

		Color color = !CanShoot() ? CrosshairNoShoot : CrosshairCanShoot;

		hud.SetBlendMode( BlendMode.Lighten );
		hud.DrawLine( center + Vector2.Left * (len + gap) * 2, center + Vector2.Left * gap * 2, w, color );
		hud.DrawLine( center - Vector2.Left * (len + gap) * 2, center - Vector2.Left * gap * 2, w, color );
		hud.DrawLine( center + Vector2.Up * (len + gap), center + Vector2.Up * gap, w, color );
		hud.DrawLine( center - Vector2.Up * (len + gap), center - Vector2.Up * gap, w, color );

	}

	public override void DrawAmmo( HudPainter hud, Vector2 bottomright )
	{
		base.DrawAmmo( hud, bottomright );
	}

}
