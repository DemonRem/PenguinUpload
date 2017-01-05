﻿using System.Linq;
using Nancy;
using Nancy.Security;
using PenguinUpload.Services.Authentication;
using PenguinUpload.Utilities;

namespace PenguinUpload.Modules
{
    public class AdminAccessModule : NancyModule
    {
        public AdminAccessModule() : base("/api/admin")
        {
            this.RequiresAuthentication();
            // Requires API key access
            this.RequiresClaims(x => x.Value == ApiClientAuthenticationService.StatelessAuthClaim.Value);

            Before += (ctx) =>
            {
                // Make sure user is an admin
                if (!PenguinUploadRegistry.Configuration.Administrators.Contains(Context.CurrentUser.Identity.Name))
                {
                    return HttpStatusCode.Unauthorized;
                }
                return null;
            };

            // List all users
            Get("/enumerateusers", async _ =>
            {
                var webUserManager = new WebUserManager();
                var allUsers = await webUserManager.GetAllUsersAsync();
                return Response.AsJsonNet(allUsers);
            });

            // Disable a user's account
            Post("/disableuser/{name}", async args =>
            {
                var userManager = new WebUserManager();
                var user = await userManager.FindUserByUsernameAsync((string) args.name);
                if (user == null) return HttpStatusCode.BadRequest;
                // Disable user
                await userManager.DisableUser(user);
                return HttpStatusCode.OK;
            });
        }
    }
}