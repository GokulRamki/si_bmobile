using System.Web.Mvc;

namespace Twinkle.Knockout
{
  public class KnockoutWithContext<TModel> : KnockoutCommonRegionContext<TModel>
  {
    public KnockoutWithContext(ViewContext viewContext, string expression) : base(viewContext, expression)
    {
    }

    protected override string Keyword
    {
      get
      {
        return "with";
      }
    }
  }
}