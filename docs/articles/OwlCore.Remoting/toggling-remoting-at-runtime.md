## Toggle remoting at Runtime

There are several ways to toggle remoting at runtime, at different levels.

### Method 1: MessageHandler Mode
Setting `Mode = RemotingMode.None` on a message handler will disable remoting on all member remotes that use that handler. 

This approach can give you a global or per-instance on-off switch at runtime, depending on how you've scoped your message handlers.

### Method 2: Delaying setup of MemberRemote
It's not always feasible to create a new message handler for every MemberRemote, just to toggle remoting at runtime. 

As stated in the [Member remote](./member-remote.md) docs, a class instance will not receive member changes until you pass it to a member remote.

That means that by creating your MemberRemote instance outside the constructor, you can keep remoting disabled until the code requires it.


```csharp
using OwlCore.Remoting;

public class MyClass : IDisposable
{
    private MemberRemote _memberRemote;

    public virtual OnRemotingReady(string id)
    {
        _memberRemote = new MemberRemote(this, id);
    }

    [RemoteProperty, RemoteOptions(RemotingDirection.Bidirectional)] 
    public int CurrentIndex { get; set; }

    [RemoteMethod, RemoteOptions(RemotingDirection.InboundHost | RemotingDirection.Outbound)] 
    public void SomeMethod(int data, string[] moreData)
    {
        // code here...
    }

    public void Dispose() => _memberRemote.Dispose();
}
```

