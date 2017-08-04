namespace OrbsHavoc.Network.Messages
{
	/// <summary>
	///   Identifies the type of a message.
	/// </summary>
	internal enum MessageType : byte
	{
		ClientConnect = 1,
		ClientRejected = 107,
		ClientSynced = 10,
		Disconnect = 106,
		EntityAdd = 6,
		EntityCollision = 105,
		EntityRemove = 7,
		PlayerChat = 5,
		PlayerInput = 103,
		PlayerJoin = 3,
		PlayerKill = 11,
		PlayerLeave = 4,
		PlayerName = 9,
		PlayerStats = 101,
		UpdateCircle = 113,
		UpdateOrb = 111,
		UpdateRay = 112,
		UpdateTransform = 110,
	}
}