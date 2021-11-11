## Controlling remoting direction.

While bidirectional (two-way) remoting is great, you may want to use one-way remoting in parts of your app.

Implementing two-way remoting is easy. Either:
- Setup RemotingDirection.Bidirectional on the member you'd like to be two-way.
- Set `messageHandler.Mode = RemotingMode.Full` to make the node act like both `Host` and `Client`, letting messages flow both ways regardless of RemotingDirection, except for members with `RemotingDirection` set to `None`.

Implementing one-way remoting in your app requires an understanding of
- The intent of how a Client or Host node will be used.
- How RemotingDirection and RemotingMode are used together.

Usually, a `Client` acts as a "listener" while a `Host` acts as a "sender". The actual behavior of your code doesn't matter, this is a loose guideline and an easy way to distinguish the different `RemotingMode`s.

`RemotingDirection` is a flag, and you can enable more than one option. For example, you can combine `RemotingDirection.Inbound | RemotingDirection.OutboundHost` to allow receiving in any Mode, but only allow outbound messages when in `Host` mode.

See [RemotingDirection](https://arlo.site/owlcore/api/OwlCore.Remoting.RemotingDirection.html) docs for a list of all possible directions and how they're used.

See [RemotingMode](https://arlo.site/owlcore/api/OwlCore.Remoting.RemotingMode.html) docs for more details about each remoting mode.
