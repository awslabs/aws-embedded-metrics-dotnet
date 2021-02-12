using System.Threading.Tasks;

namespace Amazon.CloudWatch.EMF.Sink
{
    public interface ISocketClient
    {
        Task SendMessageAsync(string message);
    }
}