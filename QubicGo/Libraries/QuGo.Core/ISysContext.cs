using Nop.Core.Domain.Stores;

namespace Nop.Core
{
    /// <summary>
    /// Store context
    /// </summary>
    public interface ISysContext
    {
        /// <summary>
        /// Gets or sets the current store
        /// </summary>
        Store CurrentStore { get; }
    }
}
