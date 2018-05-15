using QuGo.Core.Domain.Common;

namespace QuGo.Data.Mapping.Common
{
    public partial class SearchTermMap : SysEntityTypeConfiguration<SearchTerm>
    {
        public SearchTermMap()
        {
            this.ToTable("SearchTerm");
            this.HasKey(st => st.Id);
        }
    }
}
