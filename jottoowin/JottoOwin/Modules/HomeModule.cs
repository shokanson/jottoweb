using Nancy;

namespace JottoOwin.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = _ =>
                {
                    var model = new { title = "Jotto!" };
                    return View["home", model];
                };
        }
    }
}