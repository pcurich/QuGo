using QuGo.Core.Domain.Tasks;

namespace QuGo.Data.Mapping.Tasks
{
    public partial class ScheduleTaskMap : SysEntityTypeConfiguration<ScheduleTask>
    {
        public ScheduleTaskMap()
        {
            this.ToTable("ScheduleTask");
            this.HasKey(t => t.Id);
            this.Property(t => t.Name).IsRequired();
            this.Property(t => t.Type).IsRequired();
        }
    }
}