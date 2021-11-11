## Remote message handler

To delegate remote member change messages between machines, you must implement the `OwlCore.Remoting.Transfer.IRemoteMessageHandler` interface.

This instance can be passed to an individual `MemberRemote` to handle messages for that particular instance.

Or, it can be used as the default for all by setting `MemberRemote.SetDefaultMessageHandler(myHandler);`.

### Example message handler:

```csharp
public class MyMessageHandler : IRemoteMessageHandler
{
    public MyMessageHandler()
    {
        // The inbox newtonsoft converter. Tested with various types, structs, primitives, classes and more.
        // Provided by the lib for convenience, the underlying OwlCore.Remoting system doesn't use this property.
        MessageConverter = new NewtonsoftRemoteMessageConverter();
        
        // Set to client or host on different devices to specify direction for one-way remoting.
        // See inline RemotingDirection and RemotingMode docs for more info.
        Mode = RemotingMode.Client | RemotingMode.Host | RemotingMode.Full;
    }

    public RemotingMode Mode {  get; set; }

    public IRemoteMessageConverter MessageConverter { get; }

    public bool IsInitialized { get; private set; }

    // Raise this event and pass an instance of IRemoteMessage to tell all MemberRemotes to apply the member change.
    // Scoping to the correct member remote is handled automatically, assuming the data is accurate.
    public event EventHandler<IRemoteMessage>? MessageReceived;

    public Task InitAsync()
    {
        // Any async initialization needed for this handler, such as connecting to a server.
        // MemberRemote will call this automatically when it needs to.
        IsInitialized = true;
        return Task.CompletedTask;
    }

    // Automatically called by the underlying library whenever a remoting message should be sent.
    public Task SendMessageAsync(IRemoteMessage memberMessage, CancellationToken? cancellationToken = null)
    {
        // Serialize and send the message.
        return Task.CompletedTask;
    }
}

```