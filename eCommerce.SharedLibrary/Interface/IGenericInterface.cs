using eCommerce.SharedLibrary.ResponseFolder;
using System.Linq.Expressions;

namespace eCommerce.SharedLibrary.Interface
{
    public interface IGenericInterface<T> where T : class
    {
        Task<Response> CreateAsync(T entity);
        Task<Response> UpadteAsync(T entity);
        Task<Response> DeleteAsync(T entity);
        Task<IEnumerable<T>> GetaAllAsync();
        Task<T>GetByIdAsync(int id);
        Task<T> GetByAsync(Expression<Func<T, bool>> predicate);
    }
}
