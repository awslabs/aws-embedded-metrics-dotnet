using System;
using System.Threading.Tasks;

namespace Amazon.CloudWatch.EMF.Sink
{
    public interface ISocketClient : IDisposable
    {
        Task SendMessageAsync(string message);
    }
}