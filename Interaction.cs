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

		public struct Response{
			int Channel;
			string Text;
			public Response(int channel, string text){
				Channel = channel; Text = text;
			}
		}
		
		public enum ResponseSelectionMode { RandomResponse, NextResponse };
		#endregion

		public ResponseSelectionMode responseMode { get; set; }
		public string Name { get; set; }
		public TriggerList triggerList { get; set; }
		public List<Response> responses { get; protected set; }

		public float MaxRange {
			get {
				float max = 0;
				foreach (ITrigger t in triggerList._list[typeof(ProximityTrigger)]){
					ProximityTrigger trig = t as ProximityTrigger;
					if (trig.Range > max) max = trig.Range;
				}
				return max;
			}
		}

		public Interaction()
		{
			Name = "";
			triggerList = new TriggerList();
			responses = new List<Response>();
			responseMode = ResponseSelectionMode.RandomResponse;
		}
	}
}
