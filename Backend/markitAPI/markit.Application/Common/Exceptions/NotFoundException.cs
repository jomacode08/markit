
namespace markit.Application.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string entity, int idEntity) : base($"{entity} - {idEntity} was not found")
        {}

        public NotFoundException(string entity, string idEntity) : base($"{entity} - {idEntity} was not found")
        {}
    }
}
