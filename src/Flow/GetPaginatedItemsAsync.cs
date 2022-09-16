using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwlCore
{
    public partial class Flow
    {
        /// <summary>
        /// A delegate returning paginated items, given an offset and limit.
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="offset">The position to start at when getting items.</param>
        public delegate Task<IEnumerable<TResult>> GetPaginatedItemsHandler<TResult>(int offset);

        /// <summary>
        /// Loads all available items from the given page handler.
        /// </summary>
        /// <param name="total">The total number of items to get.</param>
        /// <param name="handler">A <see cref="GetPaginatedItemsHandler{TResult}"/> used to load paginated results</param>
        public static async Task<IReadOnlyList<TResult>> GetPaginatedItemsAsync<TResult>(int total, GetPaginatedItemsHandler<TResult> handler)
        {
            // Get the items from the first page
            var list = new List<TResult>();

            // Get the remaining items
            while (list.Count < total)
            {
                var page = await handler(list.Count);
                // ReSharper disable once ConstantConditionalAccessQualifier
                var results = page as TResult[] ?? page?.ToArray();

                if (results == null || !results.Any())
                    return list;

                list.AddRange(results);
            }

            return list;
        }

        /// <summary>
        /// Loads a list of items of a given type from the given endpoints
        /// </summary>
        /// <param name="total">The total number of items to get.</param>
        /// <param name="startingOffset">The offset to start at.</param>
        /// <param name="handler">A <see cref="Func{T,Result}"/> used to load paginated results</param>
        public static async Task<IReadOnlyList<TResult>> GetPaginatedItemsAsync<TResult>(int total, int startingOffset, GetPaginatedItemsHandler<TResult> handler)
        {
            // Get the items from the first page
            var list = new List<TResult>();

            // Get the remaining items
            while (list.Count < total)
            {
                var page = await handler(list.Count + startingOffset);
                // ReSharper disable once ConstantConditionalAccessQualifier
                var results = page as TResult[] ?? page?.ToArray();

                if (results == null || !results.Any())
                    return list;

                list.AddRange(results);
            }

            return list;
        }

        /// <summary>
        /// Loads a list of items of a given type from the given endpoints
        /// </summary>
        /// <param name="total">The total number of items to get.</param>
        /// <param name="firstPageItems">An existing list of items from the first page.</param>
        /// <param name="handler">A <see cref="Func{T,Result}"/> used to load paginated results</param>
        public static async Task<IReadOnlyList<TResult>> GetPaginatedItemsAsync<TResult>(int total, List<TResult> firstPageItems, GetPaginatedItemsHandler<TResult> handler)
        {
            // The items from the first page are already supplied.
            var list = firstPageItems;

            // Get the remaining items
            while (true)
            {
                var page = await handler(list.Count);
                // ReSharper disable once ConstantConditionalAccessQualifier
                var results = page as TResult[] ?? page?.ToArray();

                if (results == null || !results.Any())
                    return list;

                list.AddRange(results);

                if (list.Count >= total)
                    break;
            }

            return list;
        }
    }
}
