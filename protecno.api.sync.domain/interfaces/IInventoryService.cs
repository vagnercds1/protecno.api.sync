using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.models.generics;
using protecno.api.sync.domain.models.inventory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace protecno.api.sync.domain.interfaces
{
    public interface IInventoryService
    {
        Task<ContainerResult<Inventory>> GetPagintateAsync(InventoryPaginateRequest inventoryRQ, int userId);

        SaveListResult SaveInventoryList(List<Inventory> listInventory, UserJwt userJWT, DateTime saveDate);

        InventoryValidationResult InsertInventory(Inventory inventoryRQ, UserJwt userJWT, DateTime saveDate, bool insertRegistersWhenNotExists, bool fillAllRegisters);

        InventoryValidationResult UpdateInventory(Inventory originalInventory, Inventory updateInventory, UserJwt userJWT, DateTime saveDate, bool insertRegistersWhenNotExists, bool fillAllRegisters);

        InventoryValidationResult RemoveInventory(InventoryDeleteRequest inventoryDeleteRequest, UserJwt userJwt, DateTime saveDate);
    }
}
