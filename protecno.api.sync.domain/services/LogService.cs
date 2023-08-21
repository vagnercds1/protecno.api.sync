using protecno.api.sync.domain.models.generics;
using Serilog;
using System;
using System.Reflection;

namespace protecno.api.sync.domain.services
{
    public interface ILogService
    {
        void WhriteErro(Exception ex, string message, UserJwt userJwt);

        void WhriteInformation(string message, UserJwt userJwt);
    }

    public class LogService: ILogService
    {
        private readonly string _applicationName;
        public LogService()
        {
            _applicationName = Assembly.GetEntryAssembly()?.GetName().Name;
        }

        public void WhriteErro(Exception ex, string message, UserJwt userJwt)
        {
            Log.Logger.ForContext("ApplicationName", _applicationName)
                    .ForContext("ClientId", userJwt.CustomerId)
                    .ForContext("BaseInventarioId", userJwt.BaseInventoryId)
                    .ForContext("Usuario", userJwt.Email)
                    .Error(ex, message);
        }

        public void WhriteInformation(string message, UserJwt userJwt)
        {
            Log.Logger.ForContext("ApplicationName", _applicationName)
                    .ForContext("CustomerId", userJwt.CustomerId)
                    .ForContext("BaseInventarioId", userJwt.BaseInventoryId)
                    .ForContext("Usuario", userJwt.Email)
                    .Information( message);
        }
    }
}
