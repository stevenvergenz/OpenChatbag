using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace OpenChatbag
{
	public static class ConfigParser
	{
		public static List<Chatbag> Parse(string filename)
		{
			XmlReaderSettings readerSettings = new XmlReaderSettings();
			readerSettings.IgnoreComments = true;
			readerSettings.IgnoreWhitespace = true;
			readerSettings.ValidationType = ValidationType.Schema;
			XmlSchema schema = readerSettings.Schemas.Add("https://github.com/stevenvergenz/OpenChatbag","..\\..\\..\\chatbag.xsd");
			XmlReader reader = XmlReader.Create(filename, readerSettings);

			reader.MoveToContent();
			reader.ReadStartElement("config");

			List<string> bagTypes = new List<string>(new string[] {
				"globalChatbag", "regionChatbag", "parcelChatbag", "primChatbag", "locationChatbag"
			});
			while (reader.IsStartElement() && bagTypes.Contains( reader.Name ))
			{
				// read Chatbag object
				Chatbag chatbag;
				string bagType = reader.Name;
				if (bagType == "globalChatbag")
				{
					chatbag = new GlobalChatbag(reader.GetAttribute("name"));
				}
				else throw new XmlException("Invalid Chatbag type");
				reader.ReadStartElement();

				// read list of interactions
				while (reader.IsStartElement("interaction"))
				{
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
						Console.Out.WriteLine(reader.Name);
						reader.ReadStartElement();
					}

					reader.ReadEndElement(); // triggers

					reader.ReadStartElement("responses"); // responses
					while (reader.IsStartElement("response"))
					{
						int channel = int.Parse(reader.GetAttribute("channel"));
						reader.ReadStartElement(); // response
						Console.Out.WriteLine(channel.ToString() + " " + reader.ReadString());
						reader.ReadEndElement(); // response
						
					}
					reader.ReadEndElement(); // responses
					reader.ReadEndElement(); // interaction
				}

				reader.ReadEndElement(); // chatbag
			}
			
			return null;
		}
	}
}

