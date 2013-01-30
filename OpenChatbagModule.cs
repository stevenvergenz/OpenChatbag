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

		public static readonly ILog os_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		public static List<Scene> Scenes;

		public string Name { get { return "OpenChatbagModule"; } }
		public bool IsSharedModule { get { return true; } }
		public Type ReplaceableInterface { get { return null; } }

		public static List<Chatbag> chatbags;

		public static int WhisperDistance = 10;
		public static int SayDistance = 20;
		public static int ShoutDistance = 100;
		public static string ConfigFile = "chatbag.xml";
		public static int ProximityTimeout = 30;
		
		#endregion
		
		public static void LoadChatbagConfig()
		{
			try
			{
				os_log.Debug("[Chatbag]: Loading config file: "+ConfigFile);
				chatbags = ConfigParser.Parse(ConfigFile, "chatbag.xsd");
			}
			catch( Exception e ){
				chatbags = new List<Chatbag>();
				ConsoleChatbag console = new ConsoleChatbag(101010);
				console.AfterInteractionsSet();
				chatbags.Add(console);
				throw e;
			}
		}
		
		#region Module Handles
		// runs immediately after the module is loaded, before attachment to anything
		public void Initialise(IConfigSource source)
		{
			os_log.Debug("[Chatbag]: Build 72ebc4aabc9f56af");
			os_log.Debug("[Chatbag]: Initializing...");
			Scenes = new List<Scene>();
			if (source.Configs["Chat"].Contains("whisper_distance"))
				WhisperDistance = source.Configs["Chat"].GetInt("whisper_distance");
			if (source.Configs["Chat"].Contains("say_distance"))
				SayDistance = source.Configs["Chat"].GetInt("say_distance");
			if (source.Configs["Chat"].Contains("shout_distance"))
				ShoutDistance = source.Configs["Chat"].GetInt("shout_distance");
			
			IConfig cbconfig = source.Configs["Chatbag"];
			if( cbconfig != null )
			{
				if( cbconfig.Contains("definition_file") )
					ConfigFile = source.Configs["Chatbag"].GetString("definition_file");
				if( cbconfig.Contains("proximity_timeout") )
					ProximityTimeout = source.Configs["Chatbag"].GetInt("proximity_timeout");
			}
		}

		// runs after Initialize, but before modules are added
		public void PostInitialise()
		{
			try {
				LoadChatbagConfig();
			}
			catch (Exception e) {
				os_log.Error("[Chatbag]: Failed to load config file, loading console only! ", e);
			}
		}

		// runs every time a new region is added to the module
		public void AddRegion(Scene scene)
		{
			Scenes.Add(scene);

			scene.EventManager.OnClientMovement += PositionTracker.Instance.UpdateAvatarPosition;
			scene.EventManager.OnSceneObjectPartUpdated += PositionTracker.Instance.UpdatePrimPosition;
			scene.EventManager.OnChatFromClient += ChatHandler.Instance.HandleChatInput;
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
			scene.EventManager.OnChatFromClient -= ChatHandler.Instance.HandleChatInput;
		}

		// runs post-termination
		public void Close()
		{
			os_log.Debug("[Chatbag]: Shutting down.");
		}
		#endregion
	}
}
