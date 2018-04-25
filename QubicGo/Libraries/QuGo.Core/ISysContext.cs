using QuGo.Core.Domain.Applications;

namespace QuGo.Core
{
    /// <summary>
    /// Store context
    /// </summary>
    public interface ISysContext
    {
        /// <summary>
        /// Gets or sets the current Application
        /// </summary>
        Application CurrentApplication { get; }
    }
}
