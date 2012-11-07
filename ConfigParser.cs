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
			
			try {
				readerSettings.Schemas.Add("https://github.com/stevenvergenz/OpenChatbag","chatbag.xsd");
			}
			catch( XmlSchemaException e ){
				Console.Out.WriteLine(e);
				
			}
			return null;
		}
	}
}

