using System.Threading;
using System.Threading.Tasks;

namespace XBase;
public partial class BaseWeapon
{
	/// <summary>
	/// Should we consume 1 bullet per reload instead of filling the clip?
	/// </summary>
	[Property, Feature( "Ammo" )]
	public bool IncrementalReloading { get; set; } = false;

	/// <summary>
	/// Can we cancel reloads?
	/// </summary>
	[Property, Feature( "Ammo" )]
	public bool CanCancelReload { get; set; } = true;

	private CancellationTokenSource reloadToken;
	private bool isReloading;

	public bool CanReload()
	{
		if ( !UsesClips ) return false;
		if ( ClipContents >= ClipMaxSize ) return false;
		if ( isReloading ) return false;

		var owner = Owner;
		if ( !owner.IsValid() || owner.Ammo.GetAmmoCount( AmmoResource ) <= 0 )
			return false;

		return true;
	}

	public bool IsReloading() => isReloading;

	public virtual void CancelReload()
	{
		if ( reloadToken?.IsCancellationRequested == false )
		{
			reloadToken?.Cancel();
			isReloading = false;
		}
	}

	public virtual async void OnReloadStart()
	{
		if ( !CanReload() )
			return;

		CancelReload();

		try
		{
			reloadToken = new CancellationTokenSource();
			isReloading = true;

			await ReloadAsync( reloadToken.Token );
		}
		finally
		{
			reloadToken?.Dispose();
			reloadToken = null;
		}
	}

	protected virtual async Task ReloadAsync( CancellationToken ct )
	{
		try
		{
			IWeaponEvent.PostToGameObject( ViewModel, x => x.OnReloadStart() );

			while ( ClipContents < ClipMaxSize && !ct.IsCancellationRequested )
			{
				await Task.DelaySeconds( ReloadTime, ct );

				var owner = Owner;
				if ( !owner.IsValid() )
					break;

				var needed = IncrementalReloading ? 1 : (ClipMaxSize - ClipContents);
				var available = owner.Ammo.SubtractAmmoCount( AmmoResource, needed );

				if ( available <= 0 )
					break;

				ClipContents += available;
			}

			if ( ClipContents > 0 )
			{
				IWeaponEvent.PostToGameObject( ViewModel, x => x.OnReloadFinish() );
			}
		}
		finally
		{
			reloadToken?.Cancel();
			isReloading = false;
		}
	}
}
