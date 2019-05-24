using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Granite.Core;

namespace Granite.Infrastructure.Common.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly ISession _session;

        public Repository(ISession session)
        {
            _session = session;
        }

        public async Task<T> GetAsync(Guid id)
        {
            return await _session.GetAsync<T>(id);
        }

        public async Task<T> GetAsync(T entity)
        {
            return await _session.GetAsync<T>(entity);
        }

        public async Task<IEnumerable<T>> FindByAsync(
            Expression<Func<T, bool>> predicate)
        {
            return await _session.FindByAsync(predicate);
        }

        public async Task<IQueryable<T>> GetAllAsync()
        {
            return await _session.GetAllAsync<T>();
        }

        public async Task<T> AddAsync(T entity)
        {
            return await _session.CreateAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _session.AddRangeAsync(entities);
        }

        public async Task DeleteAsync(T entity)
        {
            await _session.DeleteAsync(entity);
        }

        public async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            await _session.DeleteManyAsync(entities);
        }

        public async Task DeleteByAsync(Expression<Func<T, bool>> predicate)
        {
            await _session.DeleteByAsync(predicate);
        }

        public async Task<T> ModifyAsync(T entity)
        {
            return await _session.ModifyAsync(entity);
        }
    }
}