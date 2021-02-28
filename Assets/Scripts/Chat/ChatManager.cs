using System.Collections.Generic;
using System.Text;
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
		public int maxChatMessageLength = 50;

		public	static	event System.Action<ChatMessage>	onChatMessage = delegate {};

		List<ChatPreprocessor> m_chatPreprocessors = new List<ChatPreprocessor>();

		static StringBuilder _stringBuilderForMessageProcessing = new StringBuilder();


		void Awake ()
		{
			
			singleton = this;

			onChatMessage += (ChatMessage chatMsg) => Debug.Log ("<color=blue>" + chatMsg.sender + "</color> : " + chatMsg.msg);
			
			ChatSync.onChatMessageReceivedOnServer += OnChatMessageReceivedOnServer;
			ChatSync.onChatMessageReceivedOnLocalPlayer += (ChatMessage chatMsg) => F.InvokeEventExceptionSafe(onChatMessage, chatMsg);

		}

		void OnSceneChanged( SanAndreasUnity.Behaviours.SceneChangedMessage info ) {

			if (NetStatus.IsServer) {
				SendChatMessageToAllPlayersAsServer ("Map changed to " + info.s2.name + ".");
			}

		}


		private void OnChatMessageReceivedOnServer(Player player, string msg)
		{
			if (!FilterWithPreprocessors(player, ref msg))
				return;

			SendChatMessageToAllPlayers(msg, "player " + player.netId);
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

		public static string ProcessChatMessage(string chatMessage, bool allowTags)
		{
			if (chatMessage == null)
				return string.Empty;

			if (chatMessage.Length > 2000)
				return string.Empty;

			var sb = _stringBuilderForMessageProcessing;
			sb.Clear();
			sb.Append(allowTags ? chatMessage : (chatMessage.Length > singleton.maxChatMessageLength ? chatMessage.Substring(0, singleton.maxChatMessageLength) : chatMessage));

			// Remove tags.
			if (!allowTags)
			{
				sb.Replace('<', ' '); // the only easy way :D
				sb.Replace('>', ' ');
				//	msg = msg.Replace ("<color", "color");
				//	msg = msg.Replace ("<size", "size");
				//	msg = msg.Replace ("<b>", "");
				//	msg = msg.Replace ("<i>", "");
				//	msg = msg.Replace (">", "\\>");
			}

			sb.Replace('\r', ' ');
			sb.Replace('\n', ' ');
			sb.Replace('\t', ' ');

			return sb.ToString().Trim();
		}

		public	static	void	SendChatMessageToAllPlayersAsServer( string msg )
		{
			NetStatus.ThrowIfNotOnServer();

			SendChatMessageToAllPlayers (msg, singleton.serverChatNick);

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

			NetStatus.ThrowIfNotOnServer();

			msg = ChatManager.ProcessChatMessage(msg, true);
			if (string.IsNullOrEmpty(msg))
				return;

			foreach (var player in Player.AllPlayers) {
				SendChatMessageToPlayer ( player, msg, sender );
			}

			if (!NetStatus.IsHost ()) {
				// running as dedicated server
				// we should invoke the event here, because there is no local player to receive the chat message
				F.InvokeEventExceptionSafe(onChatMessage, new ChatMessage(msg, sender));
			}

		}

		/// <summary> Use only on server. </summary>
		public	static	void	SendChatMessageToPlayer( Player player, string msg ) {

			NetStatus.ThrowIfNotOnServer();

			msg = ChatManager.ProcessChatMessage(msg, true);
			if (string.IsNullOrEmpty(msg))
				return;

			SendChatMessageToPlayer (player, msg, singleton.serverChatNick);

		}

		private	static	void	SendChatMessageToPlayer( Player player, string msg, string sender ) {

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
