using System.Collections.Generic;

namespace protecno.api.sync.domain.models.generics
{
    public class ProcessSave<T>
    {
        public ProcessSave()
        {
            ListFoundItens = new List<T>();
            ListNewItens = new List<T>();
            SaveListRS = new SaveListResult();
        }

        public List<T> ListFoundItens { get; set; }
        
        public List<T> ListNewItens { get; set; }
       
        public SaveListResult SaveListRS { get; set; }
    }
}
