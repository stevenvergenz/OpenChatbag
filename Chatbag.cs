using System;
using OpenMetaverse;
using OpenSim.Region.Framework.Scenes;

namespace OpenChatbag
{
	public class Chatbag
	{
		public UUID Target { get; protected set; }
		public PositionTracker movementTracker { get; protected set; }
		public ChatHandler Handler { get; protected set; }
		
		public Chatbag (UUID target = UUID.Zero)
		{
			Handler = new ChatHandler();
			Target = target;
			if( target != UUID.Zero ){
				movementTracker = PositionTracker.addTracker(target);
			}
		}
		
		public void CheckProximityTriggers( ScenePresence presence )
		{
			
		}
	}
}

