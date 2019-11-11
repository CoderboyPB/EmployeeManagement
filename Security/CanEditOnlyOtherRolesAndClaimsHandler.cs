using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmployeeManagement.Security
{
    public class CanEditOnlyOtherRolesAndClaimsHandler : AuthorizationHandler<ManageRolesAndClaimsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageRolesAndClaimsRequirement requirement)
        {
            var authFilterContext = context.Resource as AuthorizationFilterContext;
            if(authFilterContext == null)
            {
                return Task.CompletedTask;
            }

            string loggedInAdminId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value.ToLower();
            string adminIdBeingEdited = authFilterContext.HttpContext.Request.Query["userId"].ToString().ToLower();

            var user = context.User;

            if(user.IsInRole("Admin") && user.HasClaim("Edit Role","true") && loggedInAdminId != adminIdBeingEdited)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
