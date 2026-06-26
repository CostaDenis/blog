using Blog.Attributes;
using Microsoft.AspNetCore.Mvc;

//Health Check
// É como se fosse aquelas pagina de status do Xbox
namespace Blog.Properties.Controllers
{
    //Isso aqui são chamados de atributos
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet("")]
        // [ApiKey]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
