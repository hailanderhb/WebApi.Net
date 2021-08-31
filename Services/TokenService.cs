using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Shop.Models;
using Microsoft.IdentityModel.Tokens;

namespace Shop.Services
{
    public static class TokenService
    {
        public static string GenerateToken(User user)//sempre que chamar o generatetoken vai criar um token com tempo de expiração
        {
            var tokenHandler = new JwtSecurityTokenHandler(); //responsavel por gerar o token
            var key = Encoding.ASCII.GetBytes(Settings.Secret); //precisamos da chave criada em setting.cs
            var tokenDescriptor = new SecurityTokenDescriptor //token descriptor é a descrição do que vai ter dentro do token
            {
                Subject = new ClaimsIdentity(new Claim[]{
                    new Claim(ClaimTypes.Name, user.Username.ToString()), //se declarar os claims aqui, ficarão disponiveis para inspeção
                    new Claim(ClaimTypes.Role, user.Role.ToString()) //não deixar informação sensiveis, pois pode ser verificado no jwt.io
                }),
                Expires = DateTime.UtcNow.AddHours(2),//colocando horas para expirar o token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor); //pega o token na variavel com os parametros de token descriptor
            return tokenHandler.WriteToken(token); //retorna a string do token
        }
    }
}