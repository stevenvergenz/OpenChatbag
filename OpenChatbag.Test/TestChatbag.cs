using System;
using System.Collections.Generic;
using OpenChatbag;

namespace OpenChatbag.Test
{
	public class TestChatbag
	{
		static void Main(string[] args)
		{
			List<Chatbag> doclist;
			doclist = ConfigParser.Parse(@"..\..\..\chatbag.xml");
			
			Console.Out.WriteLine("Tests completed successfully.");
			Console.Read();
		}
	}
}

