using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{

    //Endpoinit = > URL
    //https://localhost:5001/categories

    [Route("v1/categories")]
    public class CategoryController : ControllerBase
    {

        //https://localhost:5001/categories
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        [ResponseCache(VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any, Duration = 30)] //cacheando com duração
        //[ResponseCache(Duration =0, Location =ResponseCacheLocation.None, NoStore = true)]  //Pra dizer que o metodo não tem cache
        public async Task<ActionResult<List<Category>>> Get([FromServices] DataContext context)
        {
            var categories = await context.Categories.AsNoTracking().ToListAsync(); //Asnotracking faz uma leitura mais rapida sem trazer o proxy
            return Ok(categories);
        }

        [HttpGet]
        [Route("{id:int}")] //indicando rota de 1 item
        [AllowAnonymous]
        public async Task<ActionResult<Category>> GetByID(
            int id,
            [FromServices] DataContext context
            )
        {
            var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return Ok(category);
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<List<Category>>> Post(
            [FromBody] Category model,
            [FromServices] DataContext context) //Devemos utilizar o category criado no Model para receber do jeito certo o json, deve-se capturar o json da requisição evitando assim passagem por parametro
        {
            if (!ModelState.IsValid) //modelstate traz o estado como um todo para validar (inclusive os erros)
                return BadRequest(ModelState);

            try
            {
                context.Categories.Add(model); //utiliza dbset e adiciona o model que é uma categoria
                await context.SaveChangesAsync(); //por usar o async, tem que usar o await para persistir os dados em memória, gerando id e preenchendo no model (utiliza o execute scalar do sqlserver)
                return Ok(model);
            }
            catch
            {
                return BadRequest(new { message = "Não foi possivel criar categoria" });
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<List<Category>>> Put(
            int id,
            [FromBody] Category model,
            [FromServices] DataContext context)
        {
            if (id != model.Id) //comparando se o que está no corpo da requisição é igual ao que está na url como parametro
                return NotFound(new { message = "Categoria não encontrada" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);  //retorna a mensagem de erro dada no model category
            try
            {
                context.Entry<Category>(model).State = EntityState.Modified; //entry com base no modelo, verifica o que foi alterado
                await context.SaveChangesAsync(); //salva a alteração em memória
                return Ok(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "Este registro já foi atualizado" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível atualizar a categoria" });
            }

        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<List<Category>>> Delete(
            int id,
            [FromServices] DataContext context
        )
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id); //traz de volta a categoria para poder assim excluir.
            if (category == null) //caso não encontre a categoria que foi resgatada do banco, apresenta o erro a seguir
                return NotFound(new { message = "Categoria não encontrada" }); //action result permite utilizar notfound, ok e etc.

            try
            {
                context.Categories.Remove(category); //contexto de categories, remove pra mim a categoria buscada
                await context.SaveChangesAsync(); //espera acontecer e salva
                return Ok(new { message = "Categoria removida com sucesso" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível remover a categoria" });
            }
        }
    }
}