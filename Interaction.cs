using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenChatbag
{
	public class Interaction
	{
		#region helper classes
		public class TriggerList
		{
			public Dictionary<Type, List<ITrigger>> _list;

			public TriggerList(){
				_list = new Dictionary<Type, List<ITrigger>>();
			}

			public List<ITrigger> GetTriggers(Type t)
			{
				if (_list.ContainsKey(t)) return _list[t];
				else return new List<ITrigger>();
			}

			public void addTrigger(ITrigger trigger)
			{
				Type t = trigger.GetType();

				if (!_list.ContainsKey(t))
					_list.Add(t, new List<ITrigger>());
				
				if( !_list[t].Contains(trigger) )
					_list[t].Add(trigger);
			}
			public bool removeTrigger(ITrigger trigger)
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

			public static TriggerList operator +(TriggerList list, ITrigger trigger){
				list.addTrigger(trigger);
				return list;
			}
			public static TriggerList operator -(TriggerList list, ITrigger trigger){
				list.removeTrigger(trigger);
				return list;
			}
		}

		
		#endregion

		public string Name { get; set; }
		public TriggerList triggerList { get; set; }
		public List<Response> responses { get; protected set; }
		public ResponseSelectionMode responseMode { get; set; }
		private int responseCounter;

		public Interaction(string name)
		{
			Name = name;
			triggerList = new TriggerList();
			responses = new List<Response>();
			responseMode = ResponseSelectionMode.RandomResponse;
			responseCounter = 0;
		}
	}
}
