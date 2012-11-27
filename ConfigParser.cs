using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

using OpenMetaverse;

namespace OpenChatbag
{
	public static class ConfigParser
	{
		public static List<Chatbag> Parse(string filename, string schemaname)
		{
			XmlReaderSettings readerSettings = new XmlReaderSettings();
			readerSettings.IgnoreComments = true;
			readerSettings.IgnoreWhitespace = true;
			readerSettings.ValidationType = ValidationType.Schema;
			XmlSchema schema = readerSettings.Schemas.Add("https://github.com/stevenvergenz/OpenChatbag",schemaname);
			XmlReader reader = XmlReader.Create(filename, readerSettings);
			List<Chatbag> ret = new List<Chatbag>();
			reader.MoveToContent();
			
			if(reader.IsStartElement("config")){
				string channelString = reader.GetAttribute("consoleChannel");
				int channel;
				if( channelString == null || !int.TryParse(channelString, out channel) ){
					channel = 101010;
					OpenChatbagModule.os_log.WarnFormat("[Chatbag]: '{0}' is not a valid debug channel, defaulting to 101010", channelString);
				}
				ConsoleChatbag console = new ConsoleChatbag(channel);
				console.AfterInteractionsSet();
				ret.Add(console);
				reader.ReadStartElement("config");
			}
			else throw new XmlException("Document element is not correct");
			

			List<string> bagTypes = new List<string>(new string[] {
				"globalChatbag", "regionChatbag", "primChatbag"
			});
			while (reader.IsStartElement() && bagTypes.Contains( reader.Name ))
			{
				// read Chatbag object
				Chatbag chatbag;
				string name = reader.GetAttribute("name");
				if( name == null ) name = "unnamed chatbag";
				
				string uuidString = reader.GetAttribute("uuid");
				UUID uuid;
				
				if (reader.Name == "globalChatbag"){
					chatbag = new GlobalChatbag(name);
				}
				else if (reader.Name == "regionChatbag")
				{
					if( uuidString == null ){
						throw new XmlException(String.Format("Region chatbag '{0}' requires a UUID", name));
					}
					if( !UUID.TryParse(uuidString, out uuid) ){
						throw new XmlException(String.Format("'{0}' is not a valid UUID", uuidString));
					}
					chatbag = new RegionChatbag(name, uuid);
				}
				else if (reader.Name == "primChatbag")
				{
					if( uuidString == null ){
						throw new XmlException(String.Format("Prim chatbag '{0}' requires a UUID", name));
					}
					if( !UUID.TryParse(uuidString, out uuid) ){
						throw new XmlException(String.Format("'{0}' is not a valid UUID", uuidString));
					}
					
					chatbag = new PrimChatbag(name, uuid);
				}
				else throw new XmlException("Invalid Chatbag type");
				reader.ReadStartElement();

				// read list of interactions
				while (reader.IsStartElement("interaction"))
				{
					name = reader.GetAttribute("name");
					if( name == null ) name = "unnamed interaction";
					Interaction i = new Interaction( name );
					reader.ReadStartElement("interaction");

					reader.ReadStartElement("triggers");
					if (reader.Name == "fields")
					{
						reader.ReadStartElement("fields"); // read fields
						while (reader.IsStartElement("field"))
						{
							string key = reader.GetAttribute("key");
							string regex = reader.GetAttribute("regex");
							if( key == null || regex == null ) 
								throw new XmlException("Fields must have both a key and an expression");
							ChatHandler.Instance.RegisterField(key, regex);
							reader.ReadStartElement(); // read field
						}
						reader.ReadEndElement(); // end fields
					}

					List<string> triggerTypes = new List<string>(new string[] {
						"chatTrigger", "proximityTrigger"
					});
					while (reader.IsStartElement() && triggerTypes.Contains(reader.Name))
					{
						ITrigger t = null;
						if (reader.Name == triggerTypes[0]) // chatTrigger
						{
							string channelString = reader.GetAttribute("channel");
							int channel;
							if( channelString == null || !int.TryParse(channelString, out channel) ){
								channel = 0;
								OpenChatbagModule.os_log.WarnFormat("[Chatbag]: '{0}' is not a valid channel number, defaulting to 0.", channelString);
							}
							string phrase = reader.GetAttribute("phrase");
							if( phrase == null )
								throw new XmlException("Chat triggers require a trigger phrase");
							t = new ChatTrigger(phrase, channel);
						}
						else if (reader.Name == triggerTypes[1]) // proximityTrigger
						{
							if( !(chatbag is PrimChatbag)){
								OpenChatbagModule.os_log.WarnFormat(
									"[Chatbag]: Proximity triggers on {0} will be ignored, they only work on Prim chatbags!",
									chatbag.Name);
							}
							string rangeString = reader.GetAttribute("range");
							float range;
							if( rangeString == null || !float.TryParse(rangeString, out range) ){
								range = 10.0f;
								OpenChatbagModule.os_log.WarnFormat("[Chatbag]: '{0}' is not a valid range (float), defaulting to 10.0", rangeString);
							}
							t = new ProximityTrigger(range);
						}
						
						i.triggerList += t;
						reader.ReadStartElement(); // read trigger
					}

					reader.ReadEndElement(); // end triggers
					
					// read responses
					i.responses = ParseResponses(reader);
					
					/*reader.ReadStartElement("responses"); // read responses
					while (reader.IsStartElement("response"))
					{
						int channel = 0;
						Interaction.VolumeType volume =
							Interaction.Response.ParseVolume(reader.GetAttribute("volume"));
						if( volume != Interaction.VolumeType.Private )
							channel = int.Parse(reader.GetAttribute("channel"));
						reader.ReadStartElement(); // read response

						Interaction.Response r = new Interaction.Response(channel, volume, reader.ReadString());
						reader.ReadEndElement(); // end response
						i.responses.Add(r);
					}
					reader.ReadEndElement(); // end responses
					*/
					
					reader.ReadEndElement(); // end interaction
					chatbag.InteractionList.Add(i);
				}

				reader.ReadEndElement(); // end chatbag
				ret.Add(chatbag);
				chatbag.AfterInteractionsSet();
			}
			reader.ReadEndElement(); // end config
			reader.Close();

			return ret;
		}
		
		private static ResponseList ParseResponses(XmlReader reader)
		{
			if( reader.IsStartElement("response") )
			{
				int channel = 0;
				Response.VolumeType volume =
					Response.ParseVolume(reader.GetAttribute("volume"));
				if( volume != Response.VolumeType.Private )
					channel = int.Parse(reader.GetAttribute("channel"));
				reader.ReadStartElement(); // read response
				string text = reader.ReadString();
				reader.ReadEndElement(); // end response
				
				return new Response(text, channel, volume);
			}
			else if( reader.IsStartElement("responses") )
			{
				ResponseList.ResponseSelectionMode mode = ResponseList.ParseSelectionMode(reader.GetAttribute("selectionMode"));
				List<ResponseList> list = new List<ResponseList>();
				reader.ReadStartElement("responses");
				while( reader.IsStartElement() )
				{
					list.Add( ParseResponses(reader) );
				}
				reader.ReadEndElement(); // end responses
				return new ResponseList(list, mode);
			}
			else
			{
				throw new XmlException("Unexpected tag in response list: "+reader.Name);
			}
		}
	}
}

