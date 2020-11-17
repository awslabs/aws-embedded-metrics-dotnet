import * as cdk from '@aws-cdk/core';
import { BuildSpec, Source, Project } from '@aws-cdk/aws-codebuild';
import { PolicyStatement } from '@aws-cdk/aws-iam';

export class CIStack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const project = new Project(this, 'aws-embedded-metrics-dotnet', {
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

    project.addToRolePolicy(new PolicyStatement({
      actions: ['ssm:GetParameters'],
      resources: ['*']
    }));
  }
}
