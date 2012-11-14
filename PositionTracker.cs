using System;
using System.Collections.Generic;

using log4net;
using System.Reflection;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Region.Framework;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Region.Framework.Interfaces;


namespace OpenChatbag
{
	public delegate void RangeChangeDelegate(PositionState s, float range);

	public class PositionState
	{
		public event RangeChangeDelegate OnRangeChange;
		public enum TargetType { Unknown=0, Region, Parcel, Prim };

		public UUID Target { get; protected set; }
		public TargetType Type { get; protected set; }
		public Vector3 Position { get; set; }
		public List<float> NearbyRadii { get; protected set; }
		public Dictionary<float,List<UUID>> NearbyAvatars { get; protected set; }

		public PositionState(UUID target)
		{
			Target = target;
			Position = new Vector3();
			NearbyRadii = new List<float>();
			NearbyAvatars = new Dictionary<float, List<UUID>>();
			

			// detect target type
			foreach (Scene s in OpenChatbagModule.Scenes)
			{
				// check if it's a region
				if (target == s.RegionInfo.RegionID){
					Type = TargetType.Region;
					NearbyAvatars.Add(0, new List<UUID>());
					break;
				}

				foreach (ILandObject land in s.LandChannel.AllParcels())
				{
					// check if it's a parcel
					if (target == land.LandData.GlobalID){
						Type = TargetType.Parcel;
						NearbyAvatars.Add(0, new List<UUID>());
						break;
					}
				}
				if (Type != TargetType.Unknown) break;

				// check if it's a prim
				SceneObjectPart p = s.GetSceneObjectPart(target);
				if (p != null){
					Type = TargetType.Prim;
					Position = PositionTracker.ToGlobalCoordinates(p.ParentGroup.Scene.RegionInfo, p.AbsolutePosition);
					break;
				}
			}

			if (Type == TargetType.Unknown)
				throw new ArgumentException("Given UUID (" + target.ToString() + ") is not in the world");
		}

		public void TriggerOnRangeChange(float range){
			OnRangeChange.Invoke(this, range);
		}
	}
	
	public class PositionTracker
	{
		#region singleton handling
		private static PositionTracker instance;
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
				
				string type = "derp";
				if( tracker.Type == PositionState.TargetType.Region ) type = "Region";
				else if( tracker.Type == PositionState.TargetType.Prim) type = "Prim";
				OpenChatbagModule.os_log.Debug("[Chatbag]: Adding new tracker of type " + type);
				
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

		public void ClearTrackers()
		{
			TrackerMap.Clear();
		}

		public void UpdateAvatarPosition(ScenePresence client)
		{
			Vector3 avatarPosition = ToGlobalCoordinates(client.Scene.RegionInfo, client.AbsolutePosition);
			foreach (PositionState poi in TrackerMap.Values)
			{
				lock (poi)
				{
					if (poi.Type == PositionState.TargetType.Prim)
					{
						foreach (float range in poi.NearbyRadii)
						{
							if (!poi.NearbyAvatars.ContainsKey(range))
								poi.NearbyAvatars.Add(range, new List<UUID>());

							bool inZone = Vector3.Distance(avatarPosition, poi.Position) < range;
							if (inZone && !poi.NearbyAvatars[range].Contains(client.UUID))
							{
								poi.NearbyAvatars[range].Add(client.UUID);
								poi.TriggerOnRangeChange(range);
							}
							else if (!inZone && poi.NearbyAvatars[range].Contains(client.UUID))
							{
								poi.NearbyAvatars[range].Remove(client.UUID);
							}
						}
					}
					else if (poi.Type == PositionState.TargetType.Parcel)
					{
						if (poi.Target == client.currentParcelUUID 
							&& !poi.NearbyAvatars[0].Contains(client.UUID))
						{
							poi.NearbyAvatars[0].Add(client.UUID);
							poi.TriggerOnRangeChange(0);
						}
						else if (poi.NearbyAvatars[0].Contains(client.UUID) 
							&& poi.Target != client.currentParcelUUID)
						{
							poi.NearbyAvatars[0].Remove(client.UUID);
						}
					}
					else if (poi.Type == PositionState.TargetType.Region)
					{
						if (poi.Target == client.Scene.RegionInfo.RegionID
							&& !poi.NearbyAvatars[0].Contains(client.UUID))
						{
							OpenChatbagModule.os_log.Debug("[Chatbag]: Movement into region " + poi.Target);
							poi.NearbyAvatars[0].Add(client.UUID);
							poi.TriggerOnRangeChange(0);
						}
						else if (poi.NearbyAvatars[0].Contains(client.UUID) 
							&& poi.Target != client.Scene.RegionInfo.RegionID)
						{
							poi.NearbyAvatars[0].Remove(client.UUID);
						}
					}
				}
			}
		}

		public void UpdatePrimPosition(SceneObjectPart sop, bool full)
		{
			if (TrackerMap.ContainsKey(sop.UUID))
			{
				OpenChatbagModule.os_log.DebugFormat("[Chatbag]: Updating location of {0}", sop.UUID.ToString());
				// transform region coordinates to globals
				PositionState tracker = TrackerMap[sop.UUID];
				lock (tracker)
				{
					tracker.Position = ToGlobalCoordinates(sop.ParentGroup.Scene.RegionInfo, sop.AbsolutePosition);
					Dictionary<float, bool> sendUpdate = new Dictionary<float, bool>();
					
					foreach (ScenePresence presence in sop.ParentGroup.Scene.GetScenePresences())
					//sop.ParentGroup.Scene.SceneGraph.ForEachAvatar( new Action<ScenePresence>( presence =>
					{
						Vector3 coord = ToGlobalCoordinates(sop.ParentGroup.Scene.RegionInfo, sop.AbsolutePosition);
						float range = Vector3.Distance(coord, tracker.Position);
						foreach (float radius in tracker.NearbyRadii)
						{
							if (range < radius && !sendUpdate.ContainsKey(radius))
								sendUpdate.Add(radius, true);
						}
					}
					foreach (float range in sendUpdate.Keys)
					{
						if( sendUpdate[range] )
							tracker.TriggerOnRangeChange(range);
					}
				}
			}
		}

		public static Vector3 ToGlobalCoordinates(RegionInfo region, Vector3 position)
		{
			Vector3 ret = new Vector3();
			ret.X = region.RegionLocX * 256 + position.X;
			ret.Y = region.RegionLocY * 256 + position.Y;
			ret.Z = position.Z;

			return ret;
		}
	}
}
