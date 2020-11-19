import * as cdk from '@aws-cdk/core';
import { BuildSpec, Source, Project, PipelineProject } from '@aws-cdk/aws-codebuild';
import { PolicyStatement } from '@aws-cdk/aws-iam';
import * as codepipeline from '@aws-cdk/aws-codepipeline';
import * as codepipeline_actions from '@aws-cdk/aws-codepipeline-actions';
import * as ecr from '@aws-cdk/aws-ecr';

export class CIStack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);
    this.createPrBuildProject();
    this.createPipeline();
    this.createCanaryResources();
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
    const buildProject = new PipelineProject(this, 'dotnet-pipeline-build', {
      buildSpec: BuildSpec.fromSourceFilename('buildspecs/buildspec.yml'),
      environment: {
        privileged: true
      },
    });

    const canaryReleaseProject = new PipelineProject(this, 'dotnet-pipeline-canary', {
      buildSpec: BuildSpec.fromSourceFilename('buildspecs/buildspec.canary.yml'),
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
}
