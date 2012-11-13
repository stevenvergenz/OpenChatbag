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
			reader.ReadStartElement("config");

			List<string> bagTypes = new List<string>(new string[] {
				"globalChatbag", "regionChatbag", "parcelChatbag", "primChatbag", "locationChatbag"
			});
			while (reader.IsStartElement() && bagTypes.Contains( reader.Name ))
			{
				// read Chatbag object
				Chatbag chatbag;
				if (reader.Name == "globalChatbag"){
					chatbag = new GlobalChatbag(reader.GetAttribute("name"));
				}
				else if (reader.Name == "regionChatbag")
				{
					chatbag = new RegionChatbag(reader.GetAttribute("name"), UUID.Parse(reader.GetAttribute("uuid")));
				}
				else if (reader.Name == "primChatbag")
				{
					chatbag = new PrimChatbag(reader.GetAttribute("name"), UUID.Parse(reader.GetAttribute("uuid")));
				}
				else throw new XmlException("Invalid Chatbag type");
				reader.ReadStartElement();

				// read list of interactions
				while (reader.IsStartElement("interaction"))
				{
					Interaction i = new Interaction( reader.GetAttribute("name") );
					reader.ReadStartElement("interaction");

					reader.ReadStartElement("triggers");
					if (reader.Name == "fields")
					{
						reader.ReadStartElement("fields"); // read fields
						while (reader.IsStartElement("field"))
						{
							Console.Out.WriteLine(String.Format("field with key {0} and regex {1}",
								reader.GetAttribute("key"), reader.GetAttribute("regex")));
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
							t = new ChatTrigger(reader.GetAttribute("phrase"), 
								int.Parse(reader.GetAttribute("channel")));
						}
						else if (reader.Name == triggerTypes[1]) // proximityTrigger
						{
							t = new ProximityTrigger(float.Parse(reader.GetAttribute("range")));
						}
						
						i.triggerList += t;
						reader.ReadStartElement(); // read trigger
					}

					reader.ReadEndElement(); // end triggers

					reader.ReadStartElement("responses"); // read responses
					while (reader.IsStartElement("response"))
					{
						int channel = int.Parse(reader.GetAttribute("channel"));
						Interaction.VolumeType volume =
							//Interaction.Response.ParseVolume(reader.GetAttribute(0));
							Interaction.Response.ParseVolume(reader.GetAttribute("volume"));
						reader.ReadStartElement(); // read response

						Interaction.Response r = new Interaction.Response(channel, volume, reader.ReadString());
						reader.ReadEndElement(); // end response
						i.responses.Add(r);
					}
					reader.ReadEndElement(); // end responses
					reader.ReadEndElement(); // end interaction
					chatbag.InteractionList.Add(i);
				}

				reader.ReadEndElement(); // end chatbag
				ret.Add(chatbag);
				chatbag.AfterInteractionsSet();
			}
			
			return ret;
		}
	}
}

