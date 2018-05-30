using QuGo.Core.Domain.Messages;

namespace QuGo.Data.Mapping.Messages
{
    public partial class NewsLetterSubscriptionMap : SysEntityTypeConfiguration<NewsLetterSubscription>
    {
        public NewsLetterSubscriptionMap()
        {
            this.ToTable("NewsLetterSubscription");
            this.HasKey(nls => nls.Id);

            this.Property(nls => nls.Email).IsRequired().HasMaxLength(255);
        }
    }
}