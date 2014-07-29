using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Amazon.EC2;
using Amazon.EC2.Model;
using ConDep.Test.Aws.Logging;

namespace ConDep.Test.Aws
{
    public class Ec2SecurityGroupHandler
    {
        private const string SEC_GROUP_PREFIX = "secgroup-condep-int-";
        private readonly IAmazonEC2 _client;

        public Ec2SecurityGroupHandler(IAmazonEC2 client)
        {
            _client = client;
        }

        public string CreateSecurityGroup(string vpcId, string boostrapId)
        {
            string secGroupName = SEC_GROUP_PREFIX + boostrapId;

            if (SecurityGroupExist(secGroupName))
            {
                DeleteSecurityGroup(secGroupName);
            }
            var newGroupRequest = new CreateSecurityGroupRequest()
            {
                GroupName = secGroupName,
                Description = "Security Group for ConDep integration tests",
                VpcId = vpcId,
            };

            var newGroupResponse = _client.CreateSecurityGroup(newGroupRequest);

            var publicIp = GetPublicIp() + "/32";
            var ruleRequest = new AuthorizeSecurityGroupIngressRequest()
            {
                GroupId = newGroupResponse.GroupId,
                IpPermissions = new List<IpPermission>
                {
                    new IpPermission
                    {
                        FromPort = 0,
                        ToPort = 65535,
                        IpProtocol = "tcp",
                        IpRanges = new List<string>
                        {
                            publicIp
                        }
                    },
                    new IpPermission
                    {
                        FromPort = -1,
                        ToPort = -1,
                        IpProtocol = "icmp",
                        IpRanges = new List<string>
                        {
                            publicIp
                        }
                    }
                }
            };
            _client.AuthorizeSecurityGroupIngress(ruleRequest);
            return newGroupResponse.GroupId;
        }

        private string GetPublicIp()
        {
            var client = new HttpClient {BaseAddress = new Uri("http://ipinfo.io")};
            var response = client.GetAsync("ip");
            return response.Result.Content.ReadAsStringAsync().Result.TrimEnd();
        }

        private string GetSecurityGroupId(string secGroupName)
        {
            var existingGroupRequest = new DescribeSecurityGroupsRequest();
            existingGroupRequest.Filters.Add(new Filter { Name = "group-name", Values = new[] { secGroupName }.ToList() });
            var existingGroup = _client.DescribeSecurityGroups(existingGroupRequest);
            return existingGroup.SecurityGroups[0].GroupId;
        }

        private bool SecurityGroupExist(string secGroupName)
        {
            var existingGroupRequest = new DescribeSecurityGroupsRequest();
            existingGroupRequest.Filters.Add(new Filter { Name = "group-name", Values = new[] { secGroupName }.ToList() });
            var existingGroup = _client.DescribeSecurityGroups(existingGroupRequest);
            return existingGroup.SecurityGroups.Count > 0;
        }

        public void DeleteSecurityGroup(string bootstrapId)
        {
            var secGroupName = SEC_GROUP_PREFIX + bootstrapId;
            Logger.Info("Deleting security group {0}", secGroupName);

            var id = GetSecurityGroupId(secGroupName);
            var deleteRequest = new DeleteSecurityGroupRequest { GroupId = id };
            try
            {
                _client.DeleteSecurityGroup(deleteRequest);
            }
            catch (AmazonEC2Exception)
            {
                Logger.Warn("Failed to delete security group {0}", secGroupName);
                Logger.Info("Waiting 10 seconds before retry...");
                Thread.Sleep(10000);
                DeleteSecurityGroup(bootstrapId);
            }
        }

    }
}