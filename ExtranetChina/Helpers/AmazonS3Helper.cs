using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Amazon;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.S3;
using Amazon.S3.Model;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Mail;

namespace AmazonSyncADUsers
{
    class AmazonS3Helper
    {
        private static string AMAZONSecurityGroup = ConfigurationManager.AppSettings["AMAZONSecurityGroup"];
        private static string AMAZONBucket = ConfigurationManager.AppSettings["AMAZONBucket"];
        private static string AMAZONPublicFolder = ConfigurationManager.AppSettings["AMAZONPublicFolder"];
        private static string AMAZONPersonalFolder = ConfigurationManager.AppSettings["AMAZONPersonalFolder"];
       
        public static string CreateFileShare(string fileName, string fileContent)
        {
            try
            {
                var s3Client = AWSClientFactory.CreateAmazonS3Client();

                String S3_KEY = string.Format("{0}{1}_{2}.pdf", AMAZONPublicFolder, fileName, Guid.NewGuid().ToString());
                var request = new PutObjectRequest()
                {
                    BucketName = AMAZONBucket,
                    Key = S3_KEY,
                    ContentBody = fileContent
                };

                s3Client.PutObject(request);

                string preSignedURL = s3Client.GetPreSignedURL(new GetPreSignedUrlRequest()
                {
                    BucketName = AMAZONBucket,
                    Key = S3_KEY,
                    Expires = System.DateTime.Now.AddMinutes(60*72)
                });

                return preSignedURL;
            }
            catch (Amazon.S3.AmazonS3Exception ex)
            {
                throw;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        
    }
}
