using System.Text.RegularExpressions;
using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SecureIdentity.Password;

namespace Blog.Controllers;

// [Authorize]  //Deixa o controller inteiro protegido
[ApiController]
public class AccountController : ControllerBase
//Dá para falar que essa controller DEPENDE de um Token Service
{
    //Criando uma dependencia
    public AccountController(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    private readonly TokenService _tokenService;

    //Como esta um o authorize, o User.Identitiy sempre irá ser preenchido
    // [Authorize(Roles = "user")]
    // [HttpGet("v1/user")]
    // public IActionResult GetUser()
    //     => Ok(User.Identity.Name);

    // [Authorize(Roles = "author")]
    // [HttpGet("v1/author")]
    // public IActionResult GetAuthor()
    //     => Ok(User.Identity.Name);

    // [Authorize(Roles = "admin")]
    // [HttpGet("v1/admin")]
    // public IActionResult GetAdmin()
    //     => Ok(User.Identity.Name);

    [HttpPost("v1/accounts/")]
    public async Task<IActionResult> Post(
        [FromBody] RegisterViewModel model,
        [FromServices] EmailService email,
        [FromServices] BlogDataContext context
        )
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Slug = model.Email.Replace("@", "-").Replace(".", "-")
        };
        //Gerando senha forte na aplicação

        //Armazenar senhas
        //Encryptar senha

        //Pacote do Balta de senha

        var password = PasswordGenerator.Generate(25);
        user.PasswordHash = PasswordHasher.Hash(password);

        try
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            email.Send(user.Name,
                        user.Email,
                        "Bem vindo ao Blog!",
                        $"Sua senha é {user.PasswordHash}");


            return Ok(new ResultViewModel<dynamic>(new
            {
                user = user.Email,
                password
            }));
        }
        catch (DbUpdateException)
        {
            return StatusCode(400, new ResultViewModel<string>("Esse email já está sendo utilizado na aplicação!"));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor!"));
        }

    }

    // [AllowAnonymous]    //Permite usar o endpoint sem estar autenticado
    [HttpPost("v1/accounts/login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginViewModel loginModel,
        [FromServices] BlogDataContext context,
        [FromServices] TokenService tokenService
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        //Na hora de buscar o user no banco, já adiciona os roles
        //A senha fica de fora por enquanto, pois a senha do model é a senha pura, e a senha do banco é hasheada
        var user = await context.Users
                            .AsNoTracking()
                            .Include(x => x.Roles)
                            .FirstOrDefaultAsync(x => x.Email == loginModel.Email);

        if (user == null)
            return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

        //PasswordHasher.Hash(senha) -> gera um novo hash

        if (!PasswordHasher.Verify(user.PasswordHash, loginModel.Password))
            return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

        try
        {
            var token = tokenService.GenerateToken(user);
            return Ok(new ResultViewModel<string>(token, null));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor!"));
        }

    }

    [Authorize]
    [HttpPost("v1/accounts/upload-image")]
    public async Task<IActionResult> UploadImage(
        [FromBody] UploadImageViewModel image,
        [FromServices] BlogDataContext context
    )
    {
        //Sendo guid, "sempre" gera um novo nome
        var fileName = $"{Guid.NewGuid().ToString()}.jpg";
        var data = new Regex(@"^data:image\/[a-z]+;base64,")
                                .Replace(image.Base64Image, "");
        var bytes = Convert.FromBase64String(data);


        try
        {
            await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor!"));
        }

        var user = await context
                            .Users
                            .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

        if (user == null)
            return NotFound(new ResultViewModel<Category>("Usuário não encontrado"));

        user.Image = $"https://localhost:0000/images/{fileName}";

        try
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor!"));
        }

        return Ok(new ResultViewModel<string>("Imagem alterada com sucesso!", null));

    }
}