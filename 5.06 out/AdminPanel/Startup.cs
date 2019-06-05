using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using React.AspNet;
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;

namespace AdminPanel
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddReact();
            services.AddJsEngineSwitcher(options => options.DefaultEngineName = ChakraCoreJsEngine.EngineName)
  .AddChakraCore();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseReact(config => { });
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}