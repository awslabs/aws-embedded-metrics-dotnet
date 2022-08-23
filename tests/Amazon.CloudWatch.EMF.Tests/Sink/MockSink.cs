using System.Threading.Tasks;
using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Sink;

namespace Amazon.CloudWatch.EMF.Tests.Sink
{
    public class MockSink : ISink
    {
        private MetricsContext _context;
        public void Accept(MetricsContext context)
        {
            _context = context;
        }

        public Task Shutdown()
        {
            return Task.CompletedTask;
        }

        public MetricsContext MetricsContext => _context;
    }
}