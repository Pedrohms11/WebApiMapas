using ApiWebMapas.Data;
using Microsoft.EntityFrameworkCore;
using WebApiMapas.Models;
using WebApiMapas.Repositories.Interfaces;

namespace WebApiMapas.Repositories
{
    public class LocalizacaoRepository : ILocalizacaoRepository
    {
        /// <summary>
        /// Implementação concreta do repositório de localizações georreferenciadas.
        /// </summary>
        
        private readonly AppDbContext _context;

        public LocalizacaoRepository(AppDbContext context)
        {
            _context = context;
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="localizacao"></param>
       /// <returns></returns>
        public async Task Add(Localizacao localizacao)
        {
            _context.Localizacoes.Add(localizacao);
            await _context.SaveChangesAsync();
        }

    }
}
