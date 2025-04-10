using Sandbox.Rendering;
using System;
using XBase;
public class GlockWeapon : BaseGun
{
	[Property] public float Damage { get; set; } = 12.0f;

	public override void OnControl( Player player )
	{
		base.OnControl( player );


		if ( Input.Pressed( "attack1" ) )
		{
			ShootBullet( player, false );
		}

		if ( Input.Down( "attack2" ) )
		{
			ShootBullet( player, true );
		}
	}

	public void ShootBullet( Player player, bool secondary )
	{
		if ( !CanShoot() )
		{
			TryAutoReload();
			return;
		}

		if ( !TakeAmmo( 1 ) )
			return;

		if ( secondary )
		{
			AddShootDelay( 0.2f );
		}
		else
		{
			AddShootDelay( 0.15f );
		}

		var aimConeAmount = GetAimConeAmount();

		// Secondary fire has an increased aim cone
		if ( secondary ) aimConeAmount *= 2;

		var forward = player.Controller.AimRay.Forward;
		var bulletRadius = 2 * 1;

		var tr = Scene.Trace.Ray( player.Controller.AimRay with { Forward = forward }, 4096 )
							.IgnoreGameObjectHierarchy( player.GameObject )
							.WithCollisionRules( "bullet" )
							.Radius( bulletRadius )
							.UseHitboxes()
							.Run();

		ShootEffects( tr.EndPosition, tr.Hit, tr.Normal, tr.GameObject, tr.Surface );
		TraceAttack( TraceAttackInfo.From( tr, Damage ) );
		TimeSinceShoot = 0;

		player.Controller.EyeAngles += new Angles( Random.Shared.Float( -0.2f, -0.5f ), Random.Shared.Float( -1, 1 ) * 0.4f, 0 );

		/*if ( player.Controller.CameraMode != XMovement.PlayerWalkControllerComplex.CameraModes.ThirdPerson )
		{
			_ = new Sandbox.CameraNoise.Recoil( 1f, 0.3f );
		}*/
	}

	// returns 0 for no aim spread, 1 for full aim cone
	float GetAimConeAmount()
	{
		return 1 - TimeSinceShoot.Relative.Clamp( 0, 1 );
	}

	public override void DrawCrosshair( HudPainter hud, Vector2 center )
	{
		var gap = 10 + GetAimConeAmount() * 22;
		var len = 8;
		var w = 2f;

		Color color = !CanShoot() ? CrosshairNoShoot : CrosshairCanShoot;

		hud.SetBlendMode( BlendMode.Lighten );
		hud.DrawLine( center + Vector2.Left * (len + gap), center + Vector2.Left * gap, w, color );
		hud.DrawLine( center - Vector2.Left * (len + gap), center - Vector2.Left * gap, w, color );
		hud.DrawLine( center + Vector2.Up * (len + gap), center + Vector2.Up * gap, w, color );
		hud.DrawLine( center - Vector2.Up * (len + gap), center - Vector2.Up * gap, w, color );
	}
}
