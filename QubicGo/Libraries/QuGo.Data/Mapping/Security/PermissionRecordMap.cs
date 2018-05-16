using QuGo.Core.Domain.Security;

namespace QuGo.Data.Mapping.Security
{
    public partial class PermissionRecordMap : SysEntityTypeConfiguration<PermissionRecord>
    {
        public PermissionRecordMap()
        {
            this.ToTable("PermissionRecord");
            this.HasKey(pr => pr.Id);
            this.Property(pr => pr.Name).IsRequired();
            this.Property(pr => pr.SystemName).IsRequired().HasMaxLength(255);
            this.Property(pr => pr.Category).IsRequired().HasMaxLength(255);

            this.HasMany(pr => pr.UserRoles)
                .WithMany(cr => cr.PermissionRecords)
                .Map(m => m.ToTable("PermissionRecord_Role_Mapping"));
        }
    }
}