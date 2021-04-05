import * as cdk from '@aws-cdk/core';
import { BuildSpec, Source, Project, PipelineProject, LinuxBuildImage } from '@aws-cdk/aws-codebuild';
import { PolicyStatement } from '@aws-cdk/aws-iam';

const environment = {
    // grants sudo permissions so we can use docker
    privileged: true,
    // AL2 3.0 build spec which supports .net 3.1
    // https://github.com/aws/aws-codebuild-docker-images/blob/407e02949dd24cd14a0db830c03639f34ff46ced/al2/x86_64/standard/3.0/Dockerfile#L406-L412
    buildImage: LinuxBuildImage.AMAZON_LINUX_2_3
};

export class BuildProjects {
    main: Project;
    canary: Project;
    publish: Project;

    constructor(stack: cdk.Construct) {
        this.main = new Project(stack, 'aws-embedded-metrics-dotnet', {
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

        this.main.addToRolePolicy(new PolicyStatement({
            actions: ['ssm:GetParameters'],
            resources: ['*']
        }));

        this.canary = new PipelineProject(stack, 'dotnet-canary', {
            projectName: 'dotnet-canary',
            buildSpec: BuildSpec.fromSourceFilename('buildspecs/buildspec.canary.yml'),
            environment,
        });

        this.canary.addToRolePolicy(new PolicyStatement({
            actions: [
                'ssm:GetParameters',
                'ecr:GetLogin',
                'ecs:UpdateService',
                'ecs:RegisterTaskDefinition',
            ],
            resources: ['*']
        }));

        this.publish = new PipelineProject(stack, 'dotnet-release', {
            projectName: 'dotnet-release',
            buildSpec: BuildSpec.fromSourceFilename('buildspecs/buildspec.release.yml'),
            environment,
        });

        this.publish.addToRolePolicy(new PolicyStatement({
            actions: [
                'ssm:GetParameters',
                'ecr:GetLogin',
                'ecs:UpdateService',
                'ecs:RegisterTaskDefinition',
            ],
            resources: ['*']
        }));
    }
}