using System.Data.Entity.ModelConfiguration;

namespace QuGo.Data.Mapping
{
    public abstract class SysEntityTypeConfiguration<T> : EntityTypeConfiguration<T> where T : class
    {
        protected SysEntityTypeConfiguration()
        {
            PostInitialize();
        }

        /// <summary>
        /// Developers can override this method in custom partial classes
        /// in order to add some custom initialization code to constructors
        /// </summary>
        protected virtual void PostInitialize()
        {
            
        }
    }
}