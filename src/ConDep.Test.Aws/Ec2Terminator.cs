using System.Collections.Generic;
using System.Linq;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using ConDep.Test.Aws.Config;
using ConDep.Test.Aws.Logging;

namespace ConDep.Test.Aws
{
    public class Ec2Terminator
    {
        private readonly IAmazonEC2 _client;
        private Ec2SnapshotHandler _snapshotHandler;
        private Ec2InstanceHandler _instanceHandler;
        private Ec2SecurityGroupHandler _securityGroupHandler;
        private Ec2TagHandler _tagHandler;

        public Ec2Terminator(string awsProfileName)
        {
            var creds = new StoredProfileAWSCredentials(awsProfileName);
            _client = AWSClientFactory.CreateAmazonEC2Client(creds);

            _instanceHandler = new Ec2InstanceHandler(_client);
            _tagHandler = new Ec2TagHandler(_client);
            _snapshotHandler = new Ec2SnapshotHandler(_client, _tagHandler);
            _securityGroupHandler = new Ec2SecurityGroupHandler(_client);
        }

        public void Terminate(string bootstrapId, string vpcId)
        {
            IEnumerable<Instance> instances = null;
            Logger.WithLogSection("Getting instances", () =>
            {
                instances = _instanceHandler.GetInstances(bootstrapId, vpcId);
            });

            _instanceHandler.Terminate(bootstrapId, vpcId);
            _snapshotHandler.TerminateSnapshots(instances.SelectMany(y => y.BlockDeviceMappings.Select(z => z.Ebs.VolumeId)));
            _securityGroupHandler.DeleteSecurityGroup(bootstrapId);

            Logger.WithLogSection("Delete config file", () => BootstrapConfigHandler.DeleteConfigFile(bootstrapId));
        }
    }
}