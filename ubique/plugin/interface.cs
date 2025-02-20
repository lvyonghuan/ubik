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
    
    //Remove a runtime node in plugin.
    //Return true if success.
    public bool RemoveRuntimeNode(int runtimeNodeId);
}

public interface IRuntimeNode
{
    //To send user's params to node, return true if success.
    //If the node don't have user params,
    //just return true.
    public bool GetUserParams(string key,object value);
    
    //To send a communicator to the runtime node.
    //Then the runtime node should stop and wait for
    //communicator send message to it. We use communication
    //to convey message.
    public bool GetCommunicator(Communicator communicator);

    //To stop the runtime node.
    //The runtime node should stop and back to the state
    //when it gets a communicator.
    //You can write nothing, but you need to return true.
    //But you better implement this function.
    public bool Stop();
    
    //If node is beginning node, implement it to run.
    //Return true if success.
    //if not beginning node, just return true.
    public bool Run();
    
    //Report current status of the node.
    public Status ReportStatus(Status status);
}