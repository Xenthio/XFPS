﻿using Sandbox.Diagnostics;
using System;

namespace XBase;

public class AmmoReserve : Component
{
	[Sync]
	public NetDictionary<AmmoTypeResource, int> AmmoCounts { get; set; } = new();

	public int GetAmmoCount( AmmoTypeResource resource )
	{
		Assert.NotNull( resource, "Ammo resource was null" );

		return AmmoCounts.GetValueOrDefault( resource, 0 );
	}

	[Rpc.Owner]
	public void GiveAmmo( AmmoTypeResource resource, int count )
	{
		if ( GetAmmoCount( resource ) + count > resource.MaxAmount )
		{
			count = resource.MaxAmount - GetAmmoCount( resource );
		}

		if ( count <= 0 )
			return;

		var amountGained = AddAmmoCount( resource, count );
	}

	public int SetAmmoCount( AmmoTypeResource resource, int count )
	{
		return AmmoCounts[resource] = count;
	}

	public int AddAmmoCount( AmmoTypeResource resource, int count )
	{
		var amountToGain = Math.Min( count, resource.MaxAmount - GetAmmoCount( resource ) );

		AmmoCounts[resource] = GetAmmoCount( resource ) + amountToGain;

		return amountToGain;
	}

	public int SubtractAmmoCount( AmmoTypeResource resource, int count )
	{
		var current = GetAmmoCount( resource );

		count = Math.Min( count, current );
		if ( count <= 0 )
			return 0;

		//if ( GameConfig.InfiniteAmmo )
		//return count;

		var to = current - count;

		AmmoCounts[resource] = to;
		return count;
	}
}
