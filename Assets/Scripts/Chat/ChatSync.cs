using UnityEngine;
using Mirror;
using SanAndreasUnity.Net;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Chat
{
	
	public class ChatSync : NetworkBehaviour
	{

		Player m_player;

		public	static	event System.Action<Player, string>	onChatMessageReceivedOnServer = delegate {};
		public	static	event System.Action<ChatMessage>	onChatMessageReceivedOnLocalPlayer = delegate {};


		void Awake()
		{
			m_player = this.GetComponentOrThrow<Player>();
		}

		[Command]
		void	CmdChatMsg( string msg ) {
			
			Player p = m_player;

			msg = ChatManager.RemoveInvalidCharacters(msg, false);
			if (string.IsNullOrEmpty(msg))
				return;

			F.InvokeEventExceptionSafe(onChatMessageReceivedOnServer, p, msg);

		}

		public	void	SendChatMsgToServer( string msg )
		{
			this.CmdChatMsg(msg);
		}

		[TargetRpc]
		void	TargetChatMsg( NetworkConnection conn, string msg, string sender ) {

			if (!this.isLocalPlayer) {
				return;
			}

			msg = ChatManager.RemoveInvalidCharacters(msg, true);
			if (string.IsNullOrEmpty(msg))
				return;

			F.InvokeEventExceptionSafe(onChatMessageReceivedOnLocalPlayer, new ChatMessage (msg, sender));

		}

		public	void	SendChatMsgToClient( NetworkConnection conn, string msg, string sender )
		{
			this.TargetChatMsg(conn, msg, sender);
		}

	}

}
