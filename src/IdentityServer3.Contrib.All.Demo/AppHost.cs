// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace IdentityServer3.Contrib.All.Demo
{
    using System.IO;
    using Funq;
    using global::ServiceStack;
    using global::ServiceStack.Razor;
    using ServiceStack.Authentication.IdentityServer.Extensions;
    using ServiceStack.Authentication.IdentityServer.Vault;

    /// <summary>Service Stack App Host</summary>
    class AppHost : AppSelfHostBase
    {
        private readonly string serviceUrl;

        public AppHost(string serviceUrl)
            : base(Program.ServiceId, typeof (AppHost).Assembly)
        {
            this.serviceUrl = serviceUrl;
        }

        public override void Configure(Container container)
        {
            this.Plugins.Add(new RazorFormat());
            SetConfig(new HostConfig
            {
#if DEBUG
                DebugMode = true,
                WebHostPhysicalPath = Path.GetFullPath(Path.Combine("~".MapServerPath(), "..", "..")),
#endif
                WebHostUrl = serviceUrl
            });

            // Vault AppId Authentication Settings
            AppSettings.Set(IdentityServerVaultAuthFeature.VaultAppIdAppSetting, Program.ServiceAppId);
            AppSettings.Set(IdentityServerVaultAuthFeature.VaultUserIdAppSetting, Program.ServiceUserId);

            // The key to use to encrypt the secrets
            AppSettings.Set(IdentityServerVaultAuthFeature.VaultEncryptionKeyAppSetting, Program.ServiceAppId);

            AppSettings.SetUserAuthProvider()
                       .SetAuthRealm("http://localhost:5000/")
                       .SetClientId(Program.ServiceId)
                       .SetScopes($"openid profile {Program.ServiceId} email offline_access");

            Plugins.Add(new IdentityServerVaultAuthFeature());
        }
    }
}
