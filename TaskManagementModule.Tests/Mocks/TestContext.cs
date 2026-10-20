using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using TaskManagementModule.Data;
using TaskManagementModule.Mapping;
using TaskManagementModule.Services;

namespace TaskManagementModule.Tests.Mocks
{
    public sealed class TestContext : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        private TestContext(TaskManagementDbContext dbContext, IMapper mapper, ServiceProvider serviceProvider)
        {
            DbContext = dbContext;
            Service = new TaskManagementService(dbContext, mapper);
            _serviceProvider = serviceProvider;
        }

        public TaskManagementDbContext DbContext { get; }
        public ITaskManagementService Service { get; }

        public static TestContext Create()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(_ => { }, typeof(TaskManagementMappingProfile).Assembly);
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var dbContext = new TaskManagementDbContext(options);
            dbContext.Database.EnsureCreated();

            return new TestContext(dbContext, mapper, serviceProvider);
        }

        public void Dispose()
        {
            DbContext.Dispose();
            _serviceProvider.Dispose();
        }
    }
}
