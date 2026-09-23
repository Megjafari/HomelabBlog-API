using Minio;
using Minio.DataModel.Args;

namespace HomelabBlog.Api.Services;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private readonly string _publicBaseUrl;

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

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var bucketExists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket));
        if (!bucketExists)
        {
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket));
        }

        var objectName = $"{Guid.NewGuid()}-{fileName}";

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType));

        return $"{_publicBaseUrl}/{objectName}";
    }
}