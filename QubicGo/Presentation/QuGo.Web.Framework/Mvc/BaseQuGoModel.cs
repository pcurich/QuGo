using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuGo.Web.Framework.Mvc
{
    /// <summary>
    /// Base application model
    /// </summary>
    [ModelBinder(typeof(QuGoModelBinder))]
    public class BaseQuGoModel
    {
        public BaseQuGoModel()
        {
            this.UserProperties = new Dictionary<string, object>();
            PostInitialize();
        }

        public virtual void BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
        }

        /// <summary>
        /// Developers can override this method in custom partial classes
        /// in order to add some custom initialization code to constructors
        /// </summary>
        protected virtual void PostInitialize()
        {

        }

        /// <summary>
        /// Use this property to store any custom value for your models. 
        /// </summary>
        public Dictionary<string, object> UserProperties { get; set; }
    }

    /// <summary>
    /// Base application entity model
    /// </summary>
    public partial class BaseNopEntityModel : BaseQuGoModel
    {
        public virtual int Id { get; set; }
    }


}