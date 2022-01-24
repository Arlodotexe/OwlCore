using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace OwlCore.Extensions
{
    /// <summary>
    /// Provides extension methods for operating on arbitrary types.
    /// </summary>
    public static partial class GenericExtensions
    {
        /// <summary>
        /// Crawls an object tree for nested properties of the same type and returns the first instance that matches the <paramref name="filterPredicate"/>. 
        /// </summary>
        /// <remarks>
        /// Does not filter against or return the <paramref name="root"/> object.
        /// </remarks>
        public static T? CrawlBy<T>(this T? root, Func<T?, T?> selectPredicate, Func<T?, bool> filterPredicate)
        {
        crawl:
            var current = selectPredicate(root);

            if (filterPredicate(current))
            {
                return current;
            }

            if (current is null)
            {
                return default;
            }

            root = current;
            goto crawl;
        }

        /// <summary>
        /// Crawls an object tree for nested properties of the same type and returns the first instance that matches the <paramref name="filterPredicate"/>. 
        /// </summary>
        /// <remarks>
        /// Does not filter against or return the <paramref name="root"/> object.
        /// </remarks>
        public static async Task<T?> CrawlByAsync<T>(this T? root, Func<T?, Task<T?>?> selectPredicate, Func<T?, bool> filterPredicate)
        {
        crawl:
            var selectTask = selectPredicate(root);

            var current = selectTask is null ? default : await selectTask;

            if (filterPredicate(current))
            {
                return current;
            }

            if (current is null)
            {
                return default;
            }

            root = current;
            goto crawl;
        }
    }
}