using protecno.api.sync.domain.entities;
using System;
using System.Collections.Generic;

namespace protecno.api.sync.domain.extensions
{
    public static class HistoricExtentions
    { 
        public static List<History> BuildHistoric<T>(this T _object, T originalRegister, T putRegister, int registerId, int origenId, int userId,string registerType, List<string> fields ) where T : class
        { 
            var historicList = new List<History>(); 
          
            Type property = putRegister.GetType();

            DateTime date = DateTime.Now;
            string transactionId = Guid.NewGuid().ToString();

            foreach (string prop in fields)
            { 
                 var valueNew = property.GetProperty(prop).GetValue(putRegister)!=null ? property.GetProperty(prop).GetValue(putRegister):"";
                 var valueOriginal = property.GetProperty(prop).GetValue(originalRegister) !=null ? property.GetProperty(prop).GetValue(originalRegister):"";
                 
                if (valueNew.ToString() != valueOriginal.ToString())
                {
                    historicList.Add(new History()
                    {
                        CampoAlterado = prop,
                        De = valueOriginal.ToString(),
                        Para = valueNew.ToString(),                      
                        RegistroId = registerId,
                        OrigemId = origenId,
                        TipoRegistro = registerType,
                        UsuarioId = userId,
                        DataAtualizacao = date,
                        TransactionId = transactionId
                    });
                }
            }
             
            return historicList;
        }
    }
}
