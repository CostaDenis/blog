using System.Security.Claims;
using Blog.Models;

namespace Blog.Extensions
{
    //Classes de extensão tem que ser static
    public static class RoleClaimsExtension
    {
        public static IEnumerable<Claim> GetClaims(this User user)
        {
            var result = new List<Claim>
            {
                new(ClaimTypes.Name, user.Email)
            };

            //Pega todos os roles de user e retorna uma Claim
            result.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Slug)));

            return result;
        }
    }
}