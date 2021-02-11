using System;

namespace Amazon.CloudWatch.EMF.Environment
{
    public interface IResourceFetcher
    {
        public T Fetch<T>(Uri endpoint);
    }
}
