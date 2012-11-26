using System;
using System.Collections.Generic;
using OpenChatbag;
using OpenMetaverse;
using OpenSim.Framework;

namespace OpenChatbag.Test
{
	public class TestChatbag
	{
		static void Main(string[] args)
		{
			//List<Chatbag> doclist;
			//doclist = ConfigParser.Parse("chatbag.xml", "chatbag.xsd");
			//Console.Out.WriteLine("Config parsing successful");
			
			ChatHandler.Instance.RegisterCommand( 
				new ChatHandler.ChatCommand(0, "hello|hi|what's up_world", 
				delegate(string command, OSChatMessage matchingPhrase) { })
			);
			ChatHandler.Instance.RegisterField("curse", "(?:frickin[g]?|fuckin[g]?)");
			ChatHandler.Instance.RegisterCommand( 
				new ChatHandler.ChatCommand(0, "{curse}", 
				delegate(string command, OSChatMessage matchingPhrase) { })
			);
			
			string[] phrases = {"What's up frickin world, what's up?", "Hello fucking world?"};
			foreach( string phrase in phrases ){
				List<ChatHandler.MatchContainer> matches = ChatHandler.Instance.DetectCommand2(phrase);
				foreach( ChatHandler.MatchContainer match in matches ){
					Console.Out.WriteLine(string.Format(
						"Command '{0}' found in '{1}' ({2})", 
						match.Phrase, match.MatchedMessage, String.Join<string>("_", match.MatchedWording) ));
				}
			}
			
			
			Console.Out.WriteLine("Match list complete");
			
			Console.Out.WriteLine("Tests completed successfully.");
			Console.Read();
		}
	}
}

