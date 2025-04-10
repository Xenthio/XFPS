namespace XBase;

using static BaseWeapon;

public sealed partial class ViewModel : WeaponModel, IWeaponEvent, ICameraSetup
{
	Vector2 InertiaScale => new( 5, 5 );
	Vector2 Last;
	Vector2 Inertia;
	bool FirstUpdate = true;

	void ApplyInertia()
	{
		// Need to fetch data from the camera for the first frame
		if ( FirstUpdate )
		{
			var rot = Scene.Camera.WorldRotation;

			Last = new Vector2( rot.Pitch(), rot.Yaw() );
			Inertia = Vector2.Zero;
			FirstUpdate = false;
		}

		var newPitch = Scene.Camera.WorldRotation.Pitch();
		var newYaw = Scene.Camera.WorldRotation.Yaw();

		Inertia = new Vector2( Angles.NormalizeAngle( newPitch - Last.x ), Angles.NormalizeAngle( Last.y - newYaw ) );
		Last = new( newPitch, newYaw );
	}

	protected override void OnStart()
	{
		foreach ( var renderer in GetComponentsInChildren<ModelRenderer>() )
		{
			// Don't render shadows for viewmodels
			renderer.RenderType = ModelRenderer.ShadowRenderType.Off;
		}
	}
	protected override void OnUpdate()
	{
		UpdateAnimation();
	}

	void ICameraSetup.Setup( Sandbox.CameraComponent cc )
	{
		WorldPosition = cc.WorldPosition;
		WorldRotation = cc.WorldRotation;

		ApplyInertia();
	}

	void UpdateAnimation()
	{
		var playerController = GetComponentInParent<PlayerController>();
		if ( playerController is null ) return;

		Renderer.Set( "b_twohanded", true );
		Renderer.Set( "b_grounded", playerController.IsOnGround );
		Renderer.Set( "move_bob", /*GameSettings.ViewBobbing*/ true ? playerController.Velocity.Length.Remap( 0, playerController.RunSpeed * 2f ) : 0 );

		Renderer.Set( "aim_pitch_inertia", Inertia.x * InertiaScale.x );
		Renderer.Set( "aim_yaw_inertia", Inertia.y * InertiaScale.y );
	}

	void IWeaponEvent.OnAttack( IWeaponEvent.AttackEvent e )
	{
		Renderer?.Set( "b_attack", true );

		DoMuzzleEffect();
		DoEjectBrass();

		if ( IsThrowable )
		{
			Renderer?.Set( "b_throw", true );

			Invoke( 0.5f, () =>
			{
				Renderer?.Set( "b_deploy_new", true );
				Renderer?.Set( "b_pull", false );
			} );
		}
	}

	void IWeaponEvent.CreateRangedEffects( BaseWeapon weapon, Vector3 hitPoint, Vector3? origin )
	{
		DoTracerEffect( hitPoint, origin );
	}

	void IWeaponEvent.OnAttackStop()
	{
	}

	void IWeaponEvent.OnReloadStart()
	{
		Renderer?.Set( "b_reload", true );
	}

	void IWeaponEvent.OnReloadFinish()
	{
		// We have to toggle this again to stop a reload with the shotgun - since we're not using the incremental reload steps
		// (too much bs imo)
		Renderer?.Set( "b_reload", true );
	}
}
