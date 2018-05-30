using System;
using QuGo.Core.Domain.Users;
using QuGo.Services.Tasks;

namespace QuGo.Services.Users
{
    /// <summary>
    /// Represents a task for deleting guest users
    /// </summary>
    public partial class DeleteGuestsTask : ITask
    {
        private readonly IUserService _userService;
        private readonly UserSettings _userSettings;

        public DeleteGuestsTask(IUserService userService, UserSettings userSettings)
        {
            this._userService = userService;
            this._userSettings = userSettings;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            var olderThanMinutes = _userSettings.DeleteGuestTaskOlderThanMinutes;
            // Default value in case 0 is returned.  0 would effectively disable this service and harm performance.
            olderThanMinutes = olderThanMinutes == 0 ? 1440 : olderThanMinutes;
    
            _userService.DeleteGuestUsers(null, DateTime.UtcNow.AddMinutes(-olderThanMinutes), true);
        }
    }
}
