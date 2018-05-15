using QuGo.Core.Domain.Common;

namespace QuGo.Data.Mapping.Common
{
    public partial class AddressMap : SysEntityTypeConfiguration<Address>
    {
        public AddressMap()
        {
            this.ToTable("Address");
            this.HasKey(a => a.Id);

            this.HasOptional(a => a.Country)
                .WithMany()
                .HasForeignKey(a => a.CountryId).WillCascadeOnDelete(false);

            this.HasOptional(a => a.StateProvince)
                .WithMany()
                .HasForeignKey(a => a.StateProvinceId).WillCascadeOnDelete(false);
        }
    }
}
