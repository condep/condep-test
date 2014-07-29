using System.Collections.Generic;
using Amazon.EC2;
using Amazon.EC2.Model;

namespace ConDep.Test.Aws
{
    public class Ec2TagHandler
    {
        private const string TAG_NAME_PREFIX = "condep-int-";
        private readonly IAmazonEC2 _client;

        public Ec2TagHandler(IAmazonEC2 client)
        {
            _client = client;
        }

        public string CreateInstanceNameTags(string bootstrapId, IEnumerable<string> instanceIds)
        {
            var tag = TAG_NAME_PREFIX + bootstrapId;

            var tagRequest = new CreateTagsRequest();
            tagRequest.Resources.AddRange(instanceIds);

            tagRequest.Tags.Add(new Tag { Key = "Name", Value = tag });
            _client.CreateTags(tagRequest);
            return tag;
        }

        public string CreateSnapshotNameTags(string bootstrapId, IEnumerable<string> snapshotIds)
        {
            var tag = TAG_NAME_PREFIX + bootstrapId;

            var tagRequest = new CreateTagsRequest();
            tagRequest.Resources.AddRange(snapshotIds);

            tagRequest.Tags.Add(new Tag { Key = "Name", Value = tag });
            _client.CreateTags(tagRequest);
            return tag;
        }

        public static string GetNameTag(string bootstrapId)
        {
            return TAG_NAME_PREFIX + bootstrapId;
        }
    }
}