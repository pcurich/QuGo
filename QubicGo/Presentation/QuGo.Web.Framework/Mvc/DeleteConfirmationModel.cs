namespace QuGo.Web.Framework.Mvc
{
    public class DeleteConfirmationModel : BaseQuGoEntityModel
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string WindowId { get; set; }
    }
}