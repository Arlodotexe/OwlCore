## Controlling remoting direction

There are 3 possible directional states:
- Bidirectional
- One-Way
- None (Disabled)

### Bidirectional remoting

Most of the time, Bidirectional remoting is probably what you want.

To enable bidirectional remoting, set a specific member as [`RemotingMode.Bidirectional`](../../api/OwlCore.Remoting.RemotingDirection.html#fields).
```csharp
[RemoteMethod, RemoteOptions(RemotingMode.Bidirectional)]
public void DoThing()
{
}
```

Or, set the [`IRemoteMessageHandler.Mode`](../../api/OwlCore.Remoting.Transfer.IRemoteMessageHandler.yml#OwlCore_Remoting_Transfer_IRemoteMessageHandler_Mode) to [`RemotingMode.Full`](../../api/OwlCore.Remoting.RemotingMode.html#fields) to make it act like both `Host` and `Client`. This allows messages to flow both ways regardless of the RemotingDirection specified on each member (except where direction is set to `None`).
```csharp
public class MyMessageHandler : IRemoteMessageHandler
{
    public MyMessageHandler()
    {
        Mode = RemotingMode.Full;
    }

    ...
}
```

### One-Way remoting

#### All about intent

What "Client" or "Host" mean in the context of your code is an excercise left to you. These Modes, combined with [`[RemotingDirection]`](../../api/OwlCore.Remoting.RemotingDirection.yml), are tools that allow you to accept or reject incoming and outgoing RPC messages. 

RemotingDirection is a flag, allowing you to mix and match very specific rules to fit your needs.
For example, you can use `RemotingDirection.Inbound | RemotingDirection.OutboundHost` to allow receiving in any Mode, but only allow outbound messages when in `Host` mode.

#### Example

```csharp

var host = new MyRemoteClass(RemotingMode.Host);
var client = new MyRemoteClass(RemotingMode.Client);

// Data property uses "HostToClient" remoting direction.
host.Data = 5; // RPC sent from host, client receives change.
client.Data = 6; // No RPC message sent

// OtherData property uses "ClientToHost" remoting direction.
host.OtherData = 5; // No RPC message sent
client.OtherData = 6; // RPC message sent, host receives change.

public class MyRemoteClass
{
    private readonly MemberRemote _memberRemote;

    public MyRemoteClass(RemotingMode mode)
    {
        _memberRemote = new MemberRemote(this, "UniqueButConsistentId", new MyMessageHandler(mode));
    }

    [RemoteProperty, RemoteOptions(RemotingDirection.HostToClient)]
    public int Data { get; set; }

    [RemoteProperty, RemoteOptions(RemotingDirection.ClientToHost)]
    public int OtherData { get; set; }
}


public class MyMessageHandler : IRemoteMessageHandler
{
    public MyMessageHandler(RemotingMode mode)
    {
        Mode = mode;
    }

    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        // connect to websocket
    }
    ...
}

```


#### Resources 
See [`RemotingDirection`](../../api/OwlCore.Remoting.RemotingDirection.yml) docs for a list of all possible directions and how they're used.

See [`RemotingMode`](../../api/OwlCore.Remoting.RemotingMode.yml) docs for more details about each remoting mode.

### Disabling remoting
If you find yourself needing to turn off remoting for specific members, or toggling it on/off at runtime, you can do that!

See [Toggling remoting at runtime](./toggling-remoting-at-runtime.md) for advanced scenarios.

#### Using RemotingDirection

To enable remoting for an entire class, you can use [`[RemoteMethod]`](../../api/OwlCore.Remoting.RemoteMethodAttribute.yml), [`[RemoteProperty]`](../../api/OwlCore.Remoting.RemotePropertyAttribute.yml) and [`[RemoteOptions]`](../../api/OwlCore.Remoting.RemoteOptionsAttribute.yml) on the class itself.

However, if you need to disable remoting for a certain member, set the [`[RemotingDirection]`](../../api/OwlCore.Remoting.RemotingDirection.yml) to "None".

```csharp
[RemoteProperty, RemoteMethod, RemoteOptions(RemotingDirection.Bidirectional)]
public class MyRemoteClass
{
    public int Data { get; set; }

    [RemoteOptions(RemotingDirection.None)]
    public int MoreData { get; set; }

    public void DoThing()
    {
    }
}
```