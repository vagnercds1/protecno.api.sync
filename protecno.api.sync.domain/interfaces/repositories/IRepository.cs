using Dapper;
using protecno.api.sync.domain.models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace protecno.api.sync.domain.interfaces.repositories
{
    public interface IRepository<E, RQ> where E : class
                                        where RQ : class
    {
        Task<List<E>> GetPagintateAsync(RQ filterRQ, string sqlFilter, DynamicParameters dbArgs);

        Task<List<E>> GetListAsync(RQ filterRQ);

        List<E> GetList(RQ filterRQ);

        int Insert(E entity);

        void Update(E register);

        int Delete(E entity);
    }
}
