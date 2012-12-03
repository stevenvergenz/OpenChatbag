using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

namespace OpenChatbag
{
	class ConsoleChatbag : GlobalChatbag
	{
		public ConsoleChatbag(int channel) : base("Console")
		{
			Interaction i;

			// add "reload console" command
			i = new Interaction("Reload Config");
			i.triggerList.addTrigger(new ChatTrigger("reload_config", channel));
			/*i.responses = new Response("The OpenChatbag config file has been reloaded",
			                           0, Response.VolumeType.Private);*/
			InteractionList.Add(i);
		}

		public override void AfterInteractionsSet()
		{
			foreach (Interaction i in InteractionList)
			{
				foreach (ChatTrigger trig in i.triggerList.GetTriggers(typeof(ChatTrigger)))
				{
					ChatHandler.ChatCommand cmd = new ChatHandler.ChatCommand(
						trig.Channel, trig.Phrase, ProcessChat);
					ChatHandler.Instance.RegisterCommand(cmd);
				}
			}
		}

		public override void ProcessChat(ChatHandler.MatchContainer match)
		{
			if (match.Command.Phrase == "reload_config") ReloadConfig();
			
			base.ProcessChat(match);
		}

		public void ReloadConfig()
		{
			ChatHandler.Instance.ClearRegistry();
			PositionTracker.Instance.ClearTrackers();

			try{
				OpenChatbagModule.LoadChatbagConfig();
				InteractionList[0].responses = new Response("The OpenChatbag config file has been reloaded",
					0, Response.VolumeType.Private);
			}
			catch( Exception e ){
				List<Response> responses = new List<Response>();
				responses.Add( new Response("There was a problem loading the config file", 0, Response.VolumeType.Private, 100) );
				responses.Add( new Response(e.Message, 0, Response.VolumeType.Private, 150) );
				InteractionList[0].responses = new ResponseList(responses, ResponseList.ResponseSelectionMode.All);
			}
		}
	}
}
