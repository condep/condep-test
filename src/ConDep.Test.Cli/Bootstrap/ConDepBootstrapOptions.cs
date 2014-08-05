namespace ConDep.Test.Cli.Bootstrap
{
    public class ConDepBootstrapOptions
    {
        public int NumOfInstances { get; set; }
        public string InstanceType { get; set; }
        public string SecurityGroup { get; set; }
        public string AwsProfileName { get; set; }
        public string VpcId { get; set; }
        public string RsaPrivateKeyPath { get; set; }
        public string AmiId { get; set; }
    }
}