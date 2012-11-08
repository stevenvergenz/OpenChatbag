using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenChatbag
{
	public class Interaction
	{
		public class TriggerList
		{
			public Dictionary<Type, List<Trigger>> _list;

			public TriggerList(){
				_list = new Dictionary<Type, List<Trigger>>();
			}
			public void addTrigger(Trigger trigger)
			{
				Type t = trigger.GetType();

				if (!_list.ContainsKey(t))
					_list.Add(t, new List<Trigger>());
				
				if( !_list[t].Contains(trigger) )
					_list[t].Add(trigger);
			}
			public bool removeTrigger(Trigger trigger)
			{
				Type t = trigger.GetType();

				if (_list.ContainsKey(t) && _list[t].Contains(trigger))
				{
					_list[t].Remove(trigger);

					if (_list[t].Count == 0)
						_list.Remove(t);

					return true;
				}
				else return false;

			}

			public static TriggerList operator +(TriggerList list, Trigger trigger){
				list.addTrigger(trigger);
				return list;
			}
			public static TriggerList operator -(TriggerList list, Trigger trigger){
				list.removeTrigger(trigger);
				return list;
			}
		}

		public string Name { get; set; }
		public TriggerList triggerList { get; protected set; }
		public List<string> Responses { get; protected set; }

		public Interaction()
		{
			Name = "";
			triggerList = new TriggerList();
			Responses = new List<string>();
		}
	}
}
