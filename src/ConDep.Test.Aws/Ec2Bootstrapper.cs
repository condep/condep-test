using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using ConDep.Test.Aws.Config;
using ConDep.Test.Aws.Logging;

namespace ConDep.Test.Aws
{
    public class Ec2Bootstrapper
    {
        private readonly string _awsProfileName;
        private readonly IAmazonEC2 _client;
        private Ec2InstanceHandler _instanceHandler;
        private Ec2SecurityGroupHandler _securityGroupHandler;
        private Ec2TagHandler _tagHandler;
        private Ec2InstancePasswordHandler _passwordHandler;
        //private Ec2SnapshotHandler _snapshotHandler;

        public Ec2Bootstrapper(string awsProfileName)
        {
            _awsProfileName = awsProfileName;
            var creds = new StoredProfileAWSCredentials(awsProfileName);
            _client = AWSClientFactory.CreateAmazonEC2Client(creds);

            _instanceHandler = new Ec2InstanceHandler(_client);
            _securityGroupHandler = new Ec2SecurityGroupHandler(_client);
            _tagHandler = new Ec2TagHandler(_client);
            _passwordHandler = new Ec2InstancePasswordHandler(_client);
            //_snapshotHandler = new Ec2SnapshotHandler(_client, _tagHandler);
        }

        public string Boostrap(string vpcId, string imageId, int numberOfInstances, string pathToRsaPrivateKey)
        {
            var config = new Ec2BootstrapConfig(Guid.NewGuid().ToString())
            {
                VpcId = vpcId,
                AwsProfileName = _awsProfileName
            };

            Logger.Info("Creating security group...");
            var securityGroupId = _securityGroupHandler.CreateSecurityGroup(vpcId, config.BootstrapId);
            Logger.Info("Creating instances...");
            var instanceIds = _instanceHandler.CreateInstances(config.BootstrapId, imageId, numberOfInstances, securityGroupId).ToList();

            Logger.Info("Tagging instances...");
            var tag = _tagHandler.CreateInstanceNameTags(config.BootstrapId, instanceIds);

            Logger.WithLogSection("Waiting for instances to be ready", () => _instanceHandler.WaitForInstancesStatus(instanceIds, Ec2InstanceState.Running));

            List<Tuple<string, string>> passwords = null;

            Logger.WithLogSection("Waiting for Windows password to be available", () =>
            {
                passwords = _passwordHandler.WaitForPassword(instanceIds, pathToRsaPrivateKey);
            });

            var instances = _instanceHandler.GetInstances(instanceIds).ToList();

            foreach (var instance in instances)
            {
                config.Instances.Add(new Ec2Instance
                {
                    InstanceId = instance.InstanceId,
                    PublicDns = instance.PublicDnsName,
                    PublicIp = instance.PublicIpAddress,
                    Tag = tag,
                    UserName = "Administrator",
                    Password = passwords.Single(x => x.Item1 == instance.InstanceId).Item2
                });
            }

            HaveAccessToServers(config);
            //StopServers(config);

            //if (takeSnapshots)
            //{
            //    Logger.Info("Taking snapshots of disks to enable resets...");
            //    _snapshotHandler.TakeSnapshots(instances, config);
            //}
            //else
            //{
            //    Logger.Warn("Snapshots disabled. Note that reset will not work without snapshots.");
            //}

            //StartServers(config);

            Logger.Info("Storing configuration...");
            var configHandler = new BootstrapConfigHandler(config.BootstrapId);
            configHandler.Write(@"C:\temp\", config);
            return config.BootstrapId;
        }

        private void StopServers(Ec2BootstrapConfig config)
        {
            Logger.WithLogSection("Stopping servers", () =>
            {
                var instanceIds = config.Instances.Select(x => x.InstanceId).ToList();
                var request = new StopInstancesRequest
                {
                    InstanceIds = instanceIds
                };
                _client.StopInstances(request);
                _instanceHandler.WaitForInstancesStatus(instanceIds, Ec2InstanceState.Stopped);
            });
        }

        private void StartServers(Ec2BootstrapConfig config)
        {
            Logger.WithLogSection("Starting servers", () =>
            {
                var instanceIds = config.Instances.Select(x => x.InstanceId).ToList();
                var request = new StartInstancesRequest
                {
                    InstanceIds = instanceIds
                };
                _client.StartInstances(request);
                _instanceHandler.WaitForInstancesStatus(instanceIds, Ec2InstanceState.Running);
            });
        }

        private static void HaveAccessToServers(Ec2BootstrapConfig config)
        {
            Logger.WithLogSection("Checking access to servers", () =>
            {
                foreach (var instance in config.Instances)
                {
                    HaveAccessToServer(instance, 5);
                }
            });
        }
        private static void HaveAccessToServer(Ec2Instance instance, int numOfRetries)
        {
            Logger.Info(
                string.Format("({1}/5) Checking if WinRM (Remote PowerShell) can be used to reach remote server [{0}]...",
                              instance.PublicDns, numOfRetries));
            var cmd = string.Format("id -r:{0} -u:{1} -p:\"{2}\"", instance.PublicDns, instance.UserName, instance.Password);

            var path = Environment.ExpandEnvironmentVariables(@"%windir%\system32\WinRM.cmd");
            var startInfo = new ProcessStartInfo(path)
            {
                Arguments = cmd,
                Verb = "RunAs",
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            var process = Process.Start(startInfo);
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                var message = process.StandardOutput.ReadToEnd();
                Logger.Info(string.Format("Contact was made with server [{0}] using WinRM (Remote PowerShell). ",
                                          instance.PublicDns));
                Logger.Verbose(string.Format("Details: {0} ", message));
            }
            else
            {
                var errorMessage = process.StandardError.ReadToEnd();
                if (numOfRetries > 0)
                {
                    Logger.Warn(string.Format("Unable to reach server [{0}] using WinRM (Remote PowerShell)",
                        instance.PublicDns));
                    Logger.Info("Waiting 30 seconds before retry...");
                    Thread.Sleep(30000);
                    HaveAccessToServer(instance, --numOfRetries);
                }
                else
                {
                    Logger.Error(string.Format("Unable to reach server [{0}] using WinRM (Remote PowerShell)",
                        instance.PublicDns));
                    Logger.Error(string.Format("Details: {0}", errorMessage));
                    Logger.Error("Max number of retries exceeded. Please check your Amazon Network firewall for why WinRM cannot connect.");
                }
            }
        }
    }
}