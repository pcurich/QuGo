namespace QuGo.Core.Domain.Seo
{
    /// <summary>
    /// Represents a page title SEO adjustment
    /// </summary>
    public enum PageTitleSeoAdjustment
    {
        /// <summary>
        /// Pagename comes after storename
        /// </summary>
        PageNameAfterApplicationName = 0,
        /// <summary>
        /// Storename comes after pagename
        /// </summary>
        ApplicationNameAfterPageName = 10
    }
}
