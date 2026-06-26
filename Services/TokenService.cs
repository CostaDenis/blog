using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog.Extensions;
using Blog.Models;
using Microsoft.IdentityModel.Tokens;

namespace Blog.Services;

public class TokenService
{
    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(Configuration.JwtKey); //Precisa ser um array de bytes, por isso tem o encoding
        var claims = user.GetClaims();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // Subject = new ClaimsIdentity(new[]
            // {
            //     //Chave e valor
            //    new Claim(ClaimTypes.Name, "denis"),  //User.Identity.Name
            //    new Claim(ClaimTypes.Role, "admin"),
            //    new Claim(ClaimTypes.Role, "user")   //User.IsInRole
            // }),

            Subject = new ClaimsIdentity(claims),   //User.IsInRole

            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)

        }; //De fato ele que fica com as informações do token
        var token = tokenHandler.CreateToken(tokenDescriptor);  //Cria o token

        return tokenHandler.WriteToken(token);  //Write Token gera uma string baseada no token
    }
}