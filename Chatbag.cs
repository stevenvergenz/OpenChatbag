using System;
using System.Collections.Generic;

using OpenMetaverse;
using OpenSim.Region.Framework.Scenes;


namespace OpenChatbag
{
	public abstract class Chatbag
	{
		public string Name { get; set; }
		public List<Interaction> InteractionList { get; protected set; }

		public Chatbag(string name)
		{
			Name = name;
			InteractionList = new List<Interaction>();
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

