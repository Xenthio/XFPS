namespace XBase;

public class BaseGun : BaseWeapon
{
	[Property]
	public SoundEvent ShootSound { get; set; }


	public TimeSince TimeSinceShoot;


	[Rpc.Broadcast]
	public void ShootEffects( Vector3 hitpoint, bool hit, Vector3 normal, GameObject hitObject, Surface hitSurface, Vector3? origin = null, bool noEvents = false )
	{
		if ( Application.IsDedicatedServer ) return;

		if ( !Owner.IsValid() )
			return;

		Owner.Controller.BodyModelRenderer.Set( "b_attack", true );

		if ( !noEvents )
		{
			var ev = new IWeaponEvent.AttackEvent( ViewModel.IsValid() );
			IWeaponEvent.PostToGameObject( GameObject.Root, x => x.OnAttack( ev ) );
			IWeaponEvent.PostToGameObject( GameObject.Root, x => x.CreateRangedEffects( this, hitpoint, origin ) );

			if ( ShootSound.IsValid() )
			{
				var snd = GameObject.PlaySound( ShootSound );
				// If we're shooting, the sound should not be spatialized
				if ( Owner.IsLocalPlayer )
				{
					snd.SpacialBlend = 0;
				}
			}
		}

		if ( hit )
		{
			var impactObjects = SurfaceImpacts.FindForResourceOrDefault( hitSurface );

			//if ( impactObjects is not null )
			{
				//DebugOverlay.Text( hitpoint, $"Surface: {hitSurface}\nImpact: {impactObjects}", 16, duration: 10, overlay: true );
			}

			if ( impactObjects.BulletImpact is not null )
			{
				var impact = impactObjects.BulletImpact.Clone();
				impact.WorldPosition = hitpoint + normal;
				impact.WorldRotation = Rotation.LookAt( normal );
				impact.SetParent( hitObject, true );
			}

			if ( impactObjects.BulletDecal is not null )
			{
				var decal = impactObjects.BulletDecal.Clone();
				decal.WorldPosition = hitpoint + normal;
				decal.WorldRotation = Rotation.LookAt( -normal );
				decal.WorldScale = 1;
				decal.Parent = Scene;
				decal.SetParent( hitObject, true );
			}
		}
	}
}
