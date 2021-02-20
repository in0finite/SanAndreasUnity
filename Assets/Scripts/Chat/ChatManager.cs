using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Net;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Chat
{


	public class ChatMessage
	{
		public ChatMessage (string msg, string sender)
		{
			this.msg = msg;
			this.sender = sender;
		}
		
		public string msg = "";
		public string sender = "";
	}

	public class ChatPreprocessorResult
	{
		public bool shouldBeDiscarded;
		public string finalChatMessage;
	}

	public class ChatPreprocessor
	{
		public System.Func<Player, string, ChatPreprocessorResult> processCallback;
	}


	public class ChatManager : MonoBehaviour
	{
		
		public	static	ChatManager singleton { get ; private set ; }
		public	string	serverChatNick = "<color=green>Server</color>";
		public	static	event System.Action<ChatMessage>	onChatMessage = delegate {};

		List<ChatPreprocessor> m_chatPreprocessors = new List<ChatPreprocessor>();


		void Awake ()
		{
			
			singleton = this;

			onChatMessage += (ChatMessage chatMsg) => Debug.Log ("<color=blue>" + chatMsg.sender + "</color> : " + chatMsg.msg);
			
			ChatSync.onChatMessageReceivedOnServer += OnChatMessageReceivedOnServer;
			ChatSync.onChatMessageReceivedOnLocalPlayer += (ChatMessage chatMsg) => onChatMessage (chatMsg);

		}

		void OnSceneChanged( SanAndreasUnity.Behaviours.SceneChangedMessage info ) {

			if (NetStatus.IsServer) {
				SendChatMessageToAllPlayersAsServer ("Map changed to " + info.s2.name + ".");
			}

		}


		private void OnChatMessageReceivedOnServer(Player player, string msg)
		{
			foreach (var chatPreprocessor in m_chatPreprocessors)
			{
				ChatPreprocessorResult result = null;
				F.RunExceptionSafe(() => result = chatPreprocessor.processCallback(player, msg));

				if (null == result || result.shouldBeDiscarded || null == result.finalChatMessage)
					return;

				msg = result.finalChatMessage;
			}

			SendChatMessageToAllPlayers(msg, "player " + player.netId);
		}

		public	static	void	SendChatMessageToAllPlayersAsServer( string msg ) {

			if (NetStatus.IsServerStarted) {
				SendChatMessageToAllPlayers (msg, singleton.serverChatNick);
			}

		}

		public	static	void	SendChatMessageToAllPlayersAsLocalPlayer( string msg ) {

			if (null == Player.Local) {
				return;
			}

			var chatSync = Player.Local.GetComponent<ChatSync> ();
			if (chatSync != null) {
				chatSync.SendChatMsgToServer (msg);
			}

		}

		/// <summary> Use only on server. </summary>
		public	static	void	SendChatMessageToAllPlayers( string msg, string sender ) {

			if (!NetStatus.IsServerStarted)
				return;

			msg = msg.Trim();

			foreach (var player in Player.AllPlayers) {
				SendChatMessageToPlayer ( player, msg, sender );
			}

			if (!NetStatus.IsHost ()) {
				// running as dedicated server
				// we should invoke the event here, because there is no local player to receive the chat message
				onChatMessage( new ChatMessage(msg, sender) );
			}

		}

		/// <summary> Use only on server. </summary>
		public	static	void	SendChatMessageToPlayer( Player player, string msg ) {

			if (!NetStatus.IsServerStarted)
				return;

			SendChatMessageToPlayer (player, msg, singleton.serverChatNick);

		}

		private	static	void	SendChatMessageToPlayer( Player player, string msg, string sender ) {

			if (!NetStatus.IsServerStarted)
				return;

			var chatSync = player.GetComponent<ChatSync> ();
			if (chatSync != null) {
				chatSync.SendChatMsgToClient (player.connectionToClient, msg, sender);
			}

		}

		public void RegisterChatPreprocessor(ChatPreprocessor chatPreprocessor)
		{
			m_chatPreprocessors.Add(chatPreprocessor);
		}

	}

}
