# Overview
OwlCore.Remoting uses IL Weaving and reflection to enable easy syncing of class instances.

It'll work across different machines, different platforms, different codebases (with common types), even different points in time.

It's stupid easy, stupid fun, and stupid powerful.

### Weavers and setup 

Member interception is handled via IL weavers and attributes that inject code at compile time.

For now, you must manually install [this package](https://www.nuget.org/packages/Cauldron.BasicInterceptors/) in your project and include the following `FodyWeavers.xml` file in the project root to enable weaving.

```xml
<?xml version="1.0" encoding="utf-8"?>
<Weavers>
  <Cauldron.Interception />
</Weavers>
```

This will be fixed in a future release.

