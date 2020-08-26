using System.Linq;
using System.Threading.Tasks;

namespace Shipbot.Data
{
    public interface IEntityRepository<T>
        where T: class
    {
        Task<T> Add(T item);

        IQueryable<T> Query();
        T Update(T item);
        Task Save();
    }
}