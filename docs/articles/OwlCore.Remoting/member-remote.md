## Member remote

The [MemberRemote](../../api/OwlCore.Remoting.MemberRemote.yml) enables sending and receiving RPC calls between two remoting-enabled object instances.

A MemberRemote does two things:

1) Emit outgoing member change data to the supplied [Message Handler](message-handler.md), using IL weaved method call and property setter interceptors.
2) Applies member change messages emitted by the supplied [Message Handler](message-handler.md) which are targeted to a MemberRemote with the same ID.

It does this by IL weaving interceptors at compile time. When a property/field setter or method call is intercepted, MemberRemote gathers the information needed to replicate the unique member change and emits an [IRemoteMemberMessage](../../api/OwlCore.Remoting.Transfer.IRemoteMemberMessage.yml), which is passed to the provided [Message Handler](message-handler.md).

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
