using System;
using System.Collections.Generic;
using System.Linq;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using ConDep.Test.Aws.Logging;

namespace ConDep.Test.Aws
{
    public class Ec2AmiLocator
    {
        private readonly IAmazonEC2 _client;

        public Ec2AmiLocator(string awsProfileName)
        {
            var creds = new StoredProfileAWSCredentials(awsProfileName);
            _client = AWSClientFactory.CreateAmazonEC2Client(creds);
        }

        public string Find2012R2Core()
        {
            Logger.Info("Finding Amazon AMI image for Windows Server 2012 R2...");
            var result = GetSearchResults("Windows_Server-2012-R2_RTM-English-64Bit-Base*");
            return result.ImageId;
        }

        public string Find2012Core()
        {
            Logger.Info("Finding Amazon AMI image for Windows Server 2012...");
            var result = GetSearchResults("Windows_Server-2012-RTM-English-64Bit-Base*");
            return result.ImageId;
        }

        public string Find2008R2Core()
        {
            Logger.Info("Finding Amazon AMI image for Windows Server 2008 R2...");
            var result = GetSearchResults("Windows_Server-2008-R2_SP1-English-64Bit-Base*");
            return result.ImageId;
        }

        public string Find(string name)
        {
            name = name.Replace(" ", "*");
            if (!name.EndsWith("*"))
            {
                name += "*";
            }
            var result = GetSearchResults(name);
            return result.ImageId;
        }

        private Image GetSearchResults(string nameSearchField)
        {
            var request = new DescribeImagesRequest();
            request.ExecutableUsers.Add("all");
            request.Owners.Add("amazon");
            request.Filters = new List<Filter>
            {
                new Filter
                {
                    Name = "platform",
                    Values = new List<string>
                    {
                        "windows"
                    }
                },
                new Filter
                {
                    Name = "name",
                    Values = new List<string>
                    {
                        nameSearchField
                    }
                }
            };

            var results = _client.DescribeImages(request);
            var image = results.Images.Count > 1 ? FilterNewest(results.Images) : results.Images.Single();
            Logger.Info("Found image with id {0}, dated {1}", image.ImageId, GetDate(image.Name).ToString("yyyy-MM-dd"));
            return image;
        }

        private Image FilterNewest(IEnumerable<Image> images)
        {
            return images.OrderByDescending(x => GetDate(x.Name)).First();
        }

        private DateTime GetDate(string name)
        {
            var dateString = name.Substring(name.Length - 10);
            return DateTime.ParseExact(dateString, "yyyy.MM.dd", null);
        }
    }
}