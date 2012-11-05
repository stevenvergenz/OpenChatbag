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
	public class PositionTracker : BaseHeartbeater
	{
		protected GIFTCapsule Parent;

		private string m_topicName;
		public string DestinationTopic{
			get	{
				return m_topicName;
			}
			set	{
				m_topicName = value;
				Destination = new ActiveMQTopic(value);
			}
		}
		public GiftMessage templateMessage{
			get	{
				return TemplateMessage;
			}
			set	{
				TemplateMessage = value;
			}
		}

		public Vector3 Position { get; protected set; }
		public Vector3 Orientation { get; protected set; }
		//StreamWriter fileWriter;

		public PositionTracker(GIFTCapsule parent, MessageBroker broker) : base(broker)
		{
			Parent = parent;
			Position = new Vector3();
			Orientation = new Vector3();
		}

		public override void Start()
		{
			if (IsActive) return;

			foreach (Scene s in GIFTConnector.Scenes)
			{
				GIFTCapsule.os_log.InfoFormat("[GIFT]: Registering listener with {0}", s.RegionInfo.RegionName);
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
		}

		public override void Stop()
		{
			if (IsActive)
			{
				base.Stop();
				foreach(Scene s in GIFTConnector.Scenes){
					s.EventManager.OnClientMovement -= UpdatePosition;
				}
				Parent.chatHandler.SendMessageToAvatar("Whew, we're done!");
			}
		}

		protected override void PrepareMessage()
		{
			TemplateMessage["location.POINT_X"] = Position.X;
			TemplateMessage["location.POINT_Y"] = Position.Y;
			TemplateMessage["location.POINT_Z"] = Position.Z;
			TemplateMessage["orientation.VECTOR_X"] = Orientation.X;
			TemplateMessage["orientation.VECTOR_Y"] = Orientation.Y;
			TemplateMessage["orientation.VECTOR_Z"] = Orientation.Z;
		}

		protected void UpdatePosition(ScenePresence client)
		{
			if (client.UUID != Parent.AvatarID)
				return;

			// transform region coordinates to globals
			Vector3 pos = Position;
			Vector3 rot = Orientation;
			pos.X = client.Scene.RegionInfo.RegionLocX * 256 + client.AbsolutePosition.X;
			pos.Y = client.Scene.RegionInfo.RegionLocY * 256 + client.AbsolutePosition.Y;
			client.Rotation.GetEulerAngles(out rot.X, out rot.Y, out rot.Z);
			Position = pos;
			Orientation = rot;
		}
	}
}
