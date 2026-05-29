namespace BankOperations.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entity, Guid id)
        : base($"{entity} with id {id} was not found.") { }

    public NotFoundException(string entity, string identifier)
        : base($"{entity} '{identifier}' was not found.") { }
}
