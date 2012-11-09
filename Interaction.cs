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

		public enum VolumeType { Global, Region, Shout, Say, Whisper }
		public struct Response{
			public int Channel;
			public VolumeType Volume;
			public string Text;
			public Response(int channel, VolumeType volume, string text){
				Channel = channel; Volume = volume;  Text = text;
			}
			public static VolumeType ParseVolume(string vol)
			{
				switch (vol.ToLower())
				{
					case "global":
						return VolumeType.Global;
					case "region":
						return VolumeType.Region;
					case "shout":
						return VolumeType.Shout;
					case "say":
						return VolumeType.Say;
					case "whisper":
						return VolumeType.Whisper;
					default:
						throw new ArgumentException("Cannot set volume to arbitrary level " + vol);
				}
			}
		}
		
		public enum ResponseSelectionMode { RandomResponse, NextResponse };
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

		public Response GetResponse()
		{
			OpenChatbagModule.os_log.Debug("[Chatbag]: Producing response");
			if (responseMode == ResponseSelectionMode.RandomResponse)
			{
				Random rand = new Random(System.DateTime.Now.Millisecond);
				int choice = rand.Next(responses.Count);
				return responses[choice];
			}
			else // NextResponse
			{
				int choice = responseCounter;
				responseCounter = (responseCounter + 1) % responses.Count;
				return responses[choice];
			}
		}

	}
}
