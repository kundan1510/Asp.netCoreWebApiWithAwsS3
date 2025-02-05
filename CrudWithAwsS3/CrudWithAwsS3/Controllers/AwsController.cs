using Amazon;
using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace CrudWithAwsS3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AwsController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AmazonS3Client _client;
        public AwsController(IConfiguration config)
        {
            _config = config;
            var awsAccess = _config.GetValue<string>("AWS:AccessKey");
            var awsSecret = _config.GetValue<string>("AWS:SecretKey");
            _client = new AmazonS3Client(awsAccess, awsSecret, RegionEndpoint.EUNorth1);
        }


        [HttpGet("test-aws-client")]
        public async Task<IActionResult> TestAWSClient()
        {
            try
            {
                var result = await _client.ListBucketsAsync();
                return Ok("AmazonS3Client works fine");
            }
            catch (Exception ex)
            {
                return Ok("AmazonS3Client does not work");
            }

        }

        [HttpGet("list-bucket")]
        public async Task<IActionResult> ListBucket()
        {
            try
            {
                var result = await _client.ListBucketsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok("Bucket could not be listed");
            }
        }


        [HttpPost("create-bucket/{bucketName}")]
        public async Task<IActionResult> CreateBucket(string bucketName)
        {
            try
            {
                PutBucketRequest request = new PutBucketRequest() { BucketName = bucketName };
                await _client.PutBucketAsync(request);
                return Ok($"Bucket: {bucketName} WAS created");
            }
            catch (Exception)
            {
                return BadRequest($"Bucket: {bucketName} WAS NOT created");
            }

        }

        [HttpPost("enable-versioning/{bucketName}")]
        public async Task<IActionResult> EnableVersioning(string bucketName)
        {
            try
            {
                PutBucketVersioningRequest request = new PutBucketVersioningRequest
                {
                    BucketName = bucketName,
                    VersioningConfig = new S3BucketVersioningConfig
                    {
                        Status = VersionStatus.Enabled
                    }
                };

                await _client.PutBucketVersioningAsync(request);
                return Ok($"Bucket {bucketName} versioning ENABLED");
            }
            catch (Exception)
            {
                return BadRequest($"Bucket: {bucketName} WAS NOT ENABLED");
            }
        }

        [HttpPost("create-folder/{bucketName}/{folderName}")]
        public async Task<IActionResult> CreateFolder(string bucketName, string folderName)
        {
            try
            {
                PutObjectRequest request = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = folderName.Replace("%2F", "/"),
                };
                await _client.PutObjectAsync(request);
                return Ok($"{folderName} folder was created inside {bucketName}");
            }
            catch (Exception)
            {
                return BadRequest($"The folder COULD NOT be created");
            }
        }

        [HttpDelete("delete-bucket/{bucketName}")]
        public async Task<IActionResult> DeleteBucket(string bucketName)
        {
            try
            {

                DeleteBucketRequest request = new DeleteBucketRequest() { BucketName = bucketName };
                await _client.DeleteBucketAsync(request);
                return Ok($"{bucketName} bucket was deleted");
            }
            catch (Exception)
            {
                return BadRequest($"{bucketName} bucket WAS NOT Deleted");
            }
        }

        [HttpPost("create-object/{bucketName}/{objectName}")]
        public async Task<IActionResult> CreateObject(string bucketName, string objectName)
        {
            try
            {
                PutObjectRequest request = new PutObjectRequest()
                {

                    BucketName = bucketName,
                    Key = objectName,
                    ContentType = "text/plain",
                    ContentBody = "Welcome to LinkedIn Learning"
                };
                await _client.PutObjectAsync(request);
                return Ok($"File create/uploaded");
            }
            catch (Exception)
            {
                return BadRequest($"File was not created/uploaded");
            }
        }

        [HttpPost("create-object-from-file/{bucketName}/{objectName}")]
        public async Task<IActionResult> CreateObjectFromFile(string bucketName)
        {
            try
            {
                FileInfo file = new FileInfo(@"C:\AWSFile\AwsFile.txt");
                PutObjectRequest request = new PutObjectRequest()
                {
                    InputStream = file.OpenRead(),
                    BucketName = bucketName,
                    Key = "AwsFile.txt",
                };
                await _client.PutObjectAsync(request);
                ListObjectsRequest listObjectsRequest = new ListObjectsRequest()
                {
                    BucketName = bucketName,
                };
                ListObjectsResponse listObjectsResponse = await _client.ListObjectsAsync(listObjectsRequest);
                return Ok(listObjectsResponse);
            }
            catch (Exception)
            {
                return BadRequest($"File was not created/uploaded");
            }
        }

        [HttpGet("list-object/{bucketName}")]
        public async Task<IActionResult> ListObjects(string bucketName)
        {
            try
            {
                ListObjectsRequest objectsRequest = new ListObjectsRequest()
                {
                    BucketName = bucketName
                };

                ListObjectsResponse response = await _client.ListObjectsAsync(objectsRequest);
                return Ok(response);
            }

            catch (Exception)
            {
                return BadRequest($"Objects could not be listed");
            }
        }


        [HttpPut("add-tags-metadata/{bucketName}/{fileName}")]
        public async Task<IActionResult> AddTagsMetadata(string bucketName, string fileName)
        {
            try
            {
                Tagging newTags = new Tagging()
                {
                    TagSet = new List<Tag>
                    {
                        new Tag { Key = "Key1", Value = "FirstTag" },
                        new Tag { Key = "Key2", Value = "SecondTag" }

                    }
                };

                PutObjectTaggingRequest request = new PutObjectTaggingRequest()
                {
                    BucketName = bucketName,
                    Key = fileName,
                    Tagging = newTags
                };
                await _client.PutObjectTaggingAsync(request);
                return Ok($"Tags added!");
            }
            catch (Exception)
            {
                return BadRequest($"Tags COULD NOT be added");
            }
        }

        [HttpPut("copy-file/{sourceBucket}/{sourceKey}/{destinationBucket}/{destinationkey}")]
        public async Task<IActionResult> CopyFile(string sourceBucket, string sourcekey, string destinationBucket, string destinationkey)
        {
            try
            {
                CopyObjectRequest request = new CopyObjectRequest()
                {
                    SourceBucket = sourceBucket,
                    SourceKey = sourcekey,
                    DestinationBucket = destinationBucket,
                    DestinationKey = destinationkey
                };

                await _client.CopyObjectAsync(request);
                return Ok($"Object/File copied");
            }
            catch (Exception)
            {
                return BadRequest($"Object/File WAS NOT copied");
            }
        }



        [HttpGet("generate-download-link/{bucketName}/{keyName}")]
        public IActionResult GenerateDownloadLink(string bucketName, string keyName)
        {
            try
            {
                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketName,
                    Key = keyName,
                    Expires = DateTime.Now.AddHours(5),
                    Protocol = Protocol.HTTP
                };
                string downloadLink = _client.GetPreSignedURL(request);
                return Ok($"Download link {downloadLink}");
            }
            catch (Exception)
            {
                return BadRequest("Download link WAS NOT generated");
            }
        }


        [HttpDelete("delete-bucket-object/{bucketName}/{objectName}")]
        public async Task<IActionResult> DeleteBucketObject(string bucketName, string objectName)
        {
            try
            {
                DeleteObjectRequest request = new DeleteObjectRequest()
                {
                    BucketName = bucketName,
                    Key = objectName
                };
                await _client.DeleteObjectAsync(request);
                return Ok($"{objectName} in {bucketName} bucket was deleted");
            }
            catch (Exception)
            {
                return BadRequest($"{objectName} in {bucketName} bucket WAS NOT deleted");

            }
        }

        [HttpDelete("cleanup-bucket/{bucketName}")]
        public async Task<IActionResult> CleanUpBucket(string bucketName)
        {
            try
            {
                DeleteObjectsRequest request = new DeleteObjectsRequest()
                {
                    BucketName = bucketName
                };

                await _client.DeleteObjectsAsync(request);
                return Ok($" {bucketName} bucket was cleaned up");
            }
            catch (Exception)
            {
                return BadRequest($"{bucketName} bucket WAS NOT cleaned up");
            }
        }
    }
}