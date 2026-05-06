namespace WebApiMapas.Controllers
{
    public class MapasController
    {
        /// <summary>
        ///  Delete Api para deletar um georeferenciamento existente, caso o id não exista retorna NotFound, caso exista deleta e retorna NoContent
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActioResult> Delete(int id)
        {
            var existente = await _service.ObterPorId(id);
            if (existente == null)
                return NotFound();

            await _service.Deletar(id);
            return NoContent();
        }
    }
}
