using System;
using Granite.Core;
using Granite.Infrastructure.Common.Data.Orm;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Granite.Tests.Common.Data
{
    public class DatabaseFixture : IDisposable
    {
        public Context Context { get; }

        public DatabaseFixture(DataStore store)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");

            if (store == DataStore.Postgresql)
            {
                configurationBuilder
                    .AddUserSecrets("dd99af01-7c47-40c9-851d-5955e7fe6fcf");
            }

            IConfiguration configuration = configurationBuilder.Build();
            
            var contextOptionsBuilder = new DbContextOptionsBuilder<Context>();

            switch (store)
            {
                case DataStore.Postgresql:
                    contextOptionsBuilder.UseNpgsql(
                        configuration.GetConnectionString(
                            "PostgreSQLConnection"));
                    break;
                case DataStore.Sqlite:
                    contextOptionsBuilder.UseSqlite(
                        configuration.GetConnectionString("SQLiteConnection"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(store), store, null);
            }

            contextOptionsBuilder.EnableSensitiveDataLogging();

            Context = new Context(contextOptionsBuilder.Options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
        }
    }
}