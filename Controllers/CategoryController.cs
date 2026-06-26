using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Properties.Controllers
{
    [ApiController]
    public class CategoryController : ControllerBase
    {
        [HttpGet("v1/categories")]
        public IActionResult Get([FromServices] BlogDataContext context)
        {
            try
            {
                var categories = context.Categories.ToList();
                return Ok(new ResultViewModel<List<Category>>(categories));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Não foi possível ler categoria");
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("Falha interna no servidor"));
            }
        }


        [HttpGet("v2/categories")]
        public async Task<IActionResult> GetAsync(
            [FromServices] BlogDataContext context,
            [FromServices] IMemoryCache cache
        )
        {
            try
            {
                var categories = cache.GetOrCreate("CategoriesCache", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return GetCategories(context);
                });
                // var categories = await context.Categories.ToListAsync();
                return Ok(new ResultViewModel<List<Category>>(categories));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Não foi possível ler categoria");
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("Falha interna no servidor"));
            }
        }

        //Métodos assincronos nunca retornam objetos concretos, eles retornam uma task
        //Não é legal um método assincrono retornar uma task
        //Com async, é possível processar em paralelo em quanto a aplicação faz outras coisas
        //Para métodos async, é recomendável tudo dentro dele ser async tbm

        private List<Category> GetCategories(BlogDataContext context)
        {
            return context.Categories.ToList();
        }

        //Toda vez que passar um parametro, dá para já definir seu tipo
        [HttpGet("v2/categories/{id:int}")]
        public async Task<IActionResult> GetByIdAsync([FromServices] BlogDataContext context,
            [FromRoute] int id)
        {
            try
            {
                var categorie = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

                if (categorie == null)
                    return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));


                return Ok(new ResultViewModel<Category>(categorie));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Não foi possível ler categoria");
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("Falha interna no servidor"));
            }

        }


        [HttpPost("v2/categories/")]
        public async Task<IActionResult> PostAsync([FromServices] BlogDataContext context,
            [FromBody] EditorCategoryViewModel model)
        {

            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));


            try
            {
                var category = new Category
                {
                    Id = 0,
                    Name = model.Name,
                    Slug = model.Slug.ToLower()
                };
                await context.Categories.AddAsync(category);
                await context.SaveChangesAsync();

                return Created($"v2/categories/{category.Id}", new ResultViewModel<Category>(category));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Não foi possível incluir categoria");
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("Falha interna no servidor"));
            }
        }


        [HttpPut("v2/categories/{id:int}")]
        public async Task<IActionResult> PutAsync([FromServices] BlogDataContext context,
            [FromRoute] int id, [FromBody] EditorCategoryViewModel model)
        {
            try
            {
                var categorie = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

                if (categorie == null)
                    return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

                categorie.Name = model.Name;
                categorie.Slug = model.Slug;

                //O update não tem async
                context.Categories.Update(categorie);
                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Category>(categorie));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Não foi possível alterar categoria");
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("Falha interna no servidor"));
            }
        }


        [HttpDelete("v2/categories/{id:int}")]
        public async Task<IActionResult> DeleteAsync([FromServices] BlogDataContext context,
            [FromRoute] int id)
        {
            try
            {
                var categorie = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

                if (categorie == null)
                    return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

                //O remove não tem async
                context.Categories.Remove(categorie);
                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Category>(categorie));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Não foi possível excluir categoria");
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("Falha interna no servidor"));
            }
        }
    }
}
