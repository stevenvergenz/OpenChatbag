using System;
using System.Collections.Generic;

using OpenMetaverse;
using OpenSim.Region.Framework.Scenes;


namespace OpenChatbag
{
	public abstract class Chatbag
	{
		public string Name { get; set; }
		public List<Trigger> TriggerList { get; protected set; }

		public Chatbag(string name)
		{
			Name = name;
			TriggerList = new List<Trigger>();
		}
	}

	public class GlobalChatbag : Chatbag
	{
		public GlobalChatbag(string name)
			: base(name)
		{

		}
	}
}

