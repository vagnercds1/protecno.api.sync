using Amazon.S3;
using AutoMapper;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.helpers;
using protecno.api.sync.domain.interfaces;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models.inventory;
using protecno.api.sync.domain.models.register;
using protecno.api.sync.domain.models.report;
using protecno.api.sync.domain.services;
using protecno.api.sync.infrastructure.Factories;
using protecno.api.sync.infrastructure.repositories;
using Serilog;

namespace protecno.api.sync.infrastructure.IoC
{
    public static class DependencyResolver
    {
        public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<RegisterResult, Register>();
                cfg.CreateMap<Register, RegisterResult>();

                cfg.CreateMap<InventoryResult, Inventory>();
                cfg.CreateMap<Inventory, InventoryResult>();

                cfg.CreateMap<InventoryPaginateRequest, Inventory>();
                cfg.CreateMap<Inventory, InventoryPaginateRequest>();

                cfg.CreateMap<ValidationResult, RegisterValidationResult>();
                cfg.CreateMap<ValidationResult, InventoryValidationResult>();

                cfg.CreateMap<ReportRequest, RegisterPaginateRequest>();
                cfg.CreateMap<RegisterPaginateRequest, ReportRequest>();
            });

            IMapper mapper = config.CreateMapper();

            services.AddSingleton(mapper);

            Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.MySQL(connectionString: configuration.GetConnectionString("ManagerDb"))
                        .ReadFrom.AppSettings()
                        .CreateLogger();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<IReportCashService, ReportCashService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IRegisterService, RegisterService>();
            services.AddScoped<IRegisterHelperService, RegisterHelperService>();
            services.AddScoped<ICsvHelperService, CsvHelperService>();
            services.AddSingleton<IAmazonS3, AmazonS3Client>();
            services.AddSingleton<IS3Helper, S3Helper>();
            services.AddScoped<IReportGeneratorService, ReportGeneratorService>();
            services.AddScoped<IImportFileService, ImportFileService>();
            services.AddScoped<IImportCashService, ImportCashService>();            
            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICacheRepository, CacheRepository>();

            services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

            var serviceProvider = services.BuildServiceProvider();
            IDbConnectionFactory dbConnectionFactory = serviceProvider.GetService<IDbConnectionFactory>();

            var dbConnection = dbConnectionFactory.GetConnection("InventoryDb");

            services.AddScoped<ICountRepository, CountRepository>(provider =>
            {
                return new CountRepository(dbConnection, serviceProvider.GetService<ICacheRepository>());
            });

            services.AddScoped<IRepository<Register, RegisterPaginateRequest>>(provider =>
            {
                return new RegisterRepository(dbConnection);
            });

            services.AddScoped<IRepository<Inventory, InventoryPaginateRequest>>(provider =>
            {
                return new InventoryRepository(dbConnection);
            });

            services.AddScoped<IHistoryRepository, HistoryRepository>(provider =>
            {
                return new HistoryRepository(dbConnection);
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>(provider =>
            {
                return new UnitOfWork(dbConnection);
            });

            return services;
        }
    }
}
