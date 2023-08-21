using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.models.generics;
using protecno.api.sync.domain.models.register;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace protecno.api.sync.domain.interfaces
{
    public interface IRegisterService
    {
        SaveListResult SaveRegisterList(List<Register> listRegisters, UserJwt userJwt, DateTime saveDate);

        RegisterValidationResult InsertRegister(Register register, UserJwt userJwt, DateTime saveDate);

        RegisterValidationResult UpdateRegister(Register originalRegister, Register updateRegister, UserJwt userJwt, DateTime saveDate);

        RegisterValidationResult RemoveRegister(RegisterDeleteRequest registerDeleteRequest, UserJwt userJwt, DateTime saveDate);

        Task<ContainerResult<Register>> GetPagintateAsync(RegisterPaginateRequest registerRequest, int userId);
    }
}
