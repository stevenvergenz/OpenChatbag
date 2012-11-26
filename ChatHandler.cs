using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Region.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

using log4net;

namespace OpenChatbag
{
	public delegate void ChatHandlerDelegate(string command, OSChatMessage matchingPhrase);

	public class ChatHandler
	{
		private class TupleList : List<Tuple<string,string> >
		{
			public TupleList() : base() { }
			public void Add(string a, string b){
				base.Add( new Tuple<string,string>(a, b) );
			}
		}
		
		List<ChatCommand> commandList;
		Dictionary<string, string> fieldValidateList;

		#region singleton handling
		private static ChatHandler instance;
		private ChatHandler() {
			commandList = new List<ChatCommand>();
			fieldValidateList = new Dictionary<string, string>();
		}
		public static ChatHandler Instance {
			get {
				if (instance == null){
					instance = new ChatHandler();
				}
				return instance;
			}
		}
		#endregion
		
		#region command registration

		public class ChatCommand : IEquatable<ChatCommand>
		{
			public bool Hazardous = false;
			public int Channel;
			public string Phrase;
			public ChatHandlerDelegate Handler;

			public ChatCommand() { }
			public ChatCommand(int channel, string phrase, ChatHandlerDelegate handler)
			{
				Channel = channel;
				Phrase = phrase;
				Handler = handler;
			}

			public bool Equals(ChatCommand other)
			{
				return Channel == other.Channel && Phrase == other.Phrase && Handler == other.Handler;
			}
		}

		public bool RegisterCommand(ChatCommand command){
			if (!commandList.Contains(command))
			{
				commandList.Add(command);
				return true;
			}
			else return false;
		}

		public bool DeregisterCommand(ChatCommand command){
			if (commandList.Contains(command))
			{
				commandList.Remove(command);
				return true;
			}
			else return false;
		}
		
		public void RegisterField(string fieldKey, string validator)
		{
			if (fieldValidateList.ContainsKey(fieldKey))
				fieldValidateList[fieldKey] = validator;
			else
				fieldValidateList.Add(fieldKey, validator);
		}

		public void DeregisterField(string fieldKey){
			if( fieldValidateList.ContainsKey(fieldKey) ){
				fieldValidateList.Remove(fieldKey);
			}
		}

		public void ClearRegistry()
		{
			fieldValidateList.Clear();
			commandList.Clear();
		}
		#endregion
		
		public List<string> DetectCommand(string msg)
		{
			List<string> wordList = new List<string>( msg.Split(' '));
			List<string> matchList = new List<string>();
			
			// sanitize input
			for (int i = 0; i < wordList.Count; i++ ){
				wordList[i] = wordList[i].ToLower().Trim(" .,!?".ToCharArray());
			}

			List<string> matches = new List<string>();

			bool commandWordsFound, commandWordFound;

			// see if the phrase matches any commands
			foreach ( ChatCommand command in commandList)
			{
				// break command into sequential words, all of which are required in the proper order
				int wordPoint = -1;
				string[] commandWords = command.Phrase.Split('_');
				matches.Clear();

				commandWordsFound = true;
				foreach( string commandWord in commandWords)
				{
					// break word into synonym list, only one of which is needed
					string[] synonyms = commandWord.Split('|');

					commandWordFound = false;
					foreach( string synonym in synonyms )
					{
						// match the word as-is
						int index = -1;
						if( (index = wordList.FindIndex(wordPoint+1, synonym.Equals)) > wordPoint )
						{
							// keyword found
							matches.Add(synonym);
							wordPoint = index;
							commandWordFound = true;
							break;
						}
						// try to match a field
						else if( synonym.StartsWith("{") && synonym.EndsWith("}") )
						{
							string keyword = synonym.Trim("{}".ToCharArray());
							for(int i=wordPoint+1; i<wordList.Count; i++)
							{
								Regex re = new Regex(fieldValidateList[keyword]);
								if( re.IsMatch(wordList[i]) )
								{
									// field found
									wordPoint = i;
									matches.Add(wordList[i]);
									commandWordFound = true;
									break; // stop checking sentence words
								}
							}
							if (commandWordFound) break; // stop checking synonyms
						}
						
					} // for each synonym

					// if any word is not found, break
					commandWordsFound &= commandWordFound;
					if (!commandWordsFound) break;

				} // for each command word

				// all words found, call handler
				if ( commandWordsFound ){
					matchList.Add(command.Phrase);
				}

			} // for each command
			return matchList;
		}
		
		public TupleList DetectCommand2(string msg)
		{
			TupleList matches = new TupleList();
			
			foreach( string command in commandList )
			{
				// convert command syntax to regex
				// 'a|b_c' goes to '(a|b).*(c)'
				string cmdRE = command;
				cmdRE = "(?i:"+cmdRE+")";
				cmdRE = cmdRE.Replace("_", ").*(");
				
				// replace field identifiers with appropriate regex
				Regex fieldPattern = new Regex(@"\{([A-Za-z][A-Za-z0-9]*)\}");
				foreach( Match m in fieldPattern.Matches(cmdRE))
				{
					if( !m.Success ) continue;
					
					string key = m.Captures[1].Value;
					cmd = cmdRE.Replace(
						String.Format("{{{0}}}", key),
						String.Format("(?:{0})", fieldValidateList[key]));
				}
				
				// compile newfound regular expression
				Console.Out.WriteLine("Command regex: "+cmdRE);
				Regex commandMatcher = new Regex(cmdRE);
				
				// if match
				Match match = commandMatcher.Match(msg);
				if( match.Success ){
					// build new string out of matching words
					
					// add to return list
				}
			}
			
			return matches;
		}
		
		public void HandleChatInput(object sender, OSChatMessage msg)
		{
			if (msg.Type == ChatTypeEnum.StartTyping || msg.Type == ChatTypeEnum.StopTyping) return;

			List<string> matchList = DetectCommand(msg.Message);

			ChatHandlerDelegate hazardHandler = null;
			foreach( string cmdstring in matchList ){
				foreach (ChatCommand cmd in commandList){
					if (cmd.Channel == msg.Channel && cmd.Phrase == cmdstring){
						if (!cmd.Hazardous){
							cmd.Handler(cmdstring, msg);
						}
						else {
							hazardHandler = cmd.Handler;
							break;
						}
					}
				}

				if (hazardHandler != null)
					hazardHandler(cmdstring, msg);
			}

		}

		#region outgoing chat functions
		public static void DelayDeliverPrivateMessage(UUID avatar, string senderName, string message, int delay){
			WaitCallback callback = delegate(object state) {
				Thread.Sleep(delay);
				DeliverPrivateMessage(avatar, senderName, message);
			};
			ThreadPool.QueueUserWorkItem(callback);
		}
		
		public static void DeliverPrivateMessage(UUID avatar, string senderName, string message)
		{
			Scene scene = null;
			IClientAPI client = null;
			foreach (Scene s in OpenChatbagModule.Scenes)
			{
				if (s.TryGetClient(avatar, out client)){
					scene = s;
					break;
				}
			}
			if (scene == null)
			{
				OpenChatbagModule.os_log.ErrorFormat("[Chatbag]: Could not find user {0}", avatar.ToString());
				return;
			}

			scene.SimChatToAgent(avatar, Utils.StringToBytes(message), Vector3.Zero, senderName, UUID.Zero, false);
			OpenChatbagModule.os_log.Debug("[Chatbag]: Message delivered to " + client.Name);
		}
		
		public static void DelayDeliverPrimMessage(UUID prim, string senderName, int channel, Response.VolumeType volume, string message, int delay){
			WaitCallback callback = delegate(object state) {
				Thread.Sleep(delay);
				DeliverPrimMessage(prim, senderName, channel, volume, message);
			};
			ThreadPool.QueueUserWorkItem(callback);
		}
		
		public static void DeliverPrimMessage(UUID prim, string senderName, int channel, Response.VolumeType volume, string message)
		{
			SceneObjectPart part = null;
			foreach (Scene s in OpenChatbagModule.Scenes){
				part = s.GetSceneObjectPart(prim);
				if (part != null) break;
			}
			if (part == null){
				OpenChatbagModule.os_log.ErrorFormat("[Chatbag]: Could not deliver to nonexistent prim {0}", prim.ToString());
				return;
			}

			ChatTypeEnum type = ChatTypeEnum.Say;
			if (volume == Response.VolumeType.Whisper) type = ChatTypeEnum.Whisper;
			else if (volume == Response.VolumeType.Say) type = ChatTypeEnum.Say;
			else if (volume == Response.VolumeType.Shout) type = ChatTypeEnum.Shout;

			part.ParentGroup.Scene.SimChat(Utils.StringToBytes(message), type, channel, 
				part.AbsolutePosition, senderName, prim, false);
			OpenChatbagModule.os_log.Debug("[Chatbag]: Message delivered to " + part.Name);
		}
		
		public static void DelayDeliverRegionMessage(UUID regionId, string senderName, int channel, string message, int delay){
			WaitCallback callback = delegate(object state) {
				Thread.Sleep(delay);
				DeliverRegionMessage(regionId, senderName, channel, message);
			};
			ThreadPool.QueueUserWorkItem(callback);
		}

		public static void DeliverRegionMessage(UUID regionId, string senderName, int channel, string message)
		{
			foreach (Scene s in OpenChatbagModule.Scenes)
			{
				
				if (s.RegionInfo.RegionID == regionId)
				{
					s.SimChat(Utils.StringToBytes(message), ChatTypeEnum.Region, channel,
						new Vector3(0, 0, 0), senderName, UUID.Zero, false);
					OpenChatbagModule.os_log.Debug("[Chatbag]: Message delivered to " + s.Name);
					return;
				}
			}
			OpenChatbagModule.os_log.Debug("[Chatbag]: Messaged failed to deliver to region " + regionId.ToString());
		}
		
		public static void DelayDeliverWorldMessage(string senderName, int channel, string message, int delay){
			WaitCallback callback = delegate(object state) {
				Thread.Sleep(delay);
				DeliverWorldMessage(senderName, channel, message);
			};
			ThreadPool.QueueUserWorkItem(callback);
		}

		public static void DeliverWorldMessage(string senderName, int channel, string message)
		{
			//foreach (Scene s in OpenChatbagModule.Scenes) {
				OpenChatbagModule.Scenes[0].SimChat(
					Utils.StringToBytes(message), ChatTypeEnum.Broadcast, channel, 
					new Vector3(0, 0, 0), senderName, UUID.Zero, false);
				OpenChatbagModule.os_log.Debug("[Chatbag]: Message delivered to all regions");
			//}
		}
		#endregion
	}
}
