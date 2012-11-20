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
			
			i.responses = new Response("The OpenChatbag config file has been reloaded",
			                           0, Response.VolumeType.Private);
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
					cmd.Hazardous = true;
					ChatHandler.Instance.RegisterCommand(cmd);
				}
			}
		}

		public override void ProcessChat(string keyphrase, OSChatMessage matchingPhrase)
		{
			if (keyphrase == "reload_config") ReloadConfig();
			
			base.ProcessChat(keyphrase, matchingPhrase);
		}

		public void ReloadConfig()
		{
			ChatHandler.Instance.ClearRegistry();
			PositionTracker.Instance.ClearTrackers();

			ISharedRegionModule mod = (ISharedRegionModule)OpenChatbagModule.Scenes[0].RegionModules["OpenChatbagModule"];
			if (mod != null)
				mod.PostInitialise();
			else
				OpenChatbagModule.os_log.Error("[OpenChatbag]: Failed to access root chatbag module");
		}
	}
}
