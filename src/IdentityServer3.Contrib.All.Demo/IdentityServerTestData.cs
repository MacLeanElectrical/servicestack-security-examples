// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace IdentityServer3.Contrib.All.Demo
{
    using System.Collections.Generic;
    using System.Linq;
    using Core;
    using Core.Models;
    using EntityFramework;

    public class IdentityServerTestData
    {
        public static void SetUp(EntityFrameworkServiceOptions options)
        {
            using (var db = new ClientConfigurationDbContext(options.ConnectionString, options.Schema))
            {
                if (!db.Clients.Any())
                {
                    foreach (var c in Clients.Get())
                    {
                        var e = c.ToEntity();
                        db.Clients.Add(e);
                    }
                    db.SaveChanges();
                }
            }

            using (var db = new ScopeConfigurationDbContext(options.ConnectionString, options.Schema))
            {
                if (!db.Scopes.Any())
                {
                    foreach (var s in Scopes.Get())
                    {
                        var e = s.ToEntity();
                        db.Scopes.Add(e);
                    }
                    db.SaveChanges();
                }
            }
        }

        class Clients
        {
            public static List<Client> Get()
            {
                return new List<Client>
                {
                    new Client
                    {
                        ClientName = "ServiceStack.Vault.ClientSecrets.Demo",
                        ClientId = Program.ServiceId,
                        Enabled = true,

                        AccessTokenType = AccessTokenType.Jwt,

                        Flow = Flows.Hybrid,

                        AllowAccessToAllScopes = true,

                        RedirectUris = new List<string>
                        {
                            "http://localhost:5001/auth/IdentityServer"
                        },

                        RequireConsent = false
                    }
                };
            }
        }

        class Scopes
        {
            public static List<Scope> Get()
            {
                return new List<Scope>(StandardScopes.All)
                {
                    StandardScopes.OfflineAccess,
                    new Scope
                    {
                        Enabled = true,
                        Name = Program.ServiceId,
                        Type = ScopeType.Identity,
                        Claims = new List<ScopeClaim>
                        {
                            new ScopeClaim(Constants.ClaimTypes.Subject),
                            new ScopeClaim(Constants.ClaimTypes.PreferredUserName)
                        }
                    }
                };
            }
        }
    }
}
