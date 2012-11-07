using System;
using System.Collections.Generic;
using System.Reflection;

using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

using log4net;

namespace OpenChatbag
{
	public delegate bool ValidationDelegate(string s);
	public delegate void ChatHandlerDelegate(List<string> command);

	public class ChatHandler
	{
		public string ChatHandle { get; set; }
		public int ChatChannel { get; set; }
		
		Dictionary<string, ChatHandlerDelegate> commandList;
		Dictionary<string, ValidationDelegate> fieldValidateList;
		
		#region constructors
		
		public ChatHandler()
		{
			commandList = new Dictionary<string, ChatHandlerDelegate>();
			fieldValidateList = new Dictionary<string, ValidationDelegate>();
			ChatHandle = "[Chatbag]";
			ChatChannel = 0;
		}

		#endregion

		#region command registration
		
		public void RegisterCommand(string command, ChatHandlerDelegate handler)
		{
			if (commandList.ContainsKey(command))
				commandList[command] = handler;
			else
				commandList.Add(command, handler);
		}
		public void DeregisterCommand(string command){
			if( commandList.ContainsKey(command) ){
				commandList.Remove(command);
			}
		}
		
		public void RegisterField(string fieldKey, ValidationDelegate validator)
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
		
		#endregion
		
		public void ProcessCommand(string msg)
		{
			List<string> wordList = new List<string>( msg.Split(' '));

			// sanitize input
			for (int i = 0; i < wordList.Count; i++ ){
				wordList[i] = wordList[i].ToLower().Trim(" .,!?".ToCharArray());
			}

			List<string> matches = new List<string>();

			bool commandFound, commandWordsFound, commandWordFound;

			// see if the phrase matches any commands
			commandFound = true;
			foreach ( KeyValuePair<string, ChatHandlerDelegate> kvp in commandList)
			{
				// break command into sequential words, all of which are required in the proper order
				int wordPoint = -1;
				string[] commandWords = kvp.Key.Split('_');
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
						else if( synonym.StartsWith("[") && synonym.EndsWith("]") )
						{
							string keyword = synonym.Trim("[]".ToCharArray());
							for(int i=wordPoint+1; i<wordList.Count; i++)
							{
								// field found
								if( fieldValidateList[keyword](wordList[i]) )
								{
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
					commandFound = true;
					kvp.Value(matches);
					break;
				}

			} // for each command

			if (!commandFound)
			{
				DefaultChatHandler(matches);
			}
		}

		public void DefaultChatHandler(List<string> command)
		{
			SendMessageToAvatar("I don't understand...");
		}


		public void HandleChatInput(object sender, OSChatMessage msg)
		{
			if (msg.Channel != ChatChannel)
				return;

			ProcessCommand(msg.Message);
			
			/*
			// parse command statement
			switch (command.Command)
			{
				case "help":
					if (command.Args.Count == 0)
					{
						SendMessageToAvatar(
							"You can ask me to do any of the following actions:\n" +
							"   help - Get more information on a command\n" +
							"   connect/disconnect - Apply the stored settings\n" +
							"   get/set hostname/ip/domain - Get/set the GIFT domain\n" +
							"   get/set port - Get/set the GIFT port\n" +
							"   get/set uri - Get/set the GIFT uri string"
						);
					}
					else if (command.Args[0] == "domain")
					{
						SendMessageToAvatar(
							"The domain is the web address, whether IP, hostname, or fully-qualified domain name, " +
							"that I will look to for my instructions. I will look to the local machine by default.");
					}
					else if (command.Args[0] == "port")
					{
						SendMessageToAvatar(
							"The port is my access point into the instruction server. It must be a valid port " +
							"number (between 1 and 65535). The default port is 61616.");
					}
					else if (command.Args[0] == "uri")
					{
						SendMessageToAvatar(
							"The URI connection string is a combination of the protocol, IP address, and port number " +
							"needed to connect to my instruction server. They look like 'tcp://127.0.0.1:61616', " +
							"which is the default.");
					}
					else if (command.Args[0] == "connect")
					{
						SendMessageToAvatar(
							"When I am told to connect, I will use the previously specified connection information " +
							"to try to contact my instruction server. You can also tell me to connect directly to " +
							"a particular URI.");
					}
					else if (command.Args[0] == "disconnect")
					{
						SendMessageToAvatar(
							"When I am told to disconnect, I will try to hang up on my instruction server.");
					}
				break;
				
			}*/
		}

		#region outgoing chat functions
		//TODO: Update avatar messaging with scene awareness
		public void SendMessageToAvatar(string message)
		{
			/*foreach (Scene s in GIFTCapsule.Scenes) {
				IWorldComm comm = s.RequestModuleInterface<IWorldComm>();
				ScenePresence p = s.GetScenePresence(Parent.AvatarID);
				if (p != null && comm != null) {
					os_log.InfoFormat("[GIFT]: Broadcasting to scene {0}", s.GetHashCode());
					comm.DeliverMessage(ChatTypeEnum.Region, ChatChannel, ChatHandle, UUID.Zero, message);
					return;
				}
			}
			os_log.Error("[GIFT]: Communication channel or scene presence could not be established!");*/
			SendMessageToWorld(ChatChannel, message);
		}

		public static void SendMessageToWorld(int channel, string message)
		{
			foreach (Scene s in OpenChatbagModule.Scenes) {
				
				IWorldComm comm = s.RequestModuleInterface<IWorldComm>();
				if (comm != null) {
					comm.DeliverMessage(ChatTypeEnum.Region, channel, ChatHandle, UUID.Zero, message);
				}
			}
		}
		#endregion
	}
}
