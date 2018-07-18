namespace OneBank.AccountsBot
{
    using System.Web.Http;
    using Autofac;
    using Autofac.Integration.WebApi;
    using Common;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Owin;

    /// <summary>
    /// Defines the <see cref="Startup" />
    /// </summary>
    public static class Startup
    {
        /// <summary>
        /// The ConfigureApp
        /// </summary>
        /// <param name="appBuilder">The <see cref="IAppBuilder" /></param>
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();

            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            config.Filters.Add(new HandleExceptionAttribute());
            appBuilder.UseWebApi(config);

            Conversation.UpdateContainer(
                builder =>
                {
                    builder.Register(c => new ServiceFabricBotDataStore("Accounts"))
                                .As<IBotDataStore<BotData>>().InstancePerLifetimeScope();
                });

            config.DependencyResolver = new AutofacWebApiDependencyResolver(Conversation.Container);

            config.Filters.Add(new BotAuthentication() { MicrosoftAppId = "a8fe8368-9518-4fec-9717-fdbc156febcc", MicrosoftAppPassword = "mtwyCDP267{[$wcfLEKC92(" });
            var microsoftAppCredentials = Conversation.Container.Resolve<MicrosoftAppCredentials>();
            microsoftAppCredentials.MicrosoftAppId = "a8fe8368-9518-4fec-9717-fdbc156febcc";
            microsoftAppCredentials.MicrosoftAppPassword = "mtwyCDP267{[$wcfLEKC92(";
        }
    }
}
