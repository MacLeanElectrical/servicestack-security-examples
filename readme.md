# IdentityServer3.Contrib.All.Demo

A demo project that authenticates a [ServiceStack](https://servicestack.net/) razor-based Client App using [IdentityServer](https://identityserver.github.io/) and [Vault](https://www.vaultproject.io/)  as the Client Secret Store
using Entity Framework for IdentityServer Client / Scope and an ASP.NET Membership Database for User data.

## Overview
This demo project bring in the various Identity Server and Service Stack plugins available in this Solution, namely:
* IdentityServer3.Contrib.Membership - An IdentityServer plugin that stores user data
* IdentityServer3.Contrib.ServiceStack - An IdentityServer plugin that supports impersonation authentication of a ServiceStack instance using IdentityServer
* IdentityServer3.Contrib.Vault.CertificateStore - An IdentityServer plugin that uses Vault to generate X509 Signing Certificates
* IdentityServer3.Contrib.Vault.ClientSecretStore - An IdentityServer plugin that uses Vault to store and authenticate client secrets
* ServiceStack.IdentityServerAuthProvider - A ServiceStack AuthProvider that authenticates a user against an IdentityServer instance
* ServiceStack.Vault.ClientSecretStore - A ServiceStack plugin that uses Vault to store and authenticate client secrets

When the project starts, you should be presented with a simple ServiceStack web app with a link that redirects to a secure service in ServiceStack. When you select the link you should be redirected to the IdentityServer instance that prompts you for login details.  Login using username "test@test.com" with password "password123".  You should then be redirected back to the ServiceStack web app and have access to the secure service (with Authenticate attribute) which displays the secure message.

### Prerequisites
* Create an empty SQL Server database called "IdentityServer" (update app.config with the correct connectionString).
* Create a SQL Server database called "Membership" using aspnet_regsql.exe (update app.config with the correct connectionString).  See below for instructions.
* Have an unitialised instance of Vault running locally on port 8200.  See below for instructions.

#### Creating an empty ASP.NET 2.0 Membership database
To create an empty ASP.NET 2.0 Membership database, run the following command:
    C:\Windows\Microsoft.NET\Framework\v2.0.50727\aspnet_regsql.exe

When the wizard opens, select next then "Configure SQL Server for application services" then next again. Select the Server instance on which the database will run and give the Database name "Membership" then continue.

#### Starting an instance of vault
Having downloaded and extracted the Vault.exe, start an unitialised instance of Vault, run the following vault command:
    vault.exe server -conf=vault.conf
        
Where vault.conf contains the following configuration:
<pre>
<code>
    listener "tcp" {
        address = "127.0.0.1:8200"
        tls_disable = 1
    }
</code>
</pre>