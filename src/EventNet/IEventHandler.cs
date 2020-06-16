using System.Threading.Tasks;

namespace EventNet.Core
{
    public interface IEventHandler<in T>
    {
        public void  HandleAsync(T @event);
    }
}