namespace FeedbackApp.Services.Interfaces;

/// <summary>
/// Generic Repository Pattern Interface
/// INTERVIEW CONCEPT: Repository Pattern + Generics
///
/// This interface demonstrates:
/// 1. Generic constraints (where T : class)
/// 2. Repository pattern for data abstraction
/// 3. Async/await pattern with Task
/// 4. Expression-based filtering (LINQ)
/// </summary>
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
    Task<int> CountAsync();
    Task<bool> ExistsAsync(Guid id);
}
