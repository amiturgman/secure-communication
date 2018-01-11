using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecuredCommunication;

namespace ilanatest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
        }

        public IConfiguration Configuration { get; }
        public KeyVault KV;
        public AzureQueueImpl securedComm;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton<IQueueManager, AzureQueueImpl>((serviceProvider) =>
            {
                KV = new KeyVault(Configuration["AzureKeyVaultUri"],
                    Configuration["applicationId"], Configuration["applicationSecret"]);
                var encryptionKeyName = Configuration["EncryptionKeyName"];
                var decryptionKeyName = Configuration["DecryptionKeyName"];
                var signKeyName = Configuration["SignKeyName"];
                var verifyKeyName = Configuration["VerifyKeyName"];

                var secretsMgmnt = new KeyVaultSecretManager(encryptionKeyName, decryptionKeyName, signKeyName, verifyKeyName, KV, KV);
                secretsMgmnt.Initialize().Wait();
                //var securedComm = new RabbitMQBusImpl(config["rabbitMqUri"], secretsMgmnt, true, "securedCommExchange");
                var queueClient = new CloudQueueClientWrapper(Configuration["AzureStorageConnectionString"]);
                securedComm = new AzureQueueImpl("ilanatest", queueClient, secretsMgmnt, true);
                securedComm.Initialize().Wait();
                securedComm.DequeueAsync(msg =>
                {
                    Console.WriteLine(msg);
                }, TimeSpan.FromSeconds(1));
                return securedComm;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
