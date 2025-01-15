namespace ubique.plugin;

//All plugin need to implement a function,
//IPlugin Init()
//To load the plugin.
//You should implement the function in a .dll or .so file.

public interface IPlugin
{
    //Return ture if state is ok.
    //Which means the plugin is ready to work.
    public bool State();
    
    //Add a runtime node in plugin.
    //NOTE THAT ALL RUNTIME NODES ARE CONCURRENT.
    //Return true if success.
    public bool AddRuntimeNode(string nodeName,int runtimeNodeId,out IRuntimeNode iRuntimeNode);
}

public interface IRuntimeNode
{
    //To send a communicator to the runtime node.
    //Then the runtime node should stop and wait for
    //communicator send message to it. We use communication
    //to convey message.
    public bool GetCommunicator(Communicator communicator);

    //To stop the runtime node.
    //The runtime node should stop and back to the state
    //when it get a communicator.
    //You can write nothing, but you need to return true.
    //But you better implement this function.
    public bool Stop();

    //To remove the runtime node.
    //Runtime node should release all resources and stop.
    //Return true if success.
    public bool Remove();
}