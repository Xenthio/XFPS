using Sandbox.Rendering;
using XBase;

public class Mp5Weapon : BaseGun
{
	[Property] public float TimeBetweenShots { get; set; } = 0.1f;
	[Property] public float Damage { get; set; } = 12.0f;
	[Property] public GameObject ProjectilePrefab { get; set; }
	[Property] AmmoTypeResource ProjectileAmmoResource { get; set; }
	public override void OnControl( Player player )
	{
		base.OnControl( player );

		if ( Input.Down( "attack1" ) )
		{
			ShootBullet( player );
		}

		if ( Input.Pressed( "Attack2" ) )
		{
			ShootProjectile( player );
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

	public void ShootProjectile( Player player )
	{
		if ( IsReloading() ) return;
		if ( TimeUntilNextSecondaryShotAllowed > 0 ) return;

		if ( !TakeAmmo( 1, ProjectileAmmoResource ) )
		{
			// Empty/click sound
			AddShootDelay( 0.2f );
			return;
		}

		AddSecondaryShootDelay( 1f );

		var forward = player.EyeTransform.Rotation.Forward;
		forward = forward.Normal;

		var tr = Scene.Trace.Ray( player.EyeTransform.ForwardRay with { Forward = forward }, 4096 )
							.IgnoreGameObjectHierarchy( player.GameObject )
							.WithCollisionRules( "bullet" )
							.UseHitboxes()
							.Run();

		TimeSinceShoot = 0;

		var ev = new IWeaponEvent.AttackEvent( ViewModel.IsValid() );
		IWeaponEvent.PostToGameObject( GameObject.Root, x => x.OnAttack( ev ) );

		// ShootEffects( tr.EndPosition );
		CreateProjectile( tr.StartPosition + tr.Direction * 64f + (player.EyeTransform.Right * 16f) + (player.EyeTransform.Down * 8f), tr.Direction, 1024 );

		player.Controller.EyeAngles += new Angles( Random.Shared.Float( -0.2f, -0.3f ), Random.Shared.Float( -0.1f, 0.1f ), 0 );

		if ( !player.Controller.ThirdPerson )
		{
			new Sandbox.CameraNoise.Punch( new Vector3( Random.Shared.Float( 45, 35 ), Random.Shared.Float( -10, -5 ), 0 ), 1.5f, 2, 0.5f );
			new Sandbox.CameraNoise.Shake( 1f, 0.6f );
		}
	}

	/// <summary>
	/// Creates the projectile with the host's permission
	/// </summary>
	/// <param name="start"></param>
	/// <param name="direction"></param>
	/// <param name="speed"></param>
	[Rpc.Host]
	void CreateProjectile( Vector3 start, Vector3 direction, float speed )
	{
		if ( !Owner.IsValid() ) return;

		var go = ProjectilePrefab?.Clone( start );

		var projectile = go.GetComponent<ExplosiveProjectile>();
		Assert.True( projectile.IsValid(), "RpgProjectile not on projectile prefab" );

		projectile.InstigatorId = Owner.PlayerId;
		projectile.Explosive.InstigatorId = Owner.PlayerId;
		projectile.WorldRotation = Rotation.LookAt( direction, Vector3.Up );
		projectile.Rigidbody.Velocity = projectile.WorldRotation.Forward * 2048 + Vector3.Up * 256;

		go.NetworkSpawn();
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
		var forward = player.EyeTransform.Rotation.Forward.WithAimCone( 0.5f + aimConeAmount * 8f, 0.25f + aimConeAmount * 4f );
		var bulletRadius = GameConfig.Mp5BulletRadius * GameConfig.BulletRadius;

		var tr = Scene.Trace.Ray( player.EyeTransform.ForwardRay with { Forward = forward }, 4096 )
							.IgnoreGameObjectHierarchy( player.GameObject )
							.WithCollisionRules( "bullet" )
							.Radius( bulletRadius )
							.UseHitboxes()
							.Run();

		Log.Info( "shoot" );
		ShootEffects( tr.EndPosition, tr.Hit, tr.Normal, tr.GameObject, tr.Surface );
		TraceAttack( TraceAttackInfo.From( tr, Damage ) );
		TimeSinceShoot = 0;

		if ( player.IsLocalPlayer )
		{
			HitMarker.CreateFromTrace( tr );
		}

		player.Controller.EyeAngles += new Angles( Random.Shared.Float( -0.1f, -0.3f ), Random.Shared.Float( -0.1f, 0.1f ), 0 );

		if ( !player.Controller.ThirdPerson )
		{
			new Sandbox.CameraNoise.Recoil( 1.0f, 1 );
		}
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

		var player = Owner;
		var ammoCount = player.GetAmmoCount( ProjectileAmmoResource );
		var str = $"{ammoCount}";

		var text = new TextRendering.Scope( str, Color.White, 32 );
		text.TextColor = (ammoCount == 0) ? CrosshairNoShoot : "#f80";
		text.FontName = "Poppins";
		text.FontWeight = 450;
		text.Shadow = new TextRendering.Shadow { Enabled = true, Color = "#f506", Offset = 0, Size = 2 };

		hud.SetBlendMode( BlendMode.Lighten );
		hud.DrawText( text, new Rect( bottomright - 100 - new Vector2( 0, 42 ), 100 ), TextFlag.RightBottom );
	}

}
