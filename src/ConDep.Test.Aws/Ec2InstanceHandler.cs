using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.EC2;
using Amazon.EC2.Model;
using ConDep.Test.Aws.Logging;

namespace ConDep.Test.Aws
{
    public class Ec2InstanceHandler
    {
        private readonly IAmazonEC2 _client;

        public Ec2InstanceHandler(IAmazonEC2 client)
        {
            _client = client;
        }

        public IEnumerable<Instance> GetInstances(IEnumerable<string> instanceIds)
        {
            var instancesRequest = new DescribeInstancesRequest();
            instancesRequest.InstanceIds.AddRange(instanceIds);
            var instances = _client.DescribeInstances(instancesRequest);
            return instances.Reservations.SelectMany(x => x.Instances);
        }

        public IEnumerable<Instance> GetInstances(string bootstrapId, string vpcId)
        {
            var instancesRequest = new DescribeInstancesRequest
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Name = "tag:Name",
                        Values = new[] {Ec2TagHandler.GetNameTag(bootstrapId)}.ToList()
                    }
                }
            };
            var instances = _client.DescribeInstances(instancesRequest);
            return instances.Reservations.SelectMany(x => x.Instances);
        }

        public IEnumerable<string> CreateInstances(string boostrapId, string imageId, int numberOfInstances, string securityGroupId)
        {
            const string userData = @"<powershell>
netsh advfirewall firewall add rule name=""WinRM Public in"" protocol=TCP dir=in profile=any localport=5985 remoteip=any localip=any action=allow
</powershell>";

            var runInstancesRequest = new RunInstancesRequest()
            {
                ImageId = imageId,
                InstanceType = "t2.micro",
                MinCount = numberOfInstances,
                MaxCount = numberOfInstances,
                KeyName = "ConDep",
                UserData = Convert.ToBase64String(Encoding.UTF8.GetBytes(userData)),
                NetworkInterfaces = new List<InstanceNetworkInterfaceSpecification>
                {
                    new InstanceNetworkInterfaceSpecification
                    {
                        AssociatePublicIpAddress = true,
                        DeviceIndex = 0,
                        SubnetId = "subnet-7eba6d1b",
                        Groups = new List<string>
                        {
                            securityGroupId
                        },
                        DeleteOnTermination = true,
                    }
                }
            };

            RunInstancesResponse runResponse = _client.RunInstances(runInstancesRequest);
            return runResponse.Reservation.Instances.Select(x => x.InstanceId);
        }

        public void WaitForInstancesStatus(IEnumerable<string> instanceIds, Ec2InstanceState state)
        {
            var instances = GetInstances(instanceIds).ToList();
            var states = instances.Select(y => y.State);

            if (states.Any(x => x.Code != (int)state))
            {
                Logger.Info("One or more instances is not in state {0}, waiting 15 seconds...", state.ToString());
                Thread.Sleep(15000);
                WaitForInstancesStatus(instanceIds, state);
            }
        }

        public void Terminate(string bootstrapId, string vpcId)
        {
            Logger.Info("Terminating instances");
            var instanceRequest = new DescribeInstancesRequest
            {
                Filters = new[]
                {
                    new Filter
                    {
                        Name = "tag:Name",
                        Values = new[] {Ec2TagHandler.GetNameTag(bootstrapId)}.ToList()
                    },
                    new Filter
                    {
                        Name = "instance-state-name",
                        Values = new[] {"running", "pending", "stopping", "stopped"}.ToList()
                    },
                    new Filter
                    {
                        Name = "vpc-id",
                        Values = new[] {vpcId}.ToList()
                    }
                }.ToList()
            };
            var instances = _client.DescribeInstances(instanceRequest);

            var terminationRequest = new TerminateInstancesRequest();
            var instanceIds = instances.Reservations.SelectMany(x => x.Instances.Select(y => y.InstanceId)).ToList();
            terminationRequest.InstanceIds.AddRange(instanceIds);

            _client.TerminateInstances(terminationRequest);
            Logger.WithLogSection("Waiting for instances to terminate", () => WaitForInstancesToTerminate(instanceIds));
        }

        private void WaitForInstancesToTerminate(List<string> instanceIds)
        {
            var instances = GetInstances(instanceIds).ToList();
            var states = instances.Select(y => y.State);

            if (states.Any(x => x.Name != "terminated"))
            {
                Logger.Info("Instance still terminating. Waiting another 5 seconds...");
                Thread.Sleep(5000);
                WaitForInstancesToTerminate(instanceIds);
            }
        }
    }
}