using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models.inventory;
using protecno.api.sync.domain.models.register;
using System;

namespace protecno.api.sync.domain.interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Inventory, InventoryPaginateRequest> InventoryRepository { get; }
         
        IRepository<Register, RegisterPaginateRequest> RegisterRepository { get; }

        IHistoryRepository HistoryRepository { get; }

        void BeginTransaction();
        void Commit();
        void Rollback();
    }
}
