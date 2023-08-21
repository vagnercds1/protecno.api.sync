using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.interfaces;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models.inventory;
using protecno.api.sync.domain.models.register;
using protecno.api.sync.infrastructure.repositories;
using System.Data;

namespace protecno.api.sync.domain.helpers
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;
        private IRepository<Inventory, InventoryPaginateRequest> _inventoryRepository;
        private IRepository<Register, RegisterPaginateRequest> _registerRepository;
        private IHistoryRepository _historyRepository;

        public UnitOfWork(IDbConnection connection)
        {
            _connection = connection;
        }

        public IRepository<Inventory, InventoryPaginateRequest> InventoryRepository
        {
            get { return _inventoryRepository ??= new InventoryRepository(_connection); }
        }

        public IRepository<Register, RegisterPaginateRequest> RegisterRepository
        {
            get { return _registerRepository ??= new RegisterRepository(_connection); }
        }

        public IHistoryRepository HistoryRepository
        {
            get { return _historyRepository ??= new HistoryRepository(_connection); }
        }

        public void BeginTransaction()
        {
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            if (_transaction == null)
                _transaction = _connection.BeginTransaction();
        }

        public void Commit()
        {
            _transaction.Commit();              
        }

        public void Rollback()
        {
            _transaction.Rollback();             
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _transaction = null;
            _connection.Close();
            _connection.Dispose();
        }
    }
}
