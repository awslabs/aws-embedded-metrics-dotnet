namespace Amazon.CloudWatch.EMF.Environment
{
    using Amazon.CloudWatch.EMF.Model;
    using Amazon.CloudWatch.EMF.Sink;

    /// <summary>
    /// A runtime environment (e.g. Lambda, EKS, ECS, EC2).
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        ///  Determines whether or not we are executing in this environment.
        /// </summary>
        /// <returns>True if it is running in this environment, otherwise, False</returns>
        bool Probe();

        /// <summary>
        /// Name of the environment. This will be used to set the ServiceName dimension.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///  Environment type. This will be used to set the ServiceType dimension.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Log group name. This will be used to set the LogGroup dimension.
        /// </summary>
        string LogGroupName { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context">Context to configure with environment properties </param>
        void ConfigureContext(MetricsContext context);

        /// <summary>
        /// Appropriate sink for this environment
        /// </summary>
        ISink Sink { get; }
    }
}
