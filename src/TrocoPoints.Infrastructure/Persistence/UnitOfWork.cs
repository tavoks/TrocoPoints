using System.Data;
using Oracle.ManagedDataAccess.Client;
using TrocoPoints.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace TrocoPoints.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly string _connectionString;
        private OracleConnection? _connection;
        private IDbTransaction? _transaction;

        public UnitOfWork(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleConnectionString")
                ?? throw new InvalidOperationException("String de conexão não encontrada.");
        }

        public IDbConnection Connection => _connection == null ? throw
            new InvalidOperationException("Conexão não iniciada.") : _connection;

        public IDbTransaction Transaction => _transaction == null ? throw
            new InvalidOperationException("Transação não iniciada.") : _transaction;

        public async Task IniciarTransacaoAsync()
        {
            if (_connection is not null)
                throw new InvalidOperationException("Conexão já iniciada.");

            _connection = new OracleConnection(_connectionString);
            await _connection.OpenAsync();
            _transaction = _connection.BeginTransaction();
        }

        public async Task CommitAsync()
        {
            if (_transaction is null)
                throw new InvalidOperationException("Transação não iniciada.");

            _transaction.Commit();
            await DisposeAsync();
        }

        public async Task RollbackAsync()
        {
            if (_transaction is null)
                throw new InvalidOperationException("Transação não iniciada.");

            _transaction.Rollback();
            await DisposeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            _transaction?.Dispose();
            _transaction = null;

            if (_connection is not null)
                await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
