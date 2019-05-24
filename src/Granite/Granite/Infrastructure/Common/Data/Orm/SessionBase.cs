using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Granite.Core;
using Microsoft.EntityFrameworkCore;

namespace Granite.Infrastructure.Common.Data.Orm
{
    public class SessionBase : ISession
    {
        public Context Context { get; }

        public SessionBase(Context context)
        {
            Context = context;
        }

        public SessionBase(
            DbContextOptionsBuilder<Context> contextOptionsBuilder)
        {
            Context = new Context(contextOptionsBuilder.Options);
            Context.Database.EnsureCreated();
        }
        private void AddTimestamps()
        {
            var addedEntries = Context.ChangeTracker
                .Entries<BaseEntity>().Where(x => x.State == EntityState.Added);

            foreach (var addedEntry in addedEntries)
            {
                addedEntry.Entity.CreatedUtc = DateTime.UtcNow;
            }

            var modifiedEntries =
                Context.ChangeTracker.Entries<BaseEntity>()
                    .Where(x => x.State == EntityState.Modified);

            foreach (var modifiedEntry in modifiedEntries)
            {
                modifiedEntry.Entity.ModifiedUtc = DateTime.UtcNow;
            }
        }

        public void Flush()
        {
            AddTimestamps();
            Context.SaveChanges();
        }

        public async Task FlushAsync()
        {
            AddTimestamps();
            await Context.SaveChangesAsync();
        }

        public async Task<T> GetAsync<T>(Guid id) where T : BaseEntity
        {
            return await Context.Set<T>()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<T> GetAsync<T>(T entity) where T : BaseEntity
        {
            return await Context.Set<T>()
                .FirstOrDefaultAsync(e => e.Id == entity.Id);
        }

        public async Task<IQueryable<T>> GetAllAsync<T>() where T : BaseEntity
        {
            var result = Context.Set<T>()
                .AsQueryable();

            return await Task.FromResult(result);
        }

        public async Task<T> CreateAsync<T>(T entity) where T : BaseEntity
        {
            var result = await Context.Set<T>()
                .AddAsync(entity);

            await FlushAsync();
            return result.Entity;
        }

        public async Task AddRangeAsync<T>(IEnumerable<T> entities)
            where T : BaseEntity
        {
            Context.Database.BeginTransaction();
            await Context.Set<T>().AddRangeAsync(entities);
            Context.Database.CommitTransaction();
            await FlushAsync();
        }

        public async Task<T> ModifyAsync<T>(T entity) where T : BaseEntity
        {
            var result = Context.Set<T>()
                .Update(entity);

            await FlushAsync();
            return result.Entity;
        }

        public async Task DeleteAsync<T>(Guid id) where T : BaseEntity
        {
            var entity = await Context.Set<T>()
                .FindAsync(id);
            Context.Set<T>().Remove(entity);

            await FlushAsync();
        }

        public async Task DeleteAsync<T>(T entity) where T : BaseEntity
        {
            await DeleteAsync<T>(entity.Id);
        }

        public async Task DeleteManyAsync<T>(IEnumerable<T> entites)
            where T : BaseEntity
        {
            Context.Database.BeginTransaction();
            Context.Set<T>().RemoveRange(entites);
            Context.Database.CommitTransaction();
            await FlushAsync();
        }

        public async Task DeleteByAsync<T>(
            Expression<Func<T, bool>> predicate) where T : BaseEntity
        {
            var entities = Context.Set<T>()
                .Where(predicate);

            await DeleteManyAsync(entities);
        }

        public async Task<IEnumerable<T>> FindByAsync<T>(
            Expression<Func<T, bool>> predicate)
            where T : BaseEntity
        {
            var result = Context.Set<T>()
                .Where(predicate)
                .AsEnumerable();

            return await Task.FromResult(result);
        }
    }
}