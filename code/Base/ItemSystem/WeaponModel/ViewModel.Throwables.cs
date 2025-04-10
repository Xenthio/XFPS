namespace XBase;

using static BaseWeapon;

/// <summary>
/// This is maybe a bit shitty - we could instead just have a ThrowableViewModel and not stick IsThrowable checks everywhere 
/// </summary>
public partial class ViewModel
{
	public enum Throwable
	{
		HEGrenade,
		SmokeGrenade,
		StunGrenade,
		Molotov,
		Flashbang
	}

	[Property, FeatureEnabled( "Throwables" )] public bool IsThrowable { get; set; }
	[Property, Feature( "Throwables" )] public Throwable ThrowableType { get; set; }

	protected override void OnEnabled()
	{
		if ( IsThrowable )
		{
			Renderer?.Set( "throwable_type", (int)ThrowableType );
		}
	}

	void IWeaponEvent.OnAttackStart( IWeaponEvent.AttackEvent ev )
	{
		if ( IsThrowable )
		{
			Renderer?.Set( "b_pull", true );
		}
		else
		{
			Renderer?.Set( "b_attack", true );
		}
	}
}
