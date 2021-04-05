# CDK

This CDK project is used to manage the build resources for aws-embedded-metrics-dotnet.

**Intended Audience:**

This document is intended for repository maintainers who need to make changes to the CI/CD pipeline.

## Deploying Updates

```
npm run build && npm run test
aws codebuild import-source-credentials --server-type GITHUB --auth-type PERSONAL_ACCESS_TOKEN --token <pat-token> --region us-west-2
AWS_DEFAULT_REGION=us-west-2 cdk deploy
```

## Useful commands

 * `npm run build`   compile typescript to js
 * `npm run watch`   watch for changes and compile
 * `npm run test`    perform the jest unit tests
 * `cdk deploy`      deploy this stack to your default AWS account/region
 * `cdk diff`        compare deployed stack with current state
 * `cdk synth`       emits the synthesized CloudFormation template
