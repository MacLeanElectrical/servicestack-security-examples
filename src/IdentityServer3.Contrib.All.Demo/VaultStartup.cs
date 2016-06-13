// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace IdentityServer3.Contrib.All.Demo
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using global::ServiceStack;
    using global::ServiceStack.Text;

    class VaultStartup
    {
        private const string VaultUriInstance = "http://127.0.0.1:8200";

        public static void Startup()
        {
            // 1. Initialize vault.
            string rootToken;
            string[] keys;
            Console.WriteLine($"Initializing vault at {VaultUriInstance}");
            Initialize(out rootToken, out keys);

            // 2. Unseal vault
            Console.WriteLine($"Unsealing vault at {VaultUriInstance}");
            Unseal(keys);

            Thread.Sleep(1000);

            // 3. Create transit end-point for encryption / decryption keys
            Console.WriteLine("Mount transit backend to create vault encryption keys");
            MountTransit(rootToken);
            Console.WriteLine("Create encryption token for encrypting/decrypting secrets");
            CreateEncryptionKey(rootToken, Program.ServiceId);

            // 3.a Create PKI end-point for certificatey "stuff"
            MountPki(rootToken);
            MountTunePki(rootToken);
            GenerateRootCertificate(rootToken, "test.com", "87600h");
            SetCertificateUrlConfiguration(rootToken);
            GetCertificateUrlConfiguration(rootToken);
            GenerateCertificateRole(rootToken, "identity-server", "test.com");

            // 4. Create list of client secrets for the micro-service
            CreateSecrets(rootToken, Program.ServiceId, new[] { "secret1", "secret2", "secret3", "secret4", "secret5" });

            // 5. Create app-id and user-id for the client that only have access to the secret end point
            EnableAppId(rootToken);

            // Create Identity Server app-id/user-id credentials
            CreateAppId(rootToken, Program.IdentityServerAppId, "root");
            CreateUserId(rootToken, Program.IdentityServerUserId);
            MapUserIdsToAppIds(rootToken, Program.IdentityServerUserId, Program.IdentityServerAppId);

            // Create Service app-id/user-id credentials
            CreateAppId(rootToken, Program.ServiceAppId, "root");
            CreateUserId(rootToken, Program.ServiceUserId);
            MapUserIdsToAppIds(rootToken, Program.ServiceUserId, Program.ServiceAppId);
        }

        public static void Initialize(out string rootToken, out string[] keys)
        {
            using (var client = new JsonServiceClient(VaultUriInstance))
            {
                var response = client.Put<JsonObject>("v1/sys/init", new { secret_shares = 5, secret_threshold = 3 });

                rootToken = response["root_token"];
                keys = response["keys"].FromJson<string[]>();
            }
        }

        public static void Unseal(string[] keys)
        {
            using (var client = new JsonServiceClient(VaultUriInstance))
            {
                for (var i = 0; i < keys.Length; i++)
                {
                    var response = client.Put<JsonObject>("v1/sys/unseal", new { key = keys[i] });
                    if (response["sealed"] == "false")
                    {
                        break;
                    }
                }
            }
        }

        public static void MountTransit(string rootToken)
        {
            using (var client = new JsonServiceClient(VaultUriInstance))
            {
                client.AddHeader("X-Vault-Token", rootToken);
                client.Post<JsonObject>("v1/sys/mounts/transit", new { type = "transit" });
            }
        }

        public static void MountPki(string rootToken)
        {
            using (var client = new JsonServiceClient(VaultUriInstance))
            {
                client.AddHeader("X-Vault-Token", rootToken);
                client.Post<JsonObject>("v1/sys/mounts/pki", new { type = "pki" });
            }
        }

        public static void MountTunePki(string rootToken)
        {
            using (var client = new JsonServiceClient(VaultUriInstance))
            {
                client.AddHeader("X-Vault-Token", rootToken);
                client.Post<JsonObject>("v1/sys/mounts/pki/tune", new { max_lease_ttl = "87600h" });
            }
        }

        public static void GenerateRootCertificate(string rootToken, string cn, string ttl)
        {
            using (var client = new JsonServiceClient(VaultUriInstance))
            {
                client.AddHeader("X-Vault-Token", rootToken);
                var result = client.Post<JsonObject>("v1/pki/root/generate/internal", new { common_name = cn, ttl });
            }
        }

        public static void SetCertificateUrlConfiguration(string rootToken)
        {
            using (var client = new JsonServiceClient(VaultUriInstance))
            {
                client.AddHeader("X-Vault-Token", rootToken);
                client.Post<JsonObject>("v1/pki/config/urls",
                    new
                    {
                        issuing_certificates = $"{VaultUriInstance}/v1/pki/ca",
                        crl_distribution_points = $"{VaultUriInstance}/v1/pki/crl"
                    });
            }
        }

        public static void GetCertificateUrlConfiguration(string rootToken)
        {
            using (var client = new JsonServiceClient(VaultUriInstance))
            {
                client.AddHeader("X-Vault-Token", rootToken);
                var response = client.Get<JsonObject>("v1/pki/config/urls");
            }
        }

        public static void GenerateCertificateRole(string rootToken, string roleName, string domains)
        {
            using (var client = new JsonServiceClient(VaultUriInstance))
            {
                client.AddHeader("X-Vault-Token", rootToken);
                client.Post<JsonObject>($"v1/pki/roles/{roleName}", new
                {
                    allowed_domains = domains,
                    allow_subdomains = true,
                    max_ttl = "72h"
                });
            }
        }

        public static void CreateEncryptionKey(string rootToken, string encryptionKey)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(VaultUriInstance) })
            {
                client.DefaultRequestHeaders.Add("X-Vault-Token", rootToken);
                var response = client.PostAsync($"/v1/transit/keys/{encryptionKey}", null).Result;
            }
        }

        public static void CreateSecrets(string rootToken, string secretName, string[] secrets)
        {
            using (var client = new JsonServiceClient(VaultUriInstance))
            {
                client.AddHeader("X-Vault-Token", rootToken);
                client.Post<JsonObject>($"v1/secret/{secretName}", new
                {
                    value = Encoding.UTF8.GetBytes(secrets.ToJson())
                });
            }
        }

        public static void EnableAppId(string rootToken)
        {
            using (var client = new JsonServiceClient(VaultUriInstance))
            {
                client.AddHeader("X-Vault-Token", rootToken);
                client.Post<JsonObject>("v1/sys/auth/app-id", new { type = "app-id" });
            }
        }

        public static void CreatePolicy(string rootToken, string name, string path, string policy)
        {
            using (var client = new JsonServiceClient(VaultUriInstance))
            {
                client.AddHeader("X-Vault-Token", rootToken);
                client.Put<string>($"v1/sys/policy/{name}", new { rules = CreateRule(path, policy).ToJson() });
            }
        }

        private static JsonObject CreateRule(string path, string policy)
        {
            return new JsonObject
            {
                ["path"] = new JsonObject
                {
                    [path] = new JsonObject
                    {
                        ["policy"] = policy
                    }.ToJson()
                }.ToJson()
            };
        }

        public static void CreateAppId(string rootToken, string appId, string policy)
        {
            using (var client = new JsonServiceClient(VaultUriInstance))
            {
                client.AddHeader("X-Vault-Token", rootToken);

                client.Put<string>($"v1/auth/app-id/map/app-id/{appId}", new { value = policy });
            }
        }

        public static void CreateUserId(string rootToken, string userId)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(VaultUriInstance) })
            {
                client.DefaultRequestHeaders.Add("X-Vault-Token", rootToken);
                var response = client.PutAsync($"v1/auth/app-id/map/user-id/{userId}", null).Result;
            }
        }

        public static void MapUserIdsToAppIds(string rootToken, string userId, params string[] appIds)
        {
            if (appIds == null || appIds.Length == 0)
                throw new Exception("user-id needs to be associated with at least 1 app-id");

            using (var client = new JsonServiceClient(VaultUriInstance))
            {
                client.AddHeader("X-Vault-Token", rootToken);
                client.Post<JsonObject>($"v1/auth/app-id/map/user-id/{userId}", new { value = appIds.Join(",") });
            }
        }
    }
}
