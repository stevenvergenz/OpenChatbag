using System;
using System.Collections.Generic;

using OpenMetaverse;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Framework;


namespace OpenChatbag
{
	public abstract class Chatbag
	{
		private static readonly int DEFAULT_CHAT_DELAY = 500;
		
		public string Name { get; set; }
		public List<Interaction> InteractionList { get; protected set; }
		public PositionState tracker;

		#region implementation
		public Chatbag(string name)
		{
			Name = name;
			InteractionList = new List<Interaction>();
		}
		public Chatbag(string name, UUID target) : this(name)
		{
			tracker = PositionTracker.Instance.addTracker(target);
			tracker.OnRangeChange += ProcessRangeChange;	
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

		public virtual void ProcessChat(string keyphrase, OSChatMessage matchingPhrase) 
		{
			foreach (Interaction i in InteractionList)
			{
				foreach (ChatTrigger trig in i.triggerList.GetTriggers(typeof(ChatTrigger)))
				{
					if (trig.Phrase == keyphrase && FinalChatCheck(keyphrase, matchingPhrase))
					{
						List<Response> message = i.responses.GetResponse();
						foreach( Response r in message ){
							switch(r.Volume){
							case Response.VolumeType.Global:
								ChatHandler.DelayDeliverWorldMessage(Name, r.Channel, r.Text, DEFAULT_CHAT_DELAY);
								break;
								
							case Response.VolumeType.Region:
								ChatHandler.DelayDeliverRegionMessage(
									matchingPhrase.Scene.RegionInfo.RegionID, Name, r.Channel, r.Text, DEFAULT_CHAT_DELAY);
								break;
								
							case Response.VolumeType.Shout:
							case Response.VolumeType.Say:
							case Response.VolumeType.Whisper:
								ChatHandler.DelayDeliverPrimMessage(
									tracker.Target, Name, r.Channel, r.Volume, r.Text, DEFAULT_CHAT_DELAY);
								break;
								
							case Response.VolumeType.Private:
								ChatHandler.DelayDeliverPrivateMessage(
									matchingPhrase.SenderUUID, Name, r.Text, DEFAULT_CHAT_DELAY);
								break;
							}
						}
						break;
					}
				}
			}
		}

		public virtual void ProcessRangeChange(PositionState state, ScenePresence client, float range){}
		
		public virtual bool FinalChatCheck(string keyphrase, OSChatMessage match) { return true; }

		#endregion
	}


	public class GlobalChatbag : Chatbag
	{
		public GlobalChatbag(string name)
			: base(name)
		{

		}

		public override bool FinalChatCheck(string keyphrase, OSChatMessage matchingPhrase)
		{
			return true;
		}
	}


	public class RegionChatbag : Chatbag
	{
		public RegionChatbag(string name, UUID uuid)
			: base(name, uuid)
		{

		}

		public override bool FinalChatCheck(string keyphrase, OSChatMessage matchingPhrase)
		{
			return matchingPhrase.Scene.RegionInfo.RegionID == tracker.Target;
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
					if (!tracker.NearbyRadii.Contains(trig.Range))
					{
						tracker.NearbyRadii.Add(trig.Range);
					}
				}
			}
		}

		public override bool FinalChatCheck(string keyphrase, OSChatMessage matchingPhrase)
		{
			float range = Vector3.Distance(
				PositionTracker.ToGlobalCoordinates(
					matchingPhrase.Scene.RegionInfo, 
					matchingPhrase.Sender.SceneAgent.AbsolutePosition),
				tracker.Position);

			return
				(matchingPhrase.Type == ChatTypeEnum.Whisper && range <= OpenChatbagModule.WhisperDistance) ||
				(matchingPhrase.Type == ChatTypeEnum.Say && range <= OpenChatbagModule.SayDistance) ||
				(matchingPhrase.Type == ChatTypeEnum.Shout && range <= OpenChatbagModule.ShoutDistance);

		}
		
		public override void ProcessRangeChange(PositionState state, ScenePresence client, float range)
		{
			foreach( Interaction i in InteractionList)
			{
				foreach( ProximityTrigger trig in i.triggerList.GetTriggers(typeof(ProximityTrigger)))
				{
					if( trig.Range == range ){
						List<Response> message = i.responses.GetResponse();
						foreach( Response r in message ){
							switch(r.Volume){
							case Response.VolumeType.Global:
								ChatHandler.DeliverWorldMessage(Name, r.Channel, r.Text);
								break;
								
							case Response.VolumeType.Region:
								ChatHandler.DeliverRegionMessage(client.Scene.RegionInfo.RegionID, 
								                                 Name, r.Channel, r.Text);
								break;
								
							case Response.VolumeType.Shout:
							case Response.VolumeType.Say:
							case Response.VolumeType.Whisper:
								ChatHandler.DeliverPrimMessage(tracker.Target, Name, 
								                               r.Channel, r.Volume, r.Text);
								break;
								
							case Response.VolumeType.Private:
								ChatHandler.DeliverPrivateMessage(client.UUID, Name, r.Text);
								break;
							}
						}
						break;
					}
				}
			}
		}
	}
}

