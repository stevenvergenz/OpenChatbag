using System;
using OpenMetaverse;
using OpenSim.Region.Framework.Scenes;

namespace OpenChatbag
{
	public class Chatbag
	{
		public UUID Target { get; protected set; }
		public PositionState targetState { get; protected set; }
		public ChatHandler Handler { get; protected set; }
		
		public Chatbag()
		{
			Handler = new ChatHandler();
			Target = UUID.Zero;
		}
		
		public Chatbag(UUID target, PositionTracker tracker)
		{
			Handler = new ChatHandler();
			Target = target;
			if( target != UUID.Zero ){
				targetState = tracker.addTracker(target);
			}
		}
		
		public void CheckProximityTriggers( ScenePresence presence )
		{
			
		}
	}
}

