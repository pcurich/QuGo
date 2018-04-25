using QuGo.Core.Domain.Applications;

namespace QuGo.Core.Domain.Stores
{
    /// <summary>
    /// Represents a application mapping record
    /// </summary>
    public partial class ApplicationMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity name
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the application identifier
        /// </summary>
        public int ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the application
        /// </summary>
        public virtual Application Application { get; set; }
    }
}
