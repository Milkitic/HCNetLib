using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace HCNetStreamServer
{
    public class UserNameRequirement : IAuthorizationRequirement
    {
        public List<string> AllowUsers { get; set; } = new List<string> { "admin" };
    }
}