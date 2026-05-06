namespace WebApiMapas.Repositories.Interfaces
{
    /// <summary>
    /// Interface que define os métodos para acessar e manipular dados 
    /// de localização georreferenciada.
    /// </summary>
    public interface ILocalizacaoRepository
    {
        Task<List<Localizacao>> GetAll();   
        Task<Localizacao> GetById(int id);
        Task Add(Localizacao localizacao);
        Task Update(Localizacao localizacao);
        Task Delete(int id);
    }
}
