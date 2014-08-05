using System.Collections.Generic;
using System.Threading;
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

        public string CreateNameTags(string bootstrapId, IEnumerable<string> ids)
        {
            var tag = TAG_NAME_PREFIX + bootstrapId;

            var tagRequest = new CreateTagsRequest();
            tagRequest.Resources.AddRange(ids);

            tagRequest.Tags.Add(new Tag { Key = "Name", Value = tag });
            try
            {
                _client.CreateTags(tagRequest);
            }
            catch
            {
                Thread.Sleep(5000);
                _client.CreateTags(tagRequest);
            }
            return tag;
        }

        public static string GetNameTag(string bootstrapId)
        {
            return TAG_NAME_PREFIX + bootstrapId;
        }
    }
}