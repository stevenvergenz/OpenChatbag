using System;
using System.Collections.Generic;

using OpenMetaverse;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Framework;


namespace OpenChatbag
{
	public abstract class Chatbag
	{
		public string Name { get; set; }
		public List<Interaction> InteractionList { get; protected set; }
		public PositionState physicalState;

		#region implementation
		public Chatbag(string name)
		{
			Name = name;
			InteractionList = new List<Interaction>();
		}
		public Chatbag(string name, UUID target) : this(name)
		{
			physicalState = PositionTracker.Instance.addTracker(target);
			physicalState.OnRangeChange += ProcessRangeChange;	
		}

		public virtual void AfterInteractionsSet()
		{
			foreach (Interaction i in InteractionList)
			{
				foreach (ChatTrigger trig in i.triggerList.GetTriggers(typeof(ChatTrigger)))
				{
					ChatHandler.Instance.RegisterCommand(
						new ChatHandler.ChatCommand(
							trig.Channel, trig.Phrase, ProcessChat
							));
				}
			}
		}

		public virtual void ProcessChat(string keyphrase, OSChatMessage matchingPhrase) { }

		public virtual void ProcessRangeChange(PositionState state, float range) { }

		#endregion
	}


	public class GlobalChatbag : Chatbag
	{
		public GlobalChatbag(string name)
			: base(name)
		{

		}

		public override void ProcessRangeChange(PositionState state, float range)
		{
			OpenChatbagModule.os_log.Debug("[Chatbag]: Checking range change");
			foreach (Interaction i in InteractionList)
			{
				if (i.triggerList.GetTriggers(typeof(ProximityTrigger)).Count != 0)
				{
					OpenChatbagModule.os_log.DebugFormat("[Chatbag]: Triggering interaction {0}", i.Name);
					Interaction.Response response = i.GetResponse();
					ChatHandler.DeliverWorldMessage(Name, response.Channel, response.Text);
				}
			}
		}

		public override void ProcessChat(string keyphrase, OSChatMessage matchingPhrase)
		{
			foreach (Interaction i in InteractionList)
			{
				foreach (ChatTrigger trig in i.triggerList.GetTriggers(typeof(ChatTrigger)))
				{
					if (trig.Phrase == keyphrase)
					{
						Interaction.Response r = i.GetResponse();
						ChatHandler.DeliverWorldMessage(Name, r.Channel, r.Text);
						break;
					}
				}
			}
		}
	}


	public class RegionChatbag : Chatbag
	{
		public RegionChatbag(string name, UUID uuid)
			: base(name, uuid)
		{

		}

		public override void ProcessRangeChange(PositionState state, float range)
		{
			foreach (Interaction i in InteractionList)
			{
				if (i.triggerList.GetTriggers(typeof(ProximityTrigger)).Count != 0)
				{
					Interaction.Response response = i.GetResponse();
					ChatHandler.DeliverRegionMessage(state.Target, Name, response.Channel, response.Text);
				}
			}
		}

		public override void ProcessChat(string keyphrase, OSChatMessage matchingPhrase)
		{
			foreach (Interaction i in InteractionList)
			{
				foreach (ChatTrigger trig in i.triggerList.GetTriggers(typeof(ChatTrigger)))
				{
					if (matchingPhrase.Scene.RegionInfo.RegionID == physicalState.Target && trig.Phrase == keyphrase)
					{
						Interaction.Response r = i.GetResponse();
						ChatHandler.DeliverRegionMessage(physicalState.Target, Name, r.Channel, r.Text);
						break;
					}
				}
			}
		}
	}

	public class PrimChatbag : Chatbag
	{
		public PrimChatbag(string name, UUID uuid)
			: base(name, uuid)
		{

		}

		public override void AfterInteractionsSet()
		{
			base.AfterInteractionsSet();

			foreach (Interaction i in InteractionList)
			{
				foreach (ProximityTrigger trig in i.triggerList.GetTriggers(typeof(ProximityTrigger)))
				{
					if (!physicalState.NearbyRadii.Contains(trig.Range))
					{
						physicalState.NearbyRadii.Add(trig.Range);
					}
				}
			}
		}

		public override void ProcessRangeChange(PositionState state, float range)
		{
			foreach (Interaction i in InteractionList)
			{
				foreach( ProximityTrigger trigger in i.triggerList.GetTriggers(typeof(ProximityTrigger)))
				{
					if (range == trigger.Range)
					{
						Interaction.Response response = i.GetResponse();
						ChatHandler.DeliverPrimMessage(state.Target, Name, response.Channel, response.Volume, response.Text);
					}
				}
			}
		}

		public override void ProcessChat(string keyphrase, OSChatMessage matchingPhrase)
		{
			float range = Vector3.Distance(
				PositionTracker.ToGlobalCoordinates(
					matchingPhrase.Scene.RegionInfo, 
					matchingPhrase.Sender.SceneAgent.AbsolutePosition),
				physicalState.Position);

			bool IsInRange =
				(matchingPhrase.Type == ChatTypeEnum.Whisper && range <= OpenChatbagModule.WhisperDistance) ||
				(matchingPhrase.Type == ChatTypeEnum.Say && range <= OpenChatbagModule.SayDistance) ||
				(matchingPhrase.Type == ChatTypeEnum.Shout && range <= OpenChatbagModule.ShoutDistance);

			foreach (Interaction i in InteractionList)
			{
				foreach (ChatTrigger trig in i.triggerList.GetTriggers(typeof(ChatTrigger)))
				{
					if ( IsInRange && trig.Phrase == keyphrase)
					{
						Interaction.Response r = i.GetResponse();
						ChatHandler.DeliverPrimMessage(physicalState.Target, Name, r.Channel, r.Volume, r.Text);
						break;
					}
				}
			}
		}
	}
}

