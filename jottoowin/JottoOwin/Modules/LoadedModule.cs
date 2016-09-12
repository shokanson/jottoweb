using Nancy;
using System;
using System.Linq;

namespace JottoOwin.Modules
{
    public class LoadedModule : NancyModule
    {
        public LoadedModule()
        {
            Get["/loaded"] = _ =>
            {
                var model = new
                {
                    title = "Loaded Assemblies",
                    assemblies = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                 orderby assembly.FullName
                                 select assembly.FullName
                };
                return View["loaded", model];
            };
        }
    }
}