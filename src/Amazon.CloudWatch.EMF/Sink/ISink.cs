using System.Threading.Tasks;
using Amazon.CloudWatch.EMF.Model;

namespace Amazon.CloudWatch.EMF.Sink
{
    public interface ISink
    {
        public void Accept(MetricsContext context);

        public Task Shutdown();
    }
}