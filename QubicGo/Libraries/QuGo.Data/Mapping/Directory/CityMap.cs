using QuGo.Core.Domain.Directory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuGo.Data.Mapping.Directory
{
    public partial class CityMap : SysEntityTypeConfiguration<City>
    {
        public CityMap()
        {
            this.ToTable("City");
            this.HasKey(sp => sp.Id);
            this.Property(sp => sp.Name).IsRequired().HasMaxLength(100);
            this.Property(sp => sp.Abbreviation).HasMaxLength(100);


            this.HasRequired(sp => sp.StateProvince)
                .WithMany(c => c.Cities)
                .HasForeignKey(sp => sp.StateProvinceId);
        }
    }
}
