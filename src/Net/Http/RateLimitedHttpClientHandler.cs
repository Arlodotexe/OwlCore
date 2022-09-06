using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Extensions;

namespace OwlCore.Net.Http
{
    /// <summary>
    /// An <see cref="HttpClientHandler"/> that provides rate limiting functionality.
    /// </summary>
    /// <remarks>
    /// This is a <see cref="DelegatingHandler"/>. By default, the <see cref="DelegatingHandler.InnerHandler"/> property is an <see cref="HttpClientHandler"/> so it can handle HTTP requests without further config.
    /// <para/>
    /// You can assign a different <c>InnerHandler</c>, including other <see cref="DelegatingHandler"/>s, to chain handlers together.
    /// </remarks>
    public class RateLimitedHttpClientHandler : DelegatingHandler
    {
        private readonly TimeSpan _cooldownWindowTimeSpan;
        private readonly int _maxNumberOfRequestsPerCooldownWindow;
        private static readonly SemaphoreSlim _mutex = new(1, 1);
        private static readonly Queue<DateTime> _requestTimestampsInCooldownWindow = new Queue<DateTime>();

        /// <summary>
        /// Creates a new instance of <see cref="RateLimitedHttpClientHandler"/>.
        /// </summary>
        /// <param name="cooldownWindowTimeSpan">The amount of time before the cooldown window resets. Requests that are this old no longer count towards <paramref name="maxNumberOfRequestsPerCooldownWindow"/>.</param>
        /// <param name="maxNumberOfRequestsPerCooldownWindow">The maximum number of requests allowed per cooldown window.</param>
        public RateLimitedHttpClientHandler(TimeSpan cooldownWindowTimeSpan, int maxNumberOfRequestsPerCooldownWindow)
        {
            _cooldownWindowTimeSpan = cooldownWindowTimeSpan;
            _maxNumberOfRequestsPerCooldownWindow = maxNumberOfRequestsPerCooldownWindow;

            InnerHandler = new HttpClientHandler();
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (await _mutex.DisposableWaitAsync(cancellationToken: cancellationToken))
            {
                // store the timestamp of the request being made
                _requestTimestampsInCooldownWindow.Enqueue(DateTime.Now);

                // if a request is older than now - cooldown, it's outside the cooldown window and should be removed.
                while (_requestTimestampsInCooldownWindow.Count > 0 && DateTime.Now.Subtract(_cooldownWindowTimeSpan).Ticks > _requestTimestampsInCooldownWindow.Peek().Ticks)
                {
                    _requestTimestampsInCooldownWindow.Dequeue();
                }

                // if there are too many requests in the current cooldown window
                // then delay [time window] - [amount of time passed since the request was made] - remaining time until the oldest request goes outside the cooldown window
                if (_requestTimestampsInCooldownWindow.Count > _maxNumberOfRequestsPerCooldownWindow)
                {
                    var timeSinceOldestRequestWasMade = DateTime.Now - _requestTimestampsInCooldownWindow.Peek();

                    await Task.Delay(_cooldownWindowTimeSpan - timeSinceOldestRequestWasMade, cancellationToken);
                }

                return await base.SendAsync(request, cancellationToken);
            }
        }
    }
}
