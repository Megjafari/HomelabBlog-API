using Minio;
using Minio.DataModel.Args;

namespace HomelabBlog.Api.Services;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private readonly string _publicBaseUrl;
    private readonly SemaphoreSlim _bucketLock = new(1, 1);
    private bool _bucketReady;

    public MinioFileStorageService(IConfiguration config)
    {
        _bucket = config["Minio:BucketName"]!;
        _publicBaseUrl = config["Minio:PublicBaseUrl"] ?? $"http://{config["Minio:Endpoint"]}/{_bucket}";

        _client = new MinioClient()
            .WithEndpoint(config["Minio:Endpoint"])
            .WithCredentials(config["Minio:AccessKey"], config["Minio:SecretKey"])
            .WithSSL(bool.Parse(config["Minio:UseSsl"] ?? "false"))
            .Build();
    }

    private async Task EnsureBucketAsync()
    {
        if (_bucketReady) return;

        await _bucketLock.WaitAsync();
        try
        {
            if (_bucketReady) return;
            await CreateBucketIfMissingAsync();
            _bucketReady = true;
        }
        finally
        {
            _bucketLock.Release();
        }
    }

    private async Task CreateBucketIfMissingAsync()
    {
        var bucketExists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket));
        if (!bucketExists)
        {
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket));

            var policy = $$"""
            {
            "Version": "2012-10-17",
            "Statement": [
                {
                "Effect": "Allow",
                "Principal": "*",
                "Action": ["s3:GetObject"],
                "Resource": ["arn:aws:s3:::{{_bucket}}/*"]
                }
            ]
            }
            """;

            await _client.SetPolicyAsync(new SetPolicyArgs()
                .WithBucket(_bucket)
                .WithPolicy(policy));
        }
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        await EnsureBucketAsync();

        // Never trust the client's file name: keep only the extension.
        var objectName = $"{Guid.NewGuid()}{Path.GetExtension(fileName).ToLowerInvariant()}";

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType));

        return $"{_publicBaseUrl}/{objectName}";
    }
}