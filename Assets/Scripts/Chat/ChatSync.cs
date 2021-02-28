using UnityEngine;
using Mirror;
using SanAndreasUnity.Net;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Chat
{
	
	public class ChatSync : NetworkBehaviour
	{

		Player m_player;


		void Awake()
		{
			m_player = this.GetComponentOrThrow<Player>();
		}

		[Command]
		void	CmdChatMsg( string msg ) {
			
			Player p = m_player;

			msg = ChatManager.ProcessChatMessage(msg, false);
			if (string.IsNullOrEmpty(msg))
				return;

			F.RunExceptionSafe(() => ChatManager.singleton.OnChatMessageReceivedOnServer(p, msg));

		}

		internal	void	SendChatMsgToServer( string msg )
		{
			this.CmdChatMsg(msg);
		}

		[TargetRpc]
		void	TargetChatMsg( NetworkConnection conn, string msg, string sender ) {

			if (!this.isLocalPlayer) {
				return;
			}

			msg = ChatManager.ProcessChatMessage(msg, true);
			if (string.IsNullOrEmpty(msg))
				return;

			F.RunExceptionSafe(() => ChatManager.singleton.OnChatMessageReceivedOnLocalPlayer(new ChatMessage (msg, sender)));

		}

		internal	void	SendChatMsgToClient( NetworkConnection conn, string msg, string sender )
		{
			NetStatus.ThrowIfNotOnServer();

			this.TargetChatMsg(conn, msg, sender);
		}

	}

}
