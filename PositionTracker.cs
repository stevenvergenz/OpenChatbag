﻿using System;
using System.Collections.Generic;

using log4net;
using System.Reflection;
using OpenMetaverse;
using OpenSim.Region.Framework;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Region.Framework.Interfaces;


namespace OpenChatbag
{
	public class PositionState
	{
		public UUID Target { get; protected set; }
		public Vector3 Position { get; set; }

		public PositionState(UUID target)
		{
			Target = target;
			Position = new Vector3();
		}
	}
	
	public class PositionTracker
	{
		#region singleton handling
		public static PositionTracker instance;
		private PositionTracker() {
			TrackerMap = new Dictionary<UUID, PositionState>();
		}
		public static PositionTracker Instance {
			get {
				if (instance == null){
					instance = new PositionTracker();
				}
				return instance;
			}
		}
		#endregion

		private Dictionary<UUID, PositionState> TrackerMap;
		
		public PositionState addTracker(UUID target)
		{
			if( TrackerMap.ContainsKey(target) )
				return TrackerMap[target];
			else {
				PositionState tracker = new PositionState(target);
				TrackerMap.Add(target, tracker);
				return tracker;
			}
		}
		public PositionState addTracker(PositionState tracker)
		{
			if( tracker != null && !TrackerMap.ContainsKey( tracker.Target ) ){
				TrackerMap.Add( tracker.Target, tracker );
			}
			return tracker;
		}
		
		public bool removeTracker(UUID target)
		{
			if( TrackerMap.ContainsKey(target) ){
				TrackerMap.Remove(target);
				return true;
			}
			else return false;
		}
		
		public void UpdatePosition(ScenePresence client)
		{
			if (TrackerMap.ContainsKey(client.UUID))
			{
				// transform region coordinates to globals
				PositionState tracker = TrackerMap[client.UUID];
				Vector3 pos = tracker.Position;
				pos.X = client.Scene.RegionInfo.RegionLocX * 256 + client.AbsolutePosition.X;
				pos.Y = client.Scene.RegionInfo.RegionLocY * 256 + client.AbsolutePosition.Y;
				tracker.Position = pos;
			}
		}
		
	}
}
