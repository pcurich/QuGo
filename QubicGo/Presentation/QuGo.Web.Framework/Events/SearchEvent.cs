using System.Collections.Generic;

namespace QuGo.Web.Framework.Events
{
    /// <summary>
    /// Product search event
    /// </summary>
    public class SearchEvent
    {
        public string SearchTerm { get; set; }
        public bool SearchInDescriptions { get; set; }
        /*
         *public IList<int> CategoryIds { get; set; }
        public int ManufacturerId { get; set; }
        public int WorkingLanguageId { get; set; }
        public int VendorId { get; set; }
         *
         */
    }
}
