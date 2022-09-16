# Getting started
[OwlCore.Remoting](https://arlo.site/owlcore/api/OwlCore.Remoting.html) is a lightweight and ultra-flexible RPC framework for .NET Standard 2.0.

It works across different processes, different machines, different platforms, and even at different points in time.

### Prerequisites

Make sure you've installed OwlCore using the [Nuget package](https://www.nuget.org/packages/OwlCore/).

This library relies on [Cauldron.BasicInterceptors](https://capgemini.github.io/Cauldron/netstandard/html/N_Cauldron_Interception.htm) to IL weave method and property interceptors at compile time.

For now, you must manually install the [nuget package](https://www.nuget.org/packages/Cauldron.BasicInterceptors/) in your project and include the following `FodyWeavers.xml` file in the project root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Weavers>
  <Cauldron.Interception />
</Weavers>
```

Note that due to the Fody dependency, compiling OwlCore requires MSBuild running under Windows, and will not compile on platforms such as Linux. [Here](https://github.com/Capgemini/Cauldron/issues/84) is the relevant GitHub issue.

### The basics
1. [Using MemberRemote](./member-remote.md) to enable remoting in a class.
2. [Create a MessageHandler](./message-handler.md)
3. Understanding [Remoting direction](./remoting-modes-and-directions.md).


### Further reading & advanced usage
- [Toggle remoting at runtime](./toggling-remoting-at-runtime.md)
- [Remotely returning data](./remotely-returning-data.md)
- More coming soon