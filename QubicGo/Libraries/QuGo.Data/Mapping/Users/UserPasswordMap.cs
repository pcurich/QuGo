using QuGo.Core.Domain.Users;

namespace QuGo.Data.Mapping.Users
{
    public partial class UserPasswordMap : SysEntityTypeConfiguration<UserPassword>
    {
        public UserPasswordMap()
        {
            this.ToTable("UserPassword");
            this.HasKey(password => password.Id);

            this.HasRequired(password => password.User)
                .WithMany()
                .HasForeignKey(password => password.UserId);

            this.Ignore(password => password.PasswordFormat);
        }
    }
}