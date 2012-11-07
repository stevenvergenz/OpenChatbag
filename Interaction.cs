using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenChatbag
{
	public class Interaction
	{
		public string Name { get; set; }
		public List<Trigger> Triggers { get; set; }
		public List<string> Responses { get; set; }

		public Interaction()
		{
			Name = "";
			Triggers = new List<Trigger>();
			Responses = new List<string>();
		}
	}
}
