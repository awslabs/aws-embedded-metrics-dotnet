import * as cdk from '@aws-cdk/core';
import { BuildSpec, Source, Project, PipelineProject } from '@aws-cdk/aws-codebuild';
import { PolicyStatement } from '@aws-cdk/aws-iam';
import * as codepipeline from '@aws-cdk/aws-codepipeline';
import * as codepipeline_actions from '@aws-cdk/aws-codepipeline-actions';

export class CIStack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);
    this.createPrBuildProject();
    this.createPipeline();
  }

  createPrBuildProject() {
    const prBuild = new Project(this, 'aws-embedded-metrics-dotnet', {
      buildSpec: BuildSpec.fromSourceFilename('buildspecs/buildspec.yml'),
      description: 'Default build for PRs',
      source: Source.gitHub({
        owner: 'awslabs',
        repo: 'aws-embedded-metrics-dotnet',
        webhook: true,
      }),
      environment: {
        privileged: true
        // Note that additional environment variables are
        // specified in the buildspec files
      }
    });

    prBuild.addToRolePolicy(new PolicyStatement({
      actions: ['ssm:GetParameters'],
      resources: ['*']
    }));
  }

  createPipeline() {
    const build = new PipelineProject(this, 'dotnet-pipeline-build', {
      buildSpec: BuildSpec.fromSourceFilename('buildspecs/buildspec.yml'),
      environment: {
        privileged: true
      },
    });

    const sourceOutput = new codepipeline.Artifact();
    const buildOutput = new codepipeline.Artifact();

    new codepipeline.Pipeline(this, 'dotnet-pipeline', {
      stages: [
        {
          stageName: 'Source',
          actions: [
            new codepipeline_actions.BitBucketSourceAction({
              actionName: 'Build',
              branch: 'main',
              owner: 'awslabs',
              repo: 'aws-embedded-metrics-dotnet',
              output: sourceOutput,
              connectionArn: 
            }),
          ],
        },
        {
          stageName: 'Build',
          actions: [
            new codepipeline_actions.CodeBuildAction({
              actionName: 'Build',
              project: build,
              input: sourceOutput,
              outputs: [buildOutput],
            }),
          ],
        }
      ],
    });
  }
}
