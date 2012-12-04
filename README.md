# OpenChatbag - Call/Response Module For OpenSim #

## Overview ##

OpenChatbag is an OpenSim region module that will provide canned responses to appropriate prompts.
You could have a simple greeter that says "Welcome!" whenever someone enters the room, or a duet bot
that replies "Never gonna let you down" to someone saying "Never gonna give you up". Currently, only
proximity triggers and chat triggers are supported, but in the future additional triggers may be
added.

OpenChatbag takes a bag-of-words approach to syntax. It is unaware of punctuation or parts of speech,
but will instead just check that each word in the designated pattern appears in the proper order.
Hence the name Chatbag.


## Compiling ##

OpenChatbag must be compiled against the version of OpenSim you are running on your server. Note
that the master branch of OpenChatbag uses features only available from OpenSim 0.7.4 onward. If
you need to use a version earlier than that, you will have to use the 0.7.3-support branch.
Appropriate assemblies are included in the repository, but they may not match your version.

1. Replace the DLLs in the lib/ directory with the appropriate assemblies from your OpenSim
installation.
2. Open OpenChatbag.sln with your IDE of choice (Visual Studio or MonoDevelop).
3. Compile the project.


## Installation ##

1. Copy bin/OpenChatbag.dll (just compiled) and chatbag.xsd to your opensim/bin directory.
2. Copy chatbag.xml (the definition file) to somewhere on your server's filesystem. Anywhere is fine,
   this is configurable.
3. Add a configuration block to the end of your opensim/bin/OpenSim.ini file, pointing to wherever
   you put chatbag.xml in the previous step.
		[Chatbag]
			definition_file = "/your/path/chatbag.xml"
4. Restart OpenSim, and watch the logs to make sure it loads the config file correctly.


## Configuration ##

The configuration file for OpenChatbag is an XML document, so you should be familiar with this format
before attempting to configure OpenChatbag. You will save yourself a lot of headaches by doing this.


### The root element - <config> ###

By and large, you should leave this tag alone.

Attributes:

- *0 or 1* - consoleChannel [integer]: The channel used to control OpenChatbag from in-world

Children: 

- *1+* - <globalChatbag>, <regionChatbag> or <primChatbag>


### The Chatbag elements - <globalChatbag>, <regionChatbag>, and <primChatbag> ###

These elements are the core of the OpenChatbag experience. They correspond to different scopes or
zones of influence. A global chatbag will respond if its triggers occur anywhere in the simulation.
A region chatbag to triggers within the specified region. Prim chatbags are a little different though.
They will respond to chat triggers within earshot, so their effective range changes based on whether
you're shouting or whispering. 
