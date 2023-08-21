using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace protecno.api.sync.domain.helpers
{
    public interface IS3Helper /*: IDisposable*/
    {
        //RequestReportResult UploadReportToS3(string fileName, string keyName, string bucketName);

        //void DowloadFileFromS3(string bucket, string keyName, string filePath);

        StringBuilder GetStreamObject(string bucketName, string keyName);
      
        Task<StringBuilder> GetStreamObjectAsync(string bucketName, string keyName);
        
        bool FileExists(string bucketName, string path, string fileName);
        
        Task<bool> FileExistsAsync(string bucketName, string path, string fileName);
        
        bool FolderExists(string bucket, string filePath, string key);
        
        Task<bool> FolderExistsAsync(string bucket, string filePath, string key);
        
        PutObjectResponse CreateDirectory(string bucket, string fileName);
        
        Task<PutObjectResponse> CreateDirectoryAsync(string bucket, string fileName);
        
        CopyObjectResponse CopyStreamObject(string sourceBucket, string objectKey, string destinationBucket, string destObjectKey);
        
        Task<CopyObjectResponse> CopyStreamObjectAsync(string sourceBucket, string objectKey, string destinationBucket, string destObjectKey);
        
        string ReadObjectData(string bucketName, string keyName);
        
        Task<string> ReadObjectDataAsync(string bucketName, string keyName);
        
        HttpWebResponse UploadObject(string filePath, string objectKey, string bucketName, double expiresPreSignedURL);
    }

    public sealed class S3Helper : IS3Helper
    {
        private readonly IAmazonS3 _client;

        public S3Helper(IAmazonS3 client)
        {
            _client = client;
        }

        public StringBuilder GetStreamObject(string bucketName, string keyName)
        {
            return GetStreamObjectAsync(bucketName, keyName).GetAwaiter().GetResult();
        }

        public async Task<StringBuilder> GetStreamObjectAsync(string bucketName, string keyName)
        {
            StringBuilder responseBody = new StringBuilder();
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                using GetObjectResponse response = await _client.GetObjectAsync(request);
                using Stream responseStream = response.ResponseStream;
                using StreamReader reader = new StreamReader(responseStream);
                while (!reader.EndOfStream)
                {
                    responseBody.AppendLine(await reader.ReadLineAsync());
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.ToString());
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.ToString());
                throw;
            }

            return responseBody;
        }

        public bool FileExists(string bucketName, string path, string fileName)
        {
            return FileExistsAsync(bucketName, path, fileName).GetAwaiter().GetResult();
        }

        public async Task<bool> FileExistsAsync(string bucketName, string path, string fileName)
        {
            var getObjects = new GetObjectMetadataRequest()
            {
                BucketName = bucketName,
                Key = $"{path}/{fileName}",
            };

            try
            {
                var response = await _client.GetObjectMetadataAsync(getObjects);
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error File Exists: " + ex.ToString());
                return false;
            }
        }

        public bool FolderExists(string bucket, string filePath, string key)
        {
            return FolderExistsAsync(bucket, filePath, key).GetAwaiter().GetResult();
        }

        public async Task<bool> FolderExistsAsync(string bucket, string filePath, string key)
        {
            try
            {
                var request = new ListObjectsRequest
                {
                    BucketName = bucket,
                    Delimiter = "/",
                    Prefix = filePath
                };

                var response = await _client.ListObjectsAsync(request);

                var commonPrefixes = response.CommonPrefixes;
                var folderExists = commonPrefixes.Any();

                return folderExists;
            }
            catch (Exception ex)
            {
                Console.WriteLine("FolderExists: " + ex.ToString());
                return false;
            }
        }

        public PutObjectResponse CreateDirectory(string bucket, string fileName)
        {
            return CreateDirectoryAsync(bucket, fileName).GetAwaiter().GetResult();
        }

        public async Task<PutObjectResponse> CreateDirectoryAsync(string bucket, string fileName)
        {
            var request = new PutObjectRequest
            {
                ContentBody = "",
                BucketName = bucket,
                Key = fileName,
                CannedACL = S3CannedACL.Private
            };

            return await _client.PutObjectAsync(request);
        }

        public CopyObjectResponse CopyStreamObject(string sourceBucket, string objectKey, string destinationBucket, string destObjectKey)
        {
            return CopyStreamObjectAsync(sourceBucket, objectKey, destinationBucket, destObjectKey).GetAwaiter().GetResult();
        }

        public async Task<CopyObjectResponse> CopyStreamObjectAsync(string sourceBucket, string objectKey, string destinationBucket, string destObjectKey)
        {
            CopyObjectRequest requestS3 = new CopyObjectRequest
            {
                SourceBucket = sourceBucket,
                SourceKey = objectKey,
                DestinationBucket = destinationBucket,
                DestinationKey = destObjectKey
            };

            return await _client.CopyObjectAsync(requestS3);
        }

        public string ReadObjectData(string bucketName, string keyName)
        {
            return ReadObjectDataAsync(bucketName, keyName).GetAwaiter().GetResult();
        }

        public async Task<string> ReadObjectDataAsync(string bucketName, string keyName)
        {
            string responseBody = "";
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = keyName
            };

            using (GetObjectResponse response = await _client.GetObjectAsync(request))
            using (Stream responseStream = response.ResponseStream)
            using (StreamReader reader = new StreamReader(responseStream))
            {
                responseBody = await reader.ReadToEndAsync();
            }

            return responseBody;
        }

        public HttpWebResponse UploadObject(string filePath, string objectKey, string bucketName, double expiresPreSignedURL)
        {
            if (!AmazonS3Util.DoesS3BucketExistV2Async(_client, bucketName).Result)
                CreateBucketAsync(bucketName);

            string url = GeneratePreSignedURL(bucketName, objectKey, expiresPreSignedURL);

            HttpWebRequest httpRequest = WebRequest.Create(url) as HttpWebRequest;
            httpRequest.Method = "PUT";
            using (Stream dataStream = httpRequest.GetRequestStream())
            {
                var buffer = new byte[8000];
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    int bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        dataStream.Write(buffer, 0, bytesRead);
                    }
                }
            }

            return httpRequest.GetResponse() as HttpWebResponse;
        }

        private string GeneratePreSignedURL(string bucketName, string objectKey, double ExpiresPreSignedURL)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddHours(ExpiresPreSignedURL),
            };

            return _client.GetPreSignedURL(request);
        }

        private void CreateBucketAsync(string bucketName)
        {
            try
            {
                var request = new PutBucketRequest
                {
                    BucketName = bucketName,
                    UseClientRegion = true,
                };

                _client.PutBucketAsync(request);                
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error creating bucket: '{ex.Message}'");                
            }
        }

        //public RequestReportResult UploadReportToS3(string fileName, string keyName, string bucketName)
        //{
        //    HttpWebResponse responseS3 = _s3Helper.UploadObject(fileName, keyName, bucketName, 2);

        //    if (responseS3.StatusCode == HttpStatusCode.OK)
        //    {
        //        return new RequestReportResult()
        //        {
        //            StatusCode = System.Net.HttpStatusCode.OK,
        //            //
        //        };
        //    }
        //    else
        //        throw new Exception("Erro ao enviar relatorio para o S3");
        //}

        //public void DowloadFileFromS3(string bucket, string keyName, string filePath)
        //{
        //    string file = _s3Helper.ReadObjectData(bucket, keyName);

        //    File.WriteAllText(filePath, file);
        //}
    }
}
