using QuGo.Core.Domain.Applications;

namespace QuGo.Data.Mapping.Stores
{
    public partial class ApplicationMappingMap : SysEntityTypeConfiguration<ApplicationMapping>
    {
        public ApplicationMappingMap()
        {
            this.ToTable("ApplicationMapping");
            this.HasKey(sm => sm.Id);

            this.Property(sm => sm.EntityName).IsRequired().HasMaxLength(400);

            this.HasRequired(sm => sm.Application)
                .WithMany()
                .HasForeignKey(sm => sm.ApplicationId)
                .WillCascadeOnDelete(true);
        }
    }
}