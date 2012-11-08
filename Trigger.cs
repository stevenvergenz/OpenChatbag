using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenChatbag
{
	public interface ITrigger { }

	public class ChatTrigger : ITrigger
	{
		public int Channel { get; protected set; }
		public string Phrase { get; protected set; }

		public ChatTrigger(string phrase, int channel = 0)
		{
			Phrase = phrase;
			Channel = channel;
		}
	}

	public class ProximityTrigger : ITrigger
	{
		public float Range { get; protected set; }

		public ProximityTrigger(float range)
		{
			Range = range;
		}
	}
}
