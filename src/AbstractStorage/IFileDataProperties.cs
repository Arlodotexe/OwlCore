using System;
using System.Threading.Tasks;

namespace OwlCore.AbstractStorage
{
    /// <summary>
    /// Provides access to the properties of a file.
    /// </summary>
    [Obsolete("AbstractStorage is being deprecated in favor of OwlCore.Storage, and will be removed in a future version. Please migrate to the new package.")]
    public interface IFileDataProperties
    {
        /// <summary>
        /// Returns music properties.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the requested <see cref="MusicFileProperties"/> if found. Otherwise null.</returns>
        Task<MusicFileProperties?> GetMusicPropertiesAsync();
    }
}