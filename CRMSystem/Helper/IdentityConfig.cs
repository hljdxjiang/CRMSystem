using IdentityServer4.Models;
using System.Collections.Generic;

namespace CRMSystem.Helper
{
    public static class IdentityConfig
    {
            public static IEnumerable<ApiResource> Apis
                => new List<ApiResource>
            {
            new ApiResource("api1","My API")
            };

            public static IEnumerable<Client> Clients =>
                new List<Client>
                {
                new Client
                {
                    ClientId="client",
                    AllowedGrantTypes =GrantTypes.ClientCredentials,
                    ClientSecrets={
                    new Secret("aju".Sha256())
                    },
                    AllowedScopes={ "api1"}
                }
                };
        }
    }
