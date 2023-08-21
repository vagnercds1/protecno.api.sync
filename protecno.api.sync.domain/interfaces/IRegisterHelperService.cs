using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.enumerators;
using protecno.api.sync.domain.models.generics;
using protecno.api.sync.domain.models.inventory;
using System;

namespace protecno.api.sync.domain.interfaces
{
    public interface IRegisterHelperService
    {
        int GetRegisterId(int baseInventarioId, ERegisterType registerType, string code);

        Inventory FillAllRegisters(Inventory inventoryRQ);

        InsertResult InsertRegistersWhenNotExists(Inventory inventoryRQ, UserJwt userJwt, DateTime date, bool fillAllRegisters);
    }
}
