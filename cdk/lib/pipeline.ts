import * as cdk from '@aws-cdk/core';
import * as codepipeline from '@aws-cdk/aws-codepipeline';
import * as codepipeline_actions from '@aws-cdk/aws-codepipeline-actions';
import { BuildProjects } from './build-projects';

export class Pipeline {
    stack: cdk.Construct;

    constructor(stack: cdk.Construct, buildProjects: BuildProjects) {
        const sourceOutput = new codepipeline.Artifact();
        const buildOutput = new codepipeline.Artifact();

        new codepipeline.Pipeline(stack, 'dotnet-pipeline', {
            pipelineName: 'aws-embedded-metrics-dotnet-pipeline',
            stages: [
                {
                    stageName: 'Source',
                    actions: [
                        new codepipeline_actions.GitHubSourceAction({
                            actionName: 'Build',
                            branch: 'main',
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
                            project: buildProjects.main,
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
                            project: buildProjects.canary,
                            input: sourceOutput,
                            outputs: [new codepipeline.Artifact()],
                        }),
                    ],
                },
                {
                    stageName: 'Bake',
                    actions: [
                        new codepipeline_actions.ManualApprovalAction({
                            actionName: 'Bake',
                        }),
                    ],
                },
                {
                    stageName: 'Release',
                    actions: [
                        new codepipeline_actions.CodeBuildAction({
                            actionName: 'Release',
                            project: buildProjects.publish,
                            input: sourceOutput,
                            outputs: [new codepipeline.Artifact()],
                        }),
                    ],
                }
            ],
        });
    }


}