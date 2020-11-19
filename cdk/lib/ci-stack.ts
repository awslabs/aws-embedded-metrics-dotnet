import * as cdk from '@aws-cdk/core';
import { BuildSpec, Source, Project, PipelineProject, LinuxBuildImage, ComputeType } from '@aws-cdk/aws-codebuild';
import { PolicyStatement } from '@aws-cdk/aws-iam';
import * as codepipeline from '@aws-cdk/aws-codepipeline';
import * as codepipeline_actions from '@aws-cdk/aws-codepipeline-actions';
import * as ecr from '@aws-cdk/aws-ecr';
import * as cw from '@aws-cdk/aws-cloudwatch';

const environment = {
  // grants sudo permissions so we can use docker
  privileged: true,
  // AL2 3.0 build spec which supports .net 3.1
  // https://github.com/aws/aws-codebuild-docker-images/blob/407e02949dd24cd14a0db830c03639f34ff46ced/al2/x86_64/standard/3.0/Dockerfile#L406-L412
  buildImage: LinuxBuildImage.AMAZON_LINUX_2_3
};

export class CIStack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);
    this.createPipeline();
    this.createCanaryResources();
    this.createDashboard();
  }

  createPipeline() {
    const buildProject = new Project(this, 'aws-embedded-metrics-dotnet', {
      projectName: 'aws-embedded-metrics-dotnet',
      buildSpec: BuildSpec.fromSourceFilename('buildspecs/buildspec.yml'),
      description: 'Default build for PRs',
      source: Source.gitHub({
        owner: 'awslabs',
        repo: 'aws-embedded-metrics-dotnet',
        webhook: true,
      }),
      environment,
    });

    buildProject.addToRolePolicy(new PolicyStatement({
      actions: ['ssm:GetParameters'],
      resources: ['*']
    }));

    const canaryReleaseProject = new PipelineProject(this, 'dotnet-canary', {
      projectName: 'dotnet-canary',
      buildSpec: BuildSpec.fromSourceFilename('buildspecs/buildspec.canary.yml'),
      environment,
    });

    canaryReleaseProject.addToRolePolicy(new PolicyStatement({
      actions: [
        'ssm:GetParameters',
        'ecr:GetLogin',
        'ecs:UpdateService',
        'ecs:RegisterTaskDefinition',
      ],
      resources: ['*']
    }));

    const sourceOutput = new codepipeline.Artifact();
    const buildOutput = new codepipeline.Artifact();

    new codepipeline.Pipeline(this, 'dotnet-pipeline', {
      pipelineName: 'aws-embedded-metrics-dotnet-pipeline',
      stages: [
        {
          stageName: 'Source',
          actions: [
            new codepipeline_actions.GitHubSourceAction({
              actionName: 'Build',
              branch: 'jared/cicd', // TODO: change this to main
              owner: 'awslabs',
              repo: 'aws-embedded-metrics-dotnet',
              output: sourceOutput,
              oauthToken: cdk.SecretValue.secretsManager('github-token'),
            }),
          ],
        },
        {
          stageName: 'Build',
          actions: [
            new codepipeline_actions.CodeBuildAction({
              actionName: 'Build',
              project: buildProject,
              input: sourceOutput,
              outputs: [buildOutput],
            }),
          ],
        },
        {
          stageName: 'Deploy-Canary',
          actions: [
            new codepipeline_actions.CodeBuildAction({
              actionName: 'Deploy-Canary',
              project: canaryReleaseProject,
              input: sourceOutput,
              outputs: [new codepipeline.Artifact()],
            }),
          ],
        }
      ],
    });
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
