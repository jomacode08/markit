
namespace markit.Application.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string entity, int idEntity) : base($"Registro {entity}: {idEntity} no fue encontrado.")
        {
        }
    }
}
