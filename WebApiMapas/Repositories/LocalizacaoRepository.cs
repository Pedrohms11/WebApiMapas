using Microsoft.EntityFrameworkCore;
using WebApiMapas.Data;
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

        public async Task<Localizacao> GetById(int id)
        {
            var localizacao = await _context.Localizacoes.FindAsync(id);

            if (localizacao == null)
            {
                throw new KeyNotFoundException($"Localização com ID {id} não encontrada.");
            }

            return localizacao;
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

        public async Task Update(Localizacao localizacao)
        {
            var existing = await _context.Localizacoes.FindAsync(localizacao.Id);

            if (existing != null)
            {
                _context.Localizacoes.Update(localizacao);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"Localização com ID {localizacao.Id} não encontrada.");
            }
        }

        public async Task Delete(int id)
        {
            var localizacao = await _context.Localizacoes.FindAsync(id);
            if (localizacao != null)
            {
                _context.Localizacoes.Remove(localizacao);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"Localização com ID {id} não encontrada.");
            }

        }
    }
