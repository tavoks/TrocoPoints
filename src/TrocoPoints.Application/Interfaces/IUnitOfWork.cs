using System.Data;

namespace TrocoPoints.Application.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }

        Task IniciarTransacaoAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
