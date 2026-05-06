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

        public async Task<List<Localizacao>> GetAll()
            => await _context.Localizacoes.ToListAsync();

        /// <summary>
        /// GetById - Responsável por retornar uma localização específica 
        /// com base no ID fornecido. Ele utiliza o método FindAsync do 
        /// Entity Framework para buscar a localização no banco de dados.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Localizacao> GetById(int id)
            => await _context.Localizacoes.FindAsync(id);

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
