using System;
using System.Threading.Tasks;

namespace EventNet.Sample.ReadModel.Repository
{
    public interface ITodoRepository
    {
        Task<TodoViewModel> GetAsync(Guid Id);
        Task SaveAsync(TodoViewModel model);
    }
}