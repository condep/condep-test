using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon.EC2;
using Amazon.EC2.Model;
using ConDep.Test.Aws.Config;
using ConDep.Test.Aws.Logging;

namespace ConDep.Test.Aws
{
    public class Ec2SnapshotHandler
    {
        private readonly IAmazonEC2 _client;
        private readonly Ec2TagHandler _tagHandler;

        public Ec2SnapshotHandler(IAmazonEC2 client, Ec2TagHandler tagHandler)
        {
            _client = client;
            _tagHandler = tagHandler;
        }

        public void TakeSnapshots(IEnumerable<Instance> instances, Ec2BootstrapConfig config)
        {
            foreach (var instance in instances)
            {
                foreach (var volumeId in instance.BlockDeviceMappings.Select(x => x.Ebs.VolumeId))
                {
                    var request = new CreateSnapshotRequest
                    {
                        Description = "Snapshot for ConDep integration testing",
                        VolumeId = volumeId
                    };
                    var response = _client.CreateSnapshot(request);
                    config.Instances
                        .Single(x => x.InstanceId == instance.InstanceId)
                        .BaseSnapshots.Add(new Ec2Snapshot
                        {
                            SnapshotId = response.Snapshot.SnapshotId,
                            VolumeId = response.Snapshot.VolumeId,
                            VolumeSize = response.Snapshot.VolumeSize
                        });
                }
            }

            var snapshotIds = config.Instances.SelectMany(x => x.BaseSnapshots.Select(y => y.SnapshotId)).ToList();
            _tagHandler.CreateNameTags(config.BootstrapId, snapshotIds);

            Logger.WithLogSection("Waiting for snapshots to finish", () => WaitForSnapshotsToFinish(snapshotIds));
        }

        private void WaitForSnapshotsToFinish(List<string> snapshotIds)
        {
            var request = new DescribeSnapshotsRequest
            {
                SnapshotIds = snapshotIds
            };

            var response = _client.DescribeSnapshots(request);
            Logger.WithLogSection("Status", () =>
            {
                foreach (var snapshot in response.Snapshots)
                {
                    Logger.Info("Snapshot: {0} Progress: {1} State: {2}", snapshot.SnapshotId, string.IsNullOrWhiteSpace(snapshot.Progress) ? "Unknown" : snapshot.Progress, snapshot.State.Value);
                }
            });

            if (response.Snapshots.Any(x => x.State.Value != SnapshotState.Completed))
            {
                Thread.Sleep(10000);
                WaitForSnapshotsToFinish(snapshotIds);
            }

        }

        public void TerminateSnapshots(IEnumerable<string> volumeIds)
        {
            Logger.WithLogSection("Terminating snapshots", () =>
            {
                foreach (var volumeId in volumeIds)
                {
                    var describe = new DescribeSnapshotsRequest
                    {
                        Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Name = "volume-id",
                            Values = new[]{volumeId}.ToList()
                        }
                    }
                    };
                    var snapshots = _client.DescribeSnapshots(describe);

                    foreach (var snapshot in snapshots.Snapshots)
                    {
                        var request = new DeleteSnapshotRequest
                        {
                            SnapshotId = snapshot.SnapshotId
                        };
                        _client.DeleteSnapshot(request);
                        Logger.Info("Snapshot {0} deleted.", snapshot.SnapshotId);
                    }
                }
            });
        }
    }
}