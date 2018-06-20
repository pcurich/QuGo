using QuGo.Core.Domain.Media;

namespace QuGo.Data.Mapping.Media
{
    public partial class DownloadMap : SysEntityTypeConfiguration<Download>
    {
        public DownloadMap()
        {
            this.ToTable("Download");
            this.HasKey(p => p.Id);
            this.Property(p => p.DownloadBinary).IsMaxLength();
        }
    }
}