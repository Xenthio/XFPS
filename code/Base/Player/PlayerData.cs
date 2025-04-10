using Sandbox.Diagnostics;
using System;
namespace XBase;
/// <summary>
/// Holds persistent player information like deaths, kills. This is from facepunch.sbdm. A system similar to the old Pawn system.
/// </summary>
public sealed partial class PlayerData : Component
{
	/// <summary>
	/// Unique Id per each player and bot, equal to owning Player connection Id if it's a real player.
	/// </summary>
	[Property] public Guid PlayerId { get; set; }
	[Property] public long SteamId { get; set; } = -1L;
	[Property] public string DisplayName { get; set; }
	[Property] public int BotId { get; set; } = -1;
	public bool IsBot => BotId != -1;

	[Sync] public int Kills { get; set; }
	[Sync] public int Deaths { get; set; }

	[Sync] public bool IsGodMode { get; set; }

	public Connection Connection => Connection.Find( PlayerId );

	/// <summary>
	/// Is this player data me?
	/// </summary>
	public bool IsMe => PlayerId == Connection.Local.Id;

	/// <inheritdoc cref="Connection.Ping"/>
	public float Ping => Connection?.Ping ?? 0;

	/// <summary>
	/// Data for all players
	/// </summary>
	public static IEnumerable<PlayerData> All => Game.ActiveScene.GetAll<PlayerData>();

	/// <summary>
	/// Get player data for a player
	/// </summary>
	/// <param name="connection"></param>
	/// <returns></returns>
	public static PlayerData For( Connection connection ) => For( connection.Id );

	/// <summary>
	/// Get player data for a player's id
	/// </summary>
	/// <param name="playerId"></param>
	/// <returns></returns>
	public static PlayerData For( Guid playerId )
	{
		return All.FirstOrDefault( x => x.PlayerId == playerId );
	}

	[Rpc.Broadcast]
	private void RpcAddStat( string identifier, int amount = 1 )
	{
		Sandbox.Services.Stats.Increment( identifier, amount );
	}

	/// <summary>
	/// Called on the host, calls a RPC on the player and adds a stat
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="amount"></param>
	public void AddStat( string identifier, int amount = 1 )
	{
		Assert.True( Networking.IsHost, "PlayerData.AddStat is host-only!" );

		using ( Rpc.FilterInclude( Connection ) )
		{
			RpcAddStat( identifier, amount );
		}
	}
}
