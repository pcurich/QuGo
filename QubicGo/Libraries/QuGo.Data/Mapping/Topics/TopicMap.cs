using QuGo.Core.Domain.Topics;

namespace QuGo.Data.Mapping.Topics
{
    public class TopicMap : SysEntityTypeConfiguration<Topic>
    {
        public TopicMap()
        {
            this.ToTable("Topic");
            this.HasKey(t => t.Id);
        }
    }
}
