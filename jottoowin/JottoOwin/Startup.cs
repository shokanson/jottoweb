using Hokanson.JottoRepository;
using Microsoft.Owin;
using Nancy;
using Nancy.Conventions;
using Newtonsoft.Json.Serialization;
using Ninject;
using Owin;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;

namespace JottoOwin
{
    using Services;
    using System.IO;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use<LoggerMiddleware>();
//            app.Use<NoCachingMiddleware>();

            app.MapSignalR();
            app.ConfigAndUseWebApi();

            // to use in OwinHost, need to use this bootstrapper--still allows working under IIS(Express)
            app.UseNancy(options => options.Bootstrapper = new ApplicationBootstrapper());

            // will work just fine under IISExpress but not IIS or OwinHost
            //app.UseNancy();
        }
    }

    internal static class MyWebApiExtensions
    {
        public static void ConfigAndUseWebApi(this IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.DependencyResolver = new NinjectResolver(CreateAndConfigureKernel());

            app.UseWebApi(config);
        }

        private static IKernel CreateAndConfigureKernel()
        {
            var kernel = new StandardKernel();

            // add other bindings here as necessary
            kernel.Bind<IJottoRepository>().To<JottoRepository>();

            kernel.Bind<IWordList>()
                  .To<FileWordList>()
                  .InSingletonScope()
                  .WithConstructorArgument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fiveletterwords.lst"));

            return kernel;
        }
    }

    internal class ApplicationBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("scripts", "Scripts"));
        }
    }

    internal class LoggerMiddleware : OwinMiddleware
    {
        public LoggerMiddleware(OwinMiddleware next)
            : base(next)
        { }

        public async override Task Invoke(IOwinContext context)
        {
            var start = DateTime.Now;
            await Next.Invoke(context);
            var elapsed = DateTime.Now - start;
            await Task.Run(() => Trace.TraceInformation("[{0}] {1} {2}\t{3}\t{4}",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                elapsed.TotalMilliseconds,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode));
        }
    }

    internal class NoCachingMiddleware : OwinMiddleware
    {
        public NoCachingMiddleware(OwinMiddleware next)
            : base(next)
        { }

        public async override Task Invoke(IOwinContext context)
        {
            await Next.Invoke(context);

            if (context.Request.Method == "GET" && context.Request.Path.ToString().EndsWith("helps"))
            {
                context.Response.Headers.Add("Cache-Control", new [] { "no-cache", "no-store", "must-revalidate" });
            }
        }
    }
}