using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hackaton_aws.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
        [HttpGet("/dinosaur")]
        public Task<FileStreamResult> GetDinosaur()
        {
            return DownloadDinosaur();
        }

        private async Task<FileStreamResult> DownloadDinosaur()
        {
            var accessKey = Environment.GetEnvironmentVariable("ACCESS_KEY_ID");
            var secretKey = Environment.GetEnvironmentVariable("SECRET_ACCESS_KEY");
            const string bucketName = "hackaton-aws";
            const string documentName = "3cae3694b07da2e9c81c9512bb406d5c.png";
            try
            {
                var credentials = new BasicAWSCredentials(accessKey,secretKey);
                var config = new AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.EUCentral1
                };
                using var client = new AmazonS3Client(credentials, config);
                using var fileTransferUtility = new TransferUtility(client);

                var objectResponse = await fileTransferUtility.S3Client.GetObjectAsync(new GetObjectRequest()
                {
                    BucketName = bucketName,
                    Key = documentName,
                });

                if (objectResponse.ResponseStream == null)
                {
                    throw new Exception("data not found");
                }
                return File(objectResponse.ResponseStream, objectResponse.Headers.ContentType, documentName);
            }
            catch(AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null
                    && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }
        }
    }
}

