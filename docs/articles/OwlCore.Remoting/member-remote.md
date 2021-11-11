## Member remote

Given a message handler, a MemberRemote does two things:
1) Emit outgoing member change data, by intercepting method calls and property changes.
   - It does this by weaving properties and methods with interceptors at compile time, and generating data that uniquely identifies the action, instance, property/method, and data needed to replicate the change. 
2) Apply incoming member change data, when the message handler is given data.
   - A member change message contains all the data needed to identify what and where a member change occured. MemberRemote uses reflection to apply the change at runtime.

## Example

```csharp
using OwlCore.Remoting;

// These attribute can be applied to a specific property/method, or an entire class.
// It even works when MemberRemote is in a base class!
[RemoteProperty]
[RemoteMethod]
[RemoteOptions(RemotingDirection.ClientToHost)]
public class MyClass : IDisposable
{
    private MemberRemote _memberRemote;

    public MemberRemote()
    {
        // Pass the instance you want to remote into MemberRemote() with an ID that is identical on both machines for that instance.
        // An instance will not receive member changes until you do this.
        // Optionally leave out the message handler. Uses the default set by MemberRemote.SetDefaultMessageHandler(handler);
        _memberRemote = new MemberRemote(this, "InstanceIdThatMatchesOnBothMachines", myMessageHandler);
    }

    // When the property setter is called, the value is captured and the property setter is invoked remotely.
    [RemoteProperty, RemoteOptions(RemotingDirection.Bidirectional)] 
    public int CurrentIndex { get; set; }

    // Method will be called remotely, including parameters.
    [RemoteMethod, RemoteOptions(RemotingDirection.InboundHost | RemotingDirection.Outbound)] 
    public void SomeMethod(int data, string[] moreData)
    {
        // code here ...
        // Execution may complete before other machines are done. 
        // If you need to wait for other machines to finish execution, use the _memberRemote.RemoteWaitAsync() and _memberRemote.RemoteReleaseAsync() extension methods.
    }

    public void Dispose()
    {
        // Dispose of the MemberRemote when finished. Forgetting to do this WILL result in a memory leak.
        _memberRemote.Dispose();
    }
}
```
