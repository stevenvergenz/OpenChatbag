using System;
using System.Collections.Generic;

namespace OpenChatbag
{
	
	public class ResponseList
	{
		public enum ResponseSelectionMode { Random, Next, All };
		
		private List<ResponseList> _list;
		private int _responseCounter;
		private ResponseSelectionMode _mode;
		
		public ResponseList(List<ResponseList> list, ResponseSelectionMode mode)
		{
			_list = list;
			_responseCounter = 0;
			_mode = mode;
		}
		
		public virtual List<Response> GetResponse()
		{
			List<Response> ret = new List<Response>();
			
			if( _mode == ResponseSelectionMode.Random )
			{
				Random rand = new Random();
				int choice = rand.Next(_list.Count);
				ret.AddRange( _list[choice].GetResponse() );
			}
			else if( _mode == ResponseSelectionMode.Next )
			{
				int choice = _responseCounter;
				_responseCounter = (_responseCounter+1)%_list.Count;
				ret.AddRange( _list[choice].GetResponse() );
			}
			else if( _mode == ResponseSelectionMode.All )
			{
				foreach( ResponseList response in _list )
				{
					ret.AddRange( response.GetResponse() );
				}
			}
			return ret;
		}
		
		public static ResponseSelectionMode ParseSelectionMode(string input)
		{
			switch( input.ToLower() ){
			case "random":
				return ResponseSelectionMode.Random;
			case "next":
				return ResponseSelectionMode.Next;
			case "all":
				return ResponseSelectionMode.All;
			default:
				throw new ArgumentException("Not a recognized selection level: "+input);
			
			}
		}
	}
	
	public class Response : ResponseList
	{
		public enum VolumeType { Global, Region, Shout, Say, Whisper, Private }
		
		public int Channel;
		public VolumeType Volume;
		public string Text;
		public uint Delay;
		
		public Response(string text, int channel = 0, VolumeType volume = VolumeType.Say, uint delay = 500)
		{
			Channel = channel; 
			Volume = volume;  
			Text = text;
			Delay = delay;
		}
		
		public override List<Response> GetResponse()
		{
			List<Response> ret = new List<Response>();
			ret.Add(this);
			return ret;
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
				case "private":
					return VolumeType.Private;
				default:
					throw new ArgumentException("Not a recognized volume level: " + vol);
			}
		}
	}
}

