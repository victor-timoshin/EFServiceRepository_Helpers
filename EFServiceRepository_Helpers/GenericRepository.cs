using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EFServiceRepository_Helpers
{
	public interface IGenericRepository<T>
	{
		IQueryable<T> GetAll();
		Task<List<T>> GetAllAsync();
		IQueryable<T> Find(Expression<Func<T, bool>> predicate);
		Task<List<T>> FindAsync(Expression<Func<T, bool>> match);
		T Single(Expression<Func<T, bool>> predicate);
		Task<T> SingleAsync(Expression<Func<T, bool>> predicate);
		T First(Expression<Func<T, bool>> predicate);
		Task<T> FirstAsync(Expression<Func<T, bool>> predicate);
		void Remove(T entity);
		void Add(T entity);
		void Update(T entity);
		Task<int> CountAsync();
	}

	public class GenericRepository<T> : IGenericRepository<T> where T : class
	{
		private DbContext databaseContext;
		private DbSet<T> entitySet;

		/// <summary>
		/// Конструктор класса
		/// </summary>
		/// <param name="context"></param>
		public GenericRepository(DbContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			databaseContext = context;
			entitySet = context.Set<T>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IQueryable<T> GetAll()
		{
			return entitySet.AsQueryable();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public async Task<List<T>> GetAllAsync()
		{
			return await entitySet.ToListAsync();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
		{
			return entitySet.Where(predicate);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public async Task<List<T>> FindAsync(Expression<Func<T, bool>> match)
		{
			return await entitySet.Where(match).ToListAsync();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public T Single(Expression<Func<T, bool>> predicate)
		{
			return entitySet.SingleOrDefault(predicate);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public async Task<T> SingleAsync(Expression<Func<T, bool>> predicate)
		{
			return await entitySet.SingleOrDefaultAsync(predicate);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public T First(Expression<Func<T, bool>> predicate)
		{
			return entitySet.FirstOrDefault(predicate);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public async Task<T> FirstAsync(Expression<Func<T, bool>> predicate)
		{
			return await entitySet.FirstOrDefaultAsync(predicate);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="entity"></param>
		public void Add(T entity)
		{
			DbEntityEntry entry = databaseContext.Entry(entity);
			if (entry.State == EntityState.Detached)
				entitySet.Add(entity);
			else
				entry.State = EntityState.Added;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="entity"></param>
		public void Remove(T entity)
		{
			DbEntityEntry entry = databaseContext.Entry(entity);
			if (entry.State == EntityState.Deleted)
			{
				entitySet.Attach(entity);
				entitySet.Remove(entity);
			}
			else
				entry.State = EntityState.Deleted;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="entity"></param>
		public void Update(T entity)
		{
			try
			{
				DbEntityEntry entry = databaseContext.Entry(entity);
				if (entry.State == EntityState.Detached)
					entitySet.Attach(entity);

				entry.State = EntityState.Modified;
			}
			catch (DbUpdateConcurrencyException exception)
			{
				DbEntityEntry entry = exception.Entries.Single();
				var clientValues = (T)entry.Entity;
				var databaseValues = (T)entry.GetDatabaseValues().ToObject();

				throw exception;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public async Task<int> CountAsync()
		{
			return await entitySet.CountAsync();
		}
	}
}