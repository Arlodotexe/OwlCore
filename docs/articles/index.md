# OwlCore [![Download from Nuget](https://img.shields.io/nuget/v/OwlCore.svg)](https://www.nuget.org/packages/OwlCore/) ![Download count](https://img.shields.io/nuget/dt/OwlCore) ![License](https://img.shields.io/github/license/Arlodotexe/OwlCore) [![Documentation](https://img.shields.io/badge/Documentation-DocFX-brightgreen)](http://arlo.site/owlcore)
#### Have you ever seen an owl do a barrel roll? Me neither.

## Our favorite features

- **.NET Standard 2.0** - Battle-tested on Uno Platform and UWP, for both hobby and enterprise-level applications.
- **[OwlCore.Remoting](OwlCore.Remoting/index.html)** - Painlessly sync member changes in C# with an external source via magic (reflection + IL weaving).
- **AbstractUI** - Standardized simple UI elements as pure data. Send over a network, store on disk, share between platforms, etc. Make something else worry about rendering and interacting. (Optional OwlCore.Remoting integration)
- **AbstractStorage** - Abstract away your storage needs for easy implementation switching and unit test mocking. Based on the Windows StorageFile APIs, designed to be completely agnostic of any underlying platform or protocol.
- **Flow.Debouncer** - For when something fires repeatedly. but you only care about when it stops.
- **Flow.EventAsTask** - Wait for an EventHandler to fire (with cancellation).
- **Flow.EasySemaphore** - Use a "using" statement with your semaphores instead of manually calling WaitAsync and Release.
- **Threading.PrimaryContext** - Use a "using" statement to execute on your main thread. No dispatcher, callbacks or cleanup needed.
- **Threading.OnPrimaryThread** - Easily invoke a callback on your primary thread.
- **CompositeHttpClientHandler** - Chain multiple HttpClientHandlers together.
- **CachedHttpClientHandler** - Cache http requests to disk and return them as needed.
- **RateLimitedHttpClientHandler** - Limit the number of requests within a window of time.
- **Countless extension methods** - PruneNull, Shuffle, InParallel, ChangeDate, HashMD5Fast, DistinctBy, InsertOrAdd, and more.
- ... and so much more.
---

> OwlCore is in a rapid development cycle and is constantly being improved upon. Until a stable release, breaking changes can happen any time. 

> Comprehensive changelogs included with every release.
