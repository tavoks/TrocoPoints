using System.Data;
using TrocoPoints.Application.Interfaces;

namespace TrocoPoints.Infrastructure.Persistence
{
    public abstract class RepositoryBase
    {
        private readonly IUnitOfWork _unitOfWork;

        protected RepositoryBase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected IDbConnection Connection => _unitOfWork.Connection;
        protected IDbTransaction Transaction => _unitOfWork.Transaction;
    }
}
