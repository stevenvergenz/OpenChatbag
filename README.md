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
I highly recommend starting from the sample config file included with this repo as a base to ensure that
the namespaces are correct.


### Simple config file ###

The below XML snippet is a valid OpenChatbag config block. Use it for reference as you read this document.

	<config consoleChannel="101010"
		xmlns="https://github.com/stevenvergenz/OpenChatbag" 
		xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
		xsi:schemaLocation="https://github.com/stevenvergenz/OpenChatbag chatbag.xsd">

		<globalChatbag name="Cheerful Bot">
			<interaction name="greetings">
				<triggers>
					<chatTrigger phrase="hello|hi|hey|what's up_world"/>
				</triggers>
				<responses selectionMode="all">
					<response volume="private">Hello yourself! Beautiful day, isn't it?</response>
					<response volume="global" channel="0" delay="100">Someone said hello to me! I'm so happy to be noticed!</response>
				</responses>
			</interaction>
		</globalChatbag>

	</config>


### The root element ###

By and large, you should be able to leave this tag default.

--------------------------------------------

Tag name: <config>

Attributes:

- *0 or 1* - consoleChannel [integer]: The channel used to control OpenChatbag from in-world. Defaults
to 101010 if not specified.

Children: 

- *1 or more* - <globalChatbag>, <regionChatbag> or <primChatbag>


### The Chatbag elements ###

The chatbag elements are the core of the OpenChatbag experience. There are three types: global, region,
and prim. They correspond to different scopes or zones of influence. A global chatbag will respond if its
triggers occur anywhere in the simulation. A region chatbag to triggers within the specified region. Prim
chatbags are a little different though. They will respond to chat triggers within earshot, so their
effective range changes based on whether you're shouting or whispering. 

You can think of chatbags as particular entities you're interacting with.

--------------------------------------------

Tag names: <globalChatbag>, <regionChatbag>, <primChatbag>

Attributes:

- *0 or 1* - name [string]: The name of the chatbag. Identifies any chats in-world. Defaults to
"unnamed chatbag" if not specified
- *Exactly 1, prim and region types only* - uuid [uuid]: The identifier of the target region or prim.

Children:

- *1 or more* - <interaction>


### Interactions ###

Interactions are built around a particular concept that the chatbag wants to express. It is composed
of the set of stimuli that evoke a response, and the response tree. More on these later.

--------------------------------------------

Tag name: <interaction>

Attributes:

- *0 or 1* - name [string]: The name of the interaction. Used solely for logging purposes.

Children:

- *Exactly 1* - <triggers>
- *Exactly 1* - <responses>


### Triggers ###

Triggers are particular pre-programmed phrases that OpenChatbag monitors in-world chat for. The definitions
are simplified case-insensitive regular expressions, but support embedded full regular expressions if you
desire the functionality.

In the trigger phrase syntax, desired words/phrases are separated by underscores. In English, this might be
the equivalent of elipsis dots (...). It tells the OpenChatbag parser to skip ahead to the next keyword/phrase.
Synonyms can also be specified by the use of a pipe symbol '|'. You can use this to account for misspellings
or variations in tense, person, or plurality in addition to normal word options.

If you are already familiar with regular expressions, you may wish to use the full syntax rather than this
limited subset. To this end, you can use fields. They are defined in a sub-block of triggers using full regex.
They can then be embedded into trigger phrases by surrounding the designated keyword with {braces}.

Prim chatbags support special functionality: they can be triggered whenever an avatar gets nearby. This is done
through the use of the <proximityTrigger> type. See below for usage information.

--------------------------------------------

Tag name: <triggers>

Attributes: (none)

Children:

- *0 or 1* - <fields>
- *1 or more* - <chatTrigger>, <proximityTrigger>

--------------------------------------------

Tag name: <fields>

Attributes: (none)

Children:

- *1 or more* - <field>

-------------------------------------------

Tag name: <field>

Attributes:

- *Exactly 1* - key [string]: The substitution identifier for the field.
- *Exactly 1* - regex [string]: The regular expression used to match the field.

Children: (none)

-------------------------------------------

Tag name: <chatTrigger>

Attributes:

- *0 or 1* - channel [integer]: Only triggering phrases on this channel will evoke a response.
- *Exactly 1* - phrase [string]: The phrase to listen for. Uses the previously described syntax.

Children: (none)

-------------------------------------------

Tag name: <proximityTrigger>

Attributes:

- *0 or 1* - range [integer]: The trigger is activated when an avatar gets within *range* meters of the prim.
Defaults to 10 meters when not specified.

Children: (none)


### Responses ###


