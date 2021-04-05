import { SynthUtils } from '@aws-cdk/assert';
import * as cdk from '@aws-cdk/core';
import * as Cdk from '../lib/code-build-stack';

test('Snapshot test', () => {
    const app = new cdk.App();
    const stack = new Cdk.CodeBuildStack(app, 'MyTestStack');
    expect(SynthUtils.toCloudFormation(stack)).toMatchSnapshot();
});
