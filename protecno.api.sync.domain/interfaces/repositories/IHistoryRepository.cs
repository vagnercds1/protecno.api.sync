using protecno.api.sync.domain.entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace protecno.api.sync.domain.interfaces.repositories
{
    public interface IHistoryRepository
    {
        Task<List<History>> GetHistoryListAsync(History history);
 
        void Insert(History item);
    }
}
