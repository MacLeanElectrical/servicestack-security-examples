// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace IdentityServer3.Contrib.All.Demo
{
    using System;
    using System.Diagnostics;
    using global::ServiceStack.Text;

    class Program
    {
        // Login for Vault using AppId Authentication Method
        public const string IdentityServerAppId = "146a3d05-2042-4855-93ba-1b122e70eb6d";
        public const string IdentityServerUserId = "976c1095-a7b4-4b6f-8cd8-d71d860c6a31";

        public const string ServiceId = "961ef5c4-73b8-4c2c-838d-80ce10c71c58";
        public const string ServiceAppId = "f8a5a40f-ecd9-43da-a009-82f180e1ef84";
        public const string ServiceUserId = "27ded1df-7aca-40ba-a825-cc9bf5cb7f88";

        static void Main(string[] args)
        {
            try
            {
                VaultStartup.Startup();

                IdentityServerStartup.Startup();

                // Now start up service stack client
                new AppHost("http://localhost:5001/").Init().Start("http://*:5001/");
                "ServiceStack Self Host with Razor listening at http://localhost:5001 ".Print();
                Process.Start("http://localhost:5001/");

                Console.ReadLine();
            }
            finally
            {
                IdentityServerStartup.TearDown();
            }
        }
    }
}
