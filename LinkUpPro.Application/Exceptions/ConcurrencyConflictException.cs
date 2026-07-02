namespace LinkUpPro.Application.Exceptions
{
    // Se lanza desde la capa de infraestructura cuando una restricción única
    // de la base de datos impide guardar un cambio por una operación
    // concurrente equivalente. Permite a los servicios de Application
    // reaccionar sin depender de tipos de Entity Framework Core.
    public class ConcurrencyConflictException : Exception
    {
        public ConcurrencyConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
