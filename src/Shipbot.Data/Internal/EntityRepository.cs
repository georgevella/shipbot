using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Shipbot.Data
{
    public class EntityRepository<T> : IEntityRepository<T>
        where T : class
    {
        private readonly ShipbotDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;

        public EntityRepository(
            ShipbotDbContext dbContext, 
            IUnitOfWork unitOfWork
            )
        {
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;

            DbSet = dbContext.Set<T>();
        }

        public DbSet<T> DbSet { get; }

        public async Task<T> Add(T item)
        {
            var entity = await DbSet.AddAsync(item);
            return entity.Entity;
        }

        public ValueTask<T> Find<TKey>(TKey id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            // ReSharper disable once HeapView.PossibleBoxingAllocation
            return DbSet.FindAsync(new object[] { id }, CancellationToken.None);
        }

        public T Update(T item)
        {
            var entity = DbSet.Update(item);
            return entity.Entity;
        }

        public Task Save()
        {
            return _dbContext.SaveChangesAsync();
        }

        public IQueryable<T> Query()
        {
            return DbSet.AsQueryable();
        }
    }
}