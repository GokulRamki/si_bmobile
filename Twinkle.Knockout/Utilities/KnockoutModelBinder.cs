using System.Web.Mvc;

namespace Twinkle.Knockout
{
  public class KnockoutModelBinder : DefaultModelBinder
  {
    public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
    {
      var result = base.BindModel(controllerContext, bindingContext);
      KnockoutUtilities.ConvertData(result);
      return result;
    }
  }
}