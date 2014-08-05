using System.Collections.Generic;

namespace ConDep.Test.Aws.Config
{
    public class Ec2BootstrapConfig
    {
        private readonly List<Ec2Instance> _instances = new List<Ec2Instance>();

        public Ec2BootstrapConfig(string bootstrapId)
        {
            BootstrapId = bootstrapId;
        }

        public string BootstrapId { get; private set; }

        public List<Ec2Instance> Instances
        {
            get { return _instances; }
        }

        public string VpcId { get; set; }
        public string AwsProfileName { get; set; }
        public string SecurityGroupId { get; set; }
        public string SecurityGroupTag { get; set; }
    }

    public class Ec2Instance
    {
        private readonly List<Ec2Snapshot> _baseSnapshots = new List<Ec2Snapshot>();

        public string InstanceId { get; set; }
        public string Tag { get; set; }
        public string PublicDns { get; set; }
        public string PublicIp { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public List<Ec2Snapshot> BaseSnapshots
        {
            get { return _baseSnapshots; }
        }
    }

    public class Ec2Snapshot
    {
        public string SnapshotId { get; set; }
        public string VolumeId { get; set; }
        public int VolumeSize { get; set; }
    }
}