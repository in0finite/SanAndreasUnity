using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SanAndreasUnity.Net;
using UGameCore.Utilities;

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
		public int maxChatMessageLength = 50;

		public	static	event System.Action<ChatMessage>	onChatMessage = delegate {};

		List<ChatPreprocessor> m_chatPreprocessors = new List<ChatPreprocessor>();

		static StringBuilder _stringBuilderForMessageProcessing = new StringBuilder();


		void Awake ()
		{
			singleton = this;

			onChatMessage += LogChatMessage;
		}

		void OnSceneChanged( SanAndreasUnity.Behaviours.SceneChangedMessage info ) {

			if (NetStatus.IsServer) {
				SendChatMessageToAllPlayersAsServer ("Map changed to " + info.s2.name + ".");
			}

		}

		void LogChatMessage(ChatMessage chatMessage)
		{
			string senderText = string.IsNullOrEmpty(chatMessage.sender) ? "" : "<color=blue>" + chatMessage.sender + "</color> : ";
			Debug.Log(senderText + chatMessage.msg);
		}

		public void SubmitMessageFromPlayer(Player player, string msg)
		{
			NetStatus.ThrowIfNotOnServer();

			if (null == player)
				throw new System.ArgumentNullException(nameof(player));

			msg = ChatManager.ProcessIncomingChatMessage(msg);
			if (string.IsNullOrEmpty(msg))
				return;

			if (!FilterWithPreprocessors(player, ref msg))
				return;

			SendChatMessageToAllPlayersAsServer(msg, player.PlayerName);
		}

		internal void OnChatMessageReceivedOnLocalPlayer(ChatMessage chatMsg)
		{
			F.InvokeEventExceptionSafe(onChatMessage, chatMsg);
		}

		private bool FilterWithPreprocessors(Player player, ref string chatMessageToFilter)
		{
			string finalMsg = chatMessageToFilter;

			foreach (var chatPreprocessor in m_chatPreprocessors)
			{
				ChatPreprocessorResult result = null;
				F.RunExceptionSafe(() => result = chatPreprocessor.processCallback(player, finalMsg));

				if (null == result || result.shouldBeDiscarded || null == result.finalChatMessage)
					return false;

				finalMsg = result.finalChatMessage;
			}

			chatMessageToFilter = finalMsg;
			return true;
		}

		public static string ProcessIncomingChatMessage(string chatMessage)
		{
			if (chatMessage == null)
				return string.Empty;

			var sb = _stringBuilderForMessageProcessing;
			sb.Clear();

            sb.Append(chatMessage, 0, Mathf.Min(singleton.maxChatMessageLength, chatMessage.Length));
			
            // remove tags
            sb.Replace("<", "< "); // the easiest way
			
			sb.Replace('\r', ' ');
			sb.Replace('\n', ' ');
			sb.Replace('\t', ' ');

			return sb.ToString().Trim();
		}

		public	static	void	SendChatMessageToAllPlayersAsServer( string msg )
		{
			NetStatus.ThrowIfNotOnServer();

			SendChatMessageToAllPlayersAsServer (msg, singleton.serverChatNick);

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

		public	static	void	SendChatMessageToAllPlayersAsServer( string msg, string sender ) {

			NetStatus.ThrowIfNotOnServer();

			foreach (var player in Player.AllPlayersCopy) {
				SendChatMessageToPlayerAsServer ( player, msg, sender );
			}

			if (!NetStatus.IsHost ()) {
				// running as dedicated server
				// we should invoke the event here, because there is no local player to receive the chat message
				F.InvokeEventExceptionSafe(onChatMessage, new ChatMessage(msg, sender));
			}

		}

		public	static	void	SendChatMessageToPlayerAsServer( Player player, string msg, bool useServerNick ) {

			NetStatus.ThrowIfNotOnServer();

			SendChatMessageToPlayerAsServer (player, msg, useServerNick ? singleton.serverChatNick : "");
		}

		/// <summary>
		/// Submit chat message. If there is a local player, message will be sent from that player to the server.
		/// Otherwise, if server is active, message will be sent from server to all players.
		/// </summary>
		public static void SubmitChatMessage(string msg)
        {
			if (Player.Local != null)
				SendChatMessageToAllPlayersAsLocalPlayer(msg);
			else if (NetStatus.IsServer)
				SendChatMessageToAllPlayersAsServer(msg);
        }

		private	static	void	SendChatMessageToPlayerAsServer( Player player, string msg, string sender ) {

			NetStatus.ThrowIfNotOnServer();

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
