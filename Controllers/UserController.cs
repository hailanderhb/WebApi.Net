using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Shop.Services;

namespace Shop.Controllers
{
    [Route("users")]
    public class UserController : Controller
    {
        [HttpGet]
        [Route("")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<User>>> Get([FromServices] DataContext context)
        {
            var users = await context.Users.AsNoTracking().ToListAsync();
            return users;

        }

        [HttpPost] //criar usuario
        [Route("")]
        [AllowAnonymous]
        //[Authorized(Roles="manager")] //para só o gerente criar
        public async Task<ActionResult<User>> Post([FromBody] User model, [FromServices] DataContext context)
        {
            if (!ModelState.IsValid) //verifica os dados
                return BadRequest(ModelState);

            try
            {
                model.Role = "employee"; //Força o usuário a ser sempre funcionário
                context.Users.Add(model);
                await context.SaveChangesAsync();

                model.Password = ""; //Escondendo a senha
                return model;
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível criar o usuário" });
            }
        }

        [HttpPost] //loggar usuario para criar token
        [Route("login")]
        public async Task<ActionResult<dynamic>> Authenticate([FromBody] User model, [FromServices] DataContext context)
        {
            var user = await context.Users.AsNoTracking().Where(x => x.Username == model.Username && x.Password == model.Password).FirstOrDefaultAsync();
            //pegando usuário já verificando o nome e a senha
            if (user == null)
                return NotFound(new { message = "Usuário ou senha inválido" });

            var token = TokenService.GenerateToken(user); //recebendo token gerado de services

            user.Password = ""; //esconde a senha

            return new //retornando o usuário e o token
            {
                user = user,
                token = token
            };
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "manager")]

        public async Task<ActionResult<User>> Put(int id, [FromBody] User model, [FromServices] DataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != model.Id)//verifica se o id informado é o mesmo do modelo
                return NotFound(new { message = "Usuário não encontrado" });

            try
            {
                context.Entry(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return model;
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível atualizar o usuário" });
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<User>>> Delete(int id, [FromServices] DataContext context)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id); //traz de volta a categoria para poder assim excluir.
            if (user == null) //caso não encontre a categoria que foi resgatada do banco, apresenta o erro a seguir
                return NotFound(new { message = "Usuário não encontrado" }); //action result permite utilizar notfound, ok e etc.

            try
            {
                context.Users.Remove(user); //contexto de categories, remove pra mim a categoria buscada
                await context.SaveChangesAsync(); //espera acontecer e salva
                return Ok(new { message = "Usuário removido com sucesso" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível remover o usuário" });
            }
        }

        // [HttpGet]
        // [Route("anonimo")]
        // [AllowAnonymous]
        // public string Anonimo() => "Anonimo";

        // [HttpGet]
        // [Route("autenticado")]
        // [Authorize] //só permite se estiver autenticado
        // public string Autenticado() => "Autenticado";

        // [HttpGet]
        // [Route("funcionario")]
        // [Authorize(Roles = "employee")]//permite acesso confome criação de usuario em users
        // public string Funcionario() => "Funcionario";

        // [HttpGet]
        // [Route("gerente")]
        // [Authorize(Roles = "manager")]
        // public string Gerente() => "Gerente";
    }
}