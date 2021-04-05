import * as cdk from '@aws-cdk/core';
import * as ecr from '@aws-cdk/aws-ecr';
import * as cw from '@aws-cdk/aws-cloudwatch';
import { Pipeline } from './pipeline';
import { BuildProjects } from './build-projects';



export class CIStack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);
    this.createPipeline();
    this.createCanaryResources();
    this.createDashboard();
  }

  createPipeline() {
    new Pipeline(this, new BuildProjects(this));
  }

  createCanaryResources() {
    new ecr.Repository(this, 'emf-dotnet-canary', {
      repositoryName: 'emf-dotnet-canary'
    });
  }

  createDashboard() {
    new cw.Dashboard(this, 'Dashboard', {
      dashboardName: 'emf-dotnet-deployment',
      widgets: [
        [
          new cw.GraphWidget({
            title: 'Memory',
            width: 12,
            left: [
              new cw.MathExpression({
                expression: `SEARCH('{Canary,Runtime,Platform,Agent,Version}  MetricName="Memory.RSS" AND "Dotnet"', 'Average', 60)`,
                usingMetrics: {}
              })
            ],
            right: [
              new cw.Metric({
                namespace: 'AWS/ECS',
                metricName: 'CPUUtilization',
                dimensions: {
                  ServiceName: 'emf-dotnet-canary',
                  ClusterName: 'emf-canary'
                },
                color: '#c7c7c7'
              })
            ]
          }),
          new cw.GraphWidget({
            title: 'Init',
            width: 12,
            left: [
              new cw.MathExpression({
                expression: `SEARCH('{Canary,Runtime,Platform,Agent,Version} MetricName="Init" AND "Dotnet"', 'Sum', 60)`,
                usingMetrics: {}
              })
            ],
          }),
        ],
        [
          new cw.LogQueryWidget({
            title: 'Application Errors',
            width: 12,
            logGroupNames: ['/ecs/emf-dotnet-canary'],
            queryString: `
            filter @logStream ~= 'canary'
            | fields @timestamp, @message
            | sort @timestamp desc 
            | limit 20`,
          }),
          new cw.GraphWidget({
            title: 'Event Count',
            width: 12,
            left: [
              new cw.MathExpression({
                expression: `SEARCH('{Canary,Runtime,Platform,Agent,Version} MetricName="Invoke" AND "Dotnet"', 'SampleCount', 60)`,
                usingMetrics: {}
              })
            ],
          }),
        ],
        [
          new cw.LogQueryWidget({
            title: 'Recent EMF Data',
            width: 24,
            logGroupNames: ['/Canary/Dotnet/CloudWatchAgent/Metrics'],
            queryString: `
            fields @timestamp, @message
            | sort @timestamp desc
            | limit 20`,
          }),
        ]
      ]
    });
  }
}
