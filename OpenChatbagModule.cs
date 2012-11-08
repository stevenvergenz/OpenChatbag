#region includes

using System;
using System.Collections.Generic;

using Mono.Addins;
using System.Reflection;

using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using OpenMetaverse;

using log4net;
using Nini.Config;

#endregion

[assembly: Addin("OpenChatbagModule", "0.1")]
[assembly: AddinDependency("OpenSim", "0.5")]

namespace OpenChatbag
{
	[Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "OpenChatbagModule")]
	public class OpenChatbagModule : ISharedRegionModule
	{
		#region Module Properties

		private readonly ILog os_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		public static List<Scene> Scenes;

		public string Name { get { return "OpenChatbagModule"; } }
		public bool IsSharedModule { get { return true; } }
		public Type ReplaceableInterface { get { return null; } }

		public List<Chatbag> chatbags;

		#endregion

		#region Module Handles
		// runs immediately after the module is loaded, before attachment to anything
		public void Initialise(IConfigSource source)
		{
			os_log.Debug("[OpenChatbag]: Initializing.");
			chatbags = ConfigParser.Parse("chatbag.xml", "chatbag.xsd");
			Scenes = new List<Scene>();
		}

		// runs after Initialize, but before modules are added
		public void PostInitialise()
		{

		}

		// runs every time a new region is added to the module
		public void AddRegion(Scene scene)
		{
			Scenes.Add(scene);
			
			scene.EventManager.OnClientMovement += PositionTracker.Instance.UpdateAvatarPosition;
			scene.EventManager.OnSceneObjectPartUpdated += PositionTracker.Instance.UpdatePrimPosition;
		}

		// runs after all modules have been loaded for each scene
		public void RegionLoaded(Scene scene)
		{
			
		}

		// runs every time a region is removed
		public void RemoveRegion(Scene scene)
		{
			Scenes.Remove(scene);
			
			scene.EventManager.OnClientMovement -= PositionTracker.Instance.UpdateAvatarPosition;
			scene.EventManager.OnSceneObjectPartUpdated -= PositionTracker.Instance.UpdatePrimPosition;
		}

		// runs post-termination
		public void Close()
		{
			os_log.Debug("[OpenChatbag]: Shutting down.");
		}
		#endregion
	}
}
