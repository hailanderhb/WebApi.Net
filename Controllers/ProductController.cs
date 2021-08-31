using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Route("products")]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> Get([FromServices] DataContext context)
        {
            var products = await context.Products.Include(x => x.Category).AsNoTracking().ToListAsync(); //include vai dar um join no sql server, porque isso ?! por que no model o product possui uma categoria do tipo categoria
            return products;
        }

        [HttpGet]
        [Route("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetById(int id, [FromServices] DataContext context)
        {
            var product = await context.Products.Include(x => x.Category).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            //variavel produto recebe um await do context products que é incluido em categoria, lido de forma rapida e trazendo o primeiro ou unico referente ao Id solicitado
            return product;
        }

        [HttpGet]
        [Route("categories/{id:int}")] //products/categories/1  para listar todos os produtos que são da categoria 1 ou conforme ID
        public async Task<ActionResult<List<Product>>> GetByCategory(int id, [FromServices] DataContext context)
        {
            var products = await context.Products.Include(x => x.Category).AsNoTracking().Where(x => x.CategoryId == id).ToListAsync();
            //variavel produtos recebe um await list do context products que é incluido em categoria, lido de forma rapida e somente trazendo quem tem o categoryId igual ao Id solicitado
            return products;
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Product>> Post([FromBody] Product model, [FromServices] DataContext context)
        {
            if (ModelState.IsValid) //se a forma colocada no body condiz com o modelo, então faça
            {
                context.Products.Add(model);
                await context.SaveChangesAsync();
                return model;
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "employee")]

        public async Task<ActionResult<List<Product>>> Put(int id, [FromBody] Product model, [FromServices] DataContext context)
        {
            if (id != model.Id) //Se o id requisitado não for igual ao id armazenado retorna
                return NotFound(new { message = "Produto não encontrada" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            //Verifica requisitos do model e se não for valido retorna badrequest
            try
            {
                context.Entry<Product>(model).State = EntityState.Modified; //Entra em modelProduct e altera o estado/modifica
                await context.SaveChangesAsync(); //salva as alterações conforme o body em json
                return Ok(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "Este registro já foi atualizado" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível atualizar o produto" });
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "employee")]

        public async Task<ActionResult<List<Product>>> Delete(int id, [FromServices] DataContext context)
        {
            var product = await context.Products.FirstOrDefaultAsync(x => x.Id == id); //busca um produto com o determinado Id para ser excluido
            if (product == null)
                return NotFound(new { message = "Produto não encontrado" });

            try
            {
                context.Products.Remove(product);
                await context.SaveChangesAsync();
                return Ok(new { message = "Produto removido com sucesso" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possivel remover o produto" });
            }

        }

    }
}