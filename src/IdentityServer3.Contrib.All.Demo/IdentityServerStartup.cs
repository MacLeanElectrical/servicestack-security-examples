// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace IdentityServer3.Contrib.All.Demo
{
    using System;
    using System.Configuration;
    using Vault.CertificateStore;
    using Vault.CertificateStore.Options;
    using Core.Configuration;
    using EntityFramework;
    using Membership;
    using Microsoft.Owin.Hosting;
    using Owin;
    using Serilog;
    using Vault.ClientSecretStore;
    using Vault.ClientSecretStore.Interfaces;
    using Vault.ClientSecretStore.Options;

    class IdentityServerStartup
    {
        private const string IdentityServerDb = "IdentityServer";
        private const string MembershipDb = "Membership";

        // Vault Digital Certificate Settings
        private const string RoleName = "identity-server";
        private const string CommonName = "idsvr.test.com";        

        private const string IdentityServerUriInstance = "http://localhost:5000";

        private const string MembershipApplicationName = "Test";

        private static IDisposable _identityServerInstance;

        public static void Startup()
        {
            Log.Logger = new LoggerConfiguration()
               .WriteTo
               .LiterateConsole(outputTemplate: "{Timestamp:HH:mm} [{Level}] ({Name:l}){NewLine} {Message}{NewLine}{Exception}")
               .CreateLogger();

            if (_identityServerInstance == null)
            {
                _identityServerInstance = WebApp.Start<IdentityServerStartup>(IdentityServerUriInstance);
            }
        }

        public static void TearDown()
        {
            _identityServerInstance?.Dispose();
        }
        
        public void Configuration(IAppBuilder app)
        {
            var efConfig = new EntityFrameworkServiceOptions
            {
                ConnectionString = IdentityServerDb
            };

            var cleanup = new TokenCleanup(efConfig, 10);
            cleanup.Start();

            // Add in the Clients and Scopes to the EF database
            IdentityServerTestData.SetUp(efConfig);
            MembershipTestData.SetUp(MembershipDb, MembershipApplicationName);

            var factory = new IdentityServerServiceFactory();

            factory.RegisterOperationalServices(efConfig);

            factory.Register(new Registration<IClientConfigurationDbContext>(resolver => new ClientConfigurationDbContext(efConfig.ConnectionString)));
            factory.RegisterClientDataStore(new Registration<IClientDataStore>(resolver => new ClientDataStore(resolver.Resolve<IClientConfigurationDbContext>())));
            factory.CorsPolicyService = new ClientConfigurationCorsPolicyRegistration(efConfig);

            factory.Register(new Registration<IScopeConfigurationDbContext>(resolver => new ScopeConfigurationDbContext(efConfig.ConnectionString)));
            factory.RegisterScopeDataStore(new Registration<IScopeDataStore>(resolver => new ScopeDataStore(resolver.Resolve<IScopeConfigurationDbContext>())));

            factory.AddVaultClientSecretStore(
                new VaultClientSecretStoreAppIdOptions
                {
                    AppId = Program.IdentityServerAppId,
                    UserId = Program.IdentityServerUserId
                });

            factory.UseMembershipService(
                new MembershipOptions
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["Membership"].ConnectionString,
                    ApplicationName = MembershipApplicationName
                });

            var options = new IdentityServerOptions
            {
                Factory = factory,
                RequireSsl = false
            };

            // Wire up Vault as being the X509 Certificate Signing Store
            options.AddVaultCertificateStore(new VaultCertificateStoreAppIdOptions
            {
                AppId = Program.IdentityServerAppId,
                UserId = Program.IdentityServerUserId,

                RoleName = RoleName,
                CommonName = CommonName
            });

            app.UseIdentityServer(options);
        }
    }

    /// <summary>Wrapper around Entity Framework Data Store so that Vault is used for storing secrets</summary>
    class ClientDataStore : ClientStore, IClientDataStore
    {
        public ClientDataStore(IClientConfigurationDbContext context) 
            : base(context)
        {

        }
    }

    /// <summary>Wrapper around Entity Framework Data Store so that Vault is used for storing secrets</summary>
    class ScopeDataStore : ScopeStore, IScopeDataStore
    {
        public ScopeDataStore(IScopeConfigurationDbContext context) 
            : base(context)
        {
        }
    }
}
