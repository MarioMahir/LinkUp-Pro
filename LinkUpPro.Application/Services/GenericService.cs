using LinkUpPro.Application.Interfaces;
using System.Linq.Expressions;

namespace LinkUpPro.Application.Services
{
    public class GenericService<T> : IGenericService<T> where T : class
    {
        private readonly IGenericRepository<T> _repository;

        public GenericService(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        public Task<T?> GetByIdAsync(object id) => _repository.GetByIdAsync(id);

        public Task<List<T>> GetAllAsync() => _repository.GetAllAsync();

        public Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
            _repository.FindAsync(predicate);

        public async Task AddAsync(T entity)
        {
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _repository.Update(entity);
            await _repository.SaveChangesAsync();
        }

        public async Task RemoveAsync(T entity)
        {
            _repository.Remove(entity);
            await _repository.SaveChangesAsync();
        }
    }
}
