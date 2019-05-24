using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Granite.Core;
using Granite.Infrastructure.Common.Data.Orm;
using Microsoft.EntityFrameworkCore;

namespace Granite.Infrastructure.Common.Data
{
    public interface ISession
    {
        Context Context { get; }

        Task<T> GetAsync<T>(Guid id) where T : BaseEntity;
        Task<T> GetAsync<T>(T entity) where T : BaseEntity;
        Task<IQueryable<T>> GetAllAsync<T>() where T : BaseEntity;
        Task<T> CreateAsync<T>(T entity) where T : BaseEntity;
        Task AddRangeAsync<T>(IEnumerable<T> entities) where T : BaseEntity;
        Task<T> ModifyAsync<T>(T entity) where T : BaseEntity;
        Task DeleteAsync<T>(Guid id) where T : BaseEntity;
        Task DeleteAsync<T>(T entity) where T : BaseEntity;
        Task DeleteManyAsync<T>(IEnumerable<T> entities) where T : BaseEntity;

        Task DeleteByAsync<T>(Expression<Func<T, bool>> predicate)
            where T : BaseEntity;

        Task<IEnumerable<T>> FindByAsync<T>(Expression<Func<T, bool>> predicate)
            where T : BaseEntity;
    }
}