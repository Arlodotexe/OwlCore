# OwlCore [![Download from Nuget](https://img.shields.io/nuget/v/OwlCore.svg)](https://www.nuget.org/packages/OwlCore/)
#### Have you ever seen an Owl do a barrel roll? Me neither


## Our favorite features

- **OwlCore.Remoting** - Painlessly sync member changes in C# with an external source via magic (reflection + IL weaving).
- **AbstractUI** - Abstracts simple UI elements as pure data. Send over a network, store on disk, share between platforms, etc. Make something else worry about rendering and interacting. (Optional OwlCore.Remoting integration)
- **AbstractStorage** - Abstract away your storage needs for easy implementation switching and unit test mocking. Based on the Windows StorageFile APIs, designed to be completely agnostic of any underlying platform or protocol.
- **Provisos** - IAsyncInit, C# 9-in-8 features.
- **Flow.Debouncer** - For when something fires repeatedly. but you only care about when it stops.
- **Flow.EventAsTask** - Wait for an EventHandler to fire (with cancellation).
- **Threading.PrimaryContext** - Easy switch to your main thread via `using (Threading.PrimaryContext) { .. }`. No dispatcher, callbacks or cleanup needed.
- **Threading.OnPrimaryThread** - Easy invoke a callback to your primary thread.
- **CompositeHttpClientHandler** - Chain multiple HttpClientHandlers together.
- **CachedHttpClientHandler** - Cache http requests to disk and return them as needed.
- **RateLimitedHttpClientHandler** - Limit the number of requests within a window of time.
- **Countless extension methods** - PruneNull, Shuffle, InParallel, ChangeDate, HashMD5Fast, DistinctBy, InsertOrAdd, and more.
- ... and so much more.

---

> OwlCore is constantly being improved upon. Until a stable release, breaking changes can happen any time.

> Comprehensive changelogs included with every release.
