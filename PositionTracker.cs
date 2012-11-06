using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using log4net;
using System.Reflection;
using OpenMetaverse;
using OpenSim.Region.Framework.Scenes;

namespace OpenChatbag
{
	public class PositionTracker
	{
		private readonly ILog os_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		public UUID Target;
		public Vector3 Position { get; protected set; }
		public Vector3 Orientation { get; protected set; }

		protected PositionTracker(UUID target)
		{
			Target = target;
			Position = new Vector3();
			Orientation = new Vector3();
		}

		/*public override void Start()
		{
			foreach (Scene s in OpenChatbagModule.Scenes)
			{
				os_log.InfoFormat("[GIFT]: Registering listener with {0}", s.RegionInfo.RegionName);
				s.EventManager.OnClientMovement += UpdatePosition;

				ScenePresence presence = s.GetScenePresence(Parent.AvatarID);
				if (presence != null)
				{
					// transform region coordinates to globals
					Vector3 pos = Position;
					Vector3 rot = Orientation;
					pos.X = s.RegionInfo.RegionLocX * 256 + presence.AbsolutePosition.X;
					pos.Y = s.RegionInfo.RegionLocY * 256 + presence.AbsolutePosition.Y;
					presence.Rotation.GetEulerAngles(out rot.X, out rot.Y, out rot.Z);
					Position = pos;
					Orientation = rot;
				}
			}

			ChatHandlerDelegate positionQuery = delegate(List<string> a){
				Parent.chatHandler.SendMessageToAvatar(String.Format("Your coordinates are {0}", Position.ToString()));
			};

			Parent.chatHandler.RegisterCommand("where_i", positionQuery);
			Parent.chatHandler.RegisterCommand("what_location|position", positionQuery);
			Parent.chatHandler.RegisterCommand("stop_tracking", delegate(List<string> a) { Stop(); });

			Parent.chatHandler.SendMessageToAvatar("Okay, we're on the clock. Let's get to work!");
			
			base.Start();
		}*/

		
		#region Static Tracker 
		
		private static Dictionary<UUID, PositionTracker> TrackerMap;
		
		static PositionTracker(){
			TrackerMap = new Dictionary<UUID, PositionTracker>();
		}
		
		public static PositionTracker addTracker(UUID target)
		{
			if( PositionTracker.TrackerMap.ContainsKey(target) )
				return PositionTracker.TrackerMap[target];
			else {
				PositionTracker tracker = new PositionTracker(target);
				PositionTracker.TrackerMap.Add(target, tracker);
				return tracker;
			}
		}
		public static PositionTracker addTracker(PositionTracker tracker)
		{
			if( tracker != null && !PositionTracker.TrackerMap.ContainsKey( tracker.Target ) ){
				PositionTracker.TrackerMap.Add( tracker.Target, tracker );
			}
			else return tracker;
		}
		
		public static bool removeTracker(UUID target)
		{
			if( PositionTracker.TrackerMap.ContainsKey(target) ){
				PositionTracker.TrackerMap.Remove(target);
				return true;
			}
			else return false;
		}
		
		public static void UpdatePosition(ScenePresence client)
		{
			if (PositionTracker.TrackerMap.ContainsKey(client.UUID))
			{
				// transform region coordinates to globals
				PositionTracker tracker = PositionTracker.TrackerMap[client.UUID];
				Vector3 pos = tracker.Position;
				Vector3 rot = tracker.Orientation;
				
				pos.X = client.Scene.RegionInfo.RegionLocX * 256 + client.AbsolutePosition.X;
				pos.Y = client.Scene.RegionInfo.RegionLocY * 256 + client.AbsolutePosition.Y;
				
				tracker.Position = pos;
				tracker.Orientation = rot;
			}
		}
		
		#endregion
	}
}
