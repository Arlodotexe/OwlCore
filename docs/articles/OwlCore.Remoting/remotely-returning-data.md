## Asynchronously returning data from another node
For remote methods, it's fire-and-forget, but what if you want to `await` some data so you can return it?

See [DataProxy](https://arlo.site/owlcore/api/OwlCore.Remoting.DataProxy.html)

---

#### !! Caution !!
- These docs are incomplete.
- These helpers are in flux and are subject to breaking changes.
- These helpers only work as expected when 2 nodes are present. 
    - Without extreme care (perfect synchronization and load balanced message distribution), > 2 nodes can cause unwanted behavior.

MemberRemote.PublishDataAsync - TODO

MemberRemote.ReceiveDataAsync - TODO
