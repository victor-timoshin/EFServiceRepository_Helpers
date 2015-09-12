using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EFServiceRepository_Helpers
{
	public interface IUnitOfWork : IDisposable
	{
		IGenericRepository<T> RepositoryFor<T>() where T : class;
		void Commit();
		Task<int> CommitAsync();
	}

	public class UnitOfWork : IUnitOfWork
	{
		//private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private DbContext context;
		private ObjectContext objectContext;
		private bool disposed;

		/// <summary>
		/// Конструктор класса
		/// </summary>
		/// <param name="context"></param>
		public UnitOfWork(DbContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			this.context = context;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IGenericRepository<T> RepositoryFor<T>() where T : class
		{
			return new GenericRepository<T>(this.context);
		}

		/// <summary>
		/// Сохраняет все обновления в источнике данных и сбрасывает отслеживание изменений в контексте объекта
		/// </summary>
		public void Commit()
		{
			bool saveFailed;
			do
			{
				saveFailed = false;

				try
				{
					this.context.SaveChanges();
				}
				catch (DbUpdateConcurrencyException exception)
				{
					//log.Debug("\n\n*** {0}\n\n", exception.InnerException);

					saveFailed = true;
					exception.Entries.Single().Reload();

					throw exception;
				}
			} while (saveFailed);
		}

		/// <summary>
		/// Сохраняет все обновления в источнике данных и сбрасывает отслеживание изменений в контексте объекта
		/// Асинхронный метод
		/// </summary>
		public Task<int> CommitAsync()
		{
			try
			{
				return this.context.SaveChangesAsync();
			}
			catch (DbUpdateException exception)
			{
				//log.Debug("\n\n*** {0}\n\n", exception.InnerException);
				throw exception;
			}
		}

		#region Implementation of IDisposable

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			if (this.context != null)
				this.context.Dispose();

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		public virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
			{
				try
				{
					if (objectContext != null && objectContext.Connection.State == ConnectionState.Open)
						objectContext.Connection.Close();
				}
				catch (ObjectDisposedException)
				{
				}

				//if (_dataContext != null)
				//{
				//    _dataContext.Dispose();
				//    _dataContext = null;
				//}
			}

			disposed = true;
		}

		#endregion
	}
}