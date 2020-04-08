using System.Threading.Tasks;

namespace EventNet.Core
{
    public interface IEventHandler<T>
    {
        public void  HandleAsync(T @event);
    }
}