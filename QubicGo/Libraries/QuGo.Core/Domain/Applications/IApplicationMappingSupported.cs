namespace QuGo.Core.Domain.Stores
{
    /// <summary>
    /// Represents an entity which supports Application mapping
    /// </summary>
    public partial interface IApplicationMappingSupported
    {
        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain Application
        /// </summary>
        bool LimitedToApplications { get; set; }
    }
}
