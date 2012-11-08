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

		// abstract methods
		public abstract void Register(List<Scene> scenes);

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

		public void AfterInteractionsSet()
		{
			float max = 0;
			foreach (Interaction i in InteractionList)
			{
				if (i.MaxRange > max) max = i.MaxRange;
			}
			physicalState.NearbyRadii.Add(max);
		}

		public void ProcessChat(object sender, OSChatMessage msg)
		{

		}

		public void ProcessRangeChange(PositionState state, float range)
		{

		}
		#endregion
	}

	public class GlobalChatbag : Chatbag
	{
		public GlobalChatbag(string name)
			: base(name)
		{

		}

		public override void Register(List<Scene> scenes)
		{
			
		}
	}
}

