using Sandbox.Rendering;
namespace XBase;

/// <summary>
/// Info about a trace attack. It's a struct so we can add to it without updating params everywhere.
/// </summary>
/// <param name="Target"></param>
/// <param name="Damage"></param>
/// <param name="Tags"></param>
/// <param name="Position"></param>
/// <param name="Origin"></param>
public record struct TraceAttackInfo( GameObject Target, float Damage, string Tags = null, Vector3 Position = default, Vector3 Origin = default )
{
	/// <summary>
	/// Constructs a <see cref="TraceAttackInfo"/> from a trace and input damage.
	/// </summary>
	/// <param name="tr"></param>
	/// <param name="damage"></param>
	/// <param name="tags"></param>
	/// <returns></returns>
	public static TraceAttackInfo From( SceneTraceResult tr, float damage, bool tags = true )
	{
		var tagSet = tags ? tr.Hitbox?.Tags is not null ? string.Join( " ", tr.Hitbox.Tags ) : null : null;
		return new TraceAttackInfo( tr.GameObject, damage, tagSet, tr.HitPosition, tr.StartPosition );
	}
}

public partial class BaseCarryable : Component
{
	[Property, Feature( "Inventory" ), Range( 0, 4 )] public int InventorySlot { get; set; } = 0;
	[Property, Feature( "Inventory" )] public int InventoryOrder { get; set; } = 0;
	[Property, Feature( "Inventory" )] public string DisplayName { get; set; } = "My Weapon";
	[Property, Feature( "Inventory" ), TextArea] public Texture DisplayIcon { get; set; }

	public GameObject ViewModel { get; protected set; }
	public GameObject WorldModel { get; protected set; }

	/// <summary>
	/// Gets a reference to the weapon model for this weapon - if there's a viewmodel, pick the viewmodel, if not, world model.
	/// </summary>
	public WeaponModel WeaponModel
	{
		get
		{
			var go = ViewModel;
			if ( !go.IsValid() ) go = WorldModel;
			if ( !go.IsValid() ) return null;

			return go.GetComponent<WeaponModel>();
		}
	}

	/// <summary>
	/// The owner of this carriable
	/// </summary>
	public Player Owner
	{
		get
		{
			return GetComponentInParent<Player>( true );
		}
	}

	protected override void OnEnabled()
	{
		CreateWorldModel();
	}

	protected override void OnDisabled()
	{
		DestroyWorldModel();
		DestroyViewModel();
	}

	protected override void OnUpdate()
	{
		var player = Owner;
		var controller = player?.Controller;
		if ( controller is null ) return;

		controller.BodyModelRenderer.Set( "holdtype", (int)HoldType );

		if ( player.IsLocalPlayer )
		{
			if ( Scene.Camera is null )
				return;

			var hud = Scene.Camera.Hud;

			var aimPos = Screen.Size * 0.5f;

			if ( controller.CameraMode == XMovement.PlayerWalkControllerComplex.CameraModes.ThirdPerson )
			{
				var tr = Scene.Trace.Ray( controller.AimRay, 4096 )
									.IgnoreGameObjectHierarchy( controller.GameObject )
									.Run();

				aimPos = Scene.Camera.PointToScreenPixels( tr.EndPosition );
			}

			DrawHud( hud, aimPos );
		}
	}

	public virtual void DrawHud( HudPainter painter, Vector2 crosshair )
	{
		// nothing
	}

	/// <summary>
	/// Called when added to the player's inventory
	/// </summary>
	/// <param name="player"></param>
	public virtual void OnAdded( Player player )
	{
		// nothing
	}

	/// <summary>
	/// Called when this is pulled out
	/// </summary>
	public virtual void OnEquipped( Player player )
	{

	}

	/// <summary>
	/// Called when this is put away
	/// </summary>
	public virtual void OnHolstered( Player player )
	{

	}

	public virtual void OnPlayerUpdate( Player player )
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

		if ( IsProxy )
			return;

		OnControl( player );
	}

	/// <summary>
	/// Called every update, scoped to the owning player
	/// </summary>
	/// <param name="player"></param>
	public virtual void OnControl( Player player )
	{
	}

	/// <summary>
	/// Called when setting up the camera - use this to apply effects on the camera based on this carriable
	/// </summary>
	/// <param name="player"></param>
	/// <param name="camera"></param>
	public virtual void OnCameraSetup( Player player, Sandbox.CameraComponent camera )
	{
	}

	/// <summary>
	/// Can directly influence the player's eye angles here
	/// </summary>
	/// <param name="player"></param>
	/// <param name="angles"></param>
	public virtual void OnCameraMove( Player player, ref Angles angles )
	{
	}

	/// <summary>
	/// Run a trace related attack with some set information.
	/// This is targeted to the host who then does things.
	/// </summary>
	/// <param name="attack"></param>
	[Rpc.Host]
	public void TraceAttack( TraceAttackInfo attack )
	{
		if ( !attack.Target.IsValid() )
			return;

		if ( !Owner.IsValid() )
			return;

		var damagable = attack.Target.GetComponentInParent<IDamageable>();
		if ( damagable is not null )
		{
			var info = new DamageInfo( attack.Damage, Owner.GameObject, GameObject );
			//info.InstigatorId = Owner.PlayerId;
			info.Position = attack.Position;
			//info.Origin = attack.Origin;

			if ( attack.Tags is not null )
			{
				// Aggregate tags
				info.Tags = [.. attack.Tags.Split( " " )];
			}

			damagable.OnDamage( info );
		}

		if ( attack.Target.GetComponentInChildren<Rigidbody>() is var rb && rb.IsValid() )
		{
			rb.ApplyForce( (attack.Position - attack.Origin) * 1000f );
		}
	}
}
