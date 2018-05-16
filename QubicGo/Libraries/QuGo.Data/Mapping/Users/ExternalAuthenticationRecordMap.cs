using QuGo.Core.Domain.Users;

namespace QuGo.Data.Mapping.Users
{
    public partial class ExternalAuthenticationRecordMap : SysEntityTypeConfiguration<ExternalAuthenticationRecord>
    {
        public ExternalAuthenticationRecordMap()
        {
            this.ToTable("ExternalAuthenticationRecord");

            this.HasKey(ear => ear.Id);

            this.HasRequired(ear => ear.User)
                .WithMany(c => c.ExternalAuthenticationRecords)
                .HasForeignKey(ear => ear.UserId);

        }
    }
}