This is the Economy API project.
The files are designed to be integrated directly into the caller mod project, to allow the caller mod to communicate with the Economy Mod.

All calls to the Economy Mod must be done on the Server side. Any calls from another mod made from a Client side will be ignored.
The exception is the Host of a Multi Player server.

This can be tested on a Multi Player Host, or single player offline mode as all Economy can work single or multi player.

Current working API:
EconPayUser


Callback:
To receive callbacks, the EconManagement must be substantiated, and Subscribe called with the preferred callback channel unique to the mod caller.
The same callback channel must be passed into the API utilised.

readonly EconomyAPI.EconManagement econoManagement = new EconManagement();

public override void UpdateBeforeSimulation()
{
	econoManagement.Subscribe(55555);
}

protected override void UnloadData()
{
	econoManagement.Unsubscribe();
}

// Test code
EconomyAPI.EconPayUser.SendMessage(MyAPIGateway.Session.Player.SteamUserId, 1234, 55.4m, "test20", 55555, 123);

Parameters are:
(from Player Identity, to Player Identity, transaction Amount, reason, callback Mod Channel, transactionId)

Player identities are /either/ their steam user id, OR the chosen NPC player ID in ulong format
transaction amount is decimal format
reason is string format
callback is long format
transactionid is long format and should be unique per transaction
