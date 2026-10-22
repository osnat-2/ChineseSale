using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Project.Bll;
using Project.Controllers;
using Xunit;

namespace Project.Tests;

public class PresentImageStorageTests
{
    [Fact]
    public async Task SaveAsync_StoresPngUnderGeneratedLocalName()
    {
        var webRoot = CreateTemporaryDirectory();
        try
        {
            var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01, 0x02, 0x03, 0x04 };
            var image = CreateFormFile(imageBytes, "../../untrusted.svg");
            var storage = new PresentImageStorage(CreateEnvironment(webRoot));

            var result = await storage.SaveAsync(image, CancellationToken.None);

            Assert.True(result.Success);
            var imageUrl = Assert.IsType<string>(result.ImageUrl);
            Assert.Matches("^/uploads/presents/[0-9a-f]{32}\\.png$", imageUrl);
            var savedPath = Path.Combine(webRoot, "uploads", "presents", Path.GetFileName(imageUrl));
            Assert.Equal(imageBytes, await File.ReadAllBytesAsync(savedPath));
        }
        finally
        {
            Directory.Delete(webRoot, recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_RejectsUnsupportedFileContent()
    {
        var webRoot = CreateTemporaryDirectory();
        try
        {
            var image = CreateFormFile("<svg></svg>"u8.ToArray(), "gift.png");
            var storage = new PresentImageStorage(CreateEnvironment(webRoot));

            var result = await storage.SaveAsync(image, CancellationToken.None);

            Assert.False(result.Success);
            Assert.Equal("Only JPEG, PNG, and WebP images are supported.", result.ErrorMessage);
            Assert.False(Directory.Exists(Path.Combine(webRoot, "uploads", "presents")));
        }
        finally
        {
            Directory.Delete(webRoot, recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_RejectsFilesLargerThanTheLimit()
    {
        var webRoot = CreateTemporaryDirectory();
        try
        {
            var imageBytes = new byte[checked((int)PresentImageStorage.MaximumFileSizeBytes + 1)];
            imageBytes[0] = 0xFF;
            imageBytes[1] = 0xD8;
            imageBytes[2] = 0xFF;
            var image = CreateFormFile(imageBytes, "gift.jpg");
            var storage = new PresentImageStorage(CreateEnvironment(webRoot));

            var result = await storage.SaveAsync(image, CancellationToken.None);

            Assert.False(result.Success);
            Assert.True(result.IsTooLarge);
            Assert.False(Directory.Exists(Path.Combine(webRoot, "uploads", "presents")));
        }
        finally
        {
            Directory.Delete(webRoot, recursive: true);
        }
    }

    [Fact]
    public void UploadImage_IsAdminOnlyAndHasARequestSizeLimit()
    {
        var method = typeof(PresentController).GetMethod(nameof(PresentController.UploadImageAsync))!;
        var authorization = method.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Single();
        var sizeLimit = method.GetCustomAttributes(typeof(RequestSizeLimitAttribute), inherit: true)
            .Cast<RequestSizeLimitAttribute>()
            .Single();

        Assert.Equal("Admin", authorization.Roles);
        Assert.NotNull(sizeLimit);
    }

    private static FormFile CreateFormFile(byte[] content, string fileName)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, stream.Length, "image", fileName);
    }

    private static string CreateTemporaryDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"ChineseSale-PresentImage-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static IWebHostEnvironment CreateEnvironment(string webRoot) => new TestWebHostEnvironment
    {
        WebRootPath = webRoot,
        ContentRootPath = webRoot,
        WebRootFileProvider = new NullFileProvider(),
        ContentRootFileProvider = new NullFileProvider()
    };

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Project.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
