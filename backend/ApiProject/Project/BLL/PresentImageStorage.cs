using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Project.Bll;

public sealed class PresentImageStorage(IWebHostEnvironment environment)
{
    public const long MaximumFileSizeBytes = 5 * 1024 * 1024;

    public async Task<PresentImageStorageResult> SaveAsync(IFormFile image, CancellationToken cancellationToken)
    {
        if (image.Length <= 0)
        {
            return PresentImageStorageResult.Failure("The image file is empty.");
        }

        if (image.Length > MaximumFileSizeBytes)
        {
            return PresentImageStorageResult.Failure("The image file must not exceed 5 MB.", isTooLarge: true);
        }

        await using var input = image.OpenReadStream();
        var header = new byte[12];
        var bytesRead = 0;
        while (bytesRead < header.Length)
        {
            var count = await input.ReadAsync(header.AsMemory(bytesRead), cancellationToken);
            if (count == 0)
            {
                break;
            }

            bytesRead += count;
        }

        var extension = GetImageExtension(header, bytesRead);
        if (extension is null)
        {
            return PresentImageStorageResult.Failure("Only JPEG, PNG, and WebP images are supported.");
        }

        var webRootPath = string.IsNullOrWhiteSpace(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, "wwwroot")
            : environment.WebRootPath;
        var imageDirectory = Path.Combine(webRootPath, "uploads", "presents");
        Directory.CreateDirectory(imageDirectory);

        var fileName = $"{Guid.NewGuid():N}.{extension}";
        var filePath = Path.Combine(imageDirectory, fileName);
        var temporaryPath = $"{filePath}.tmp";
        try
        {
            await using (var output = new FileStream(
                             temporaryPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             bufferSize: 81920,
                             useAsync: true))
            {
                await output.WriteAsync(header.AsMemory(0, bytesRead), cancellationToken);
                await input.CopyToAsync(output, cancellationToken);
            }

            File.Move(temporaryPath, filePath);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }

        return PresentImageStorageResult.Saved($"/uploads/presents/{fileName}");
    }

    private static string? GetImageExtension(byte[] header, int bytesRead)
    {
        if (bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            return "jpg";
        }

        if (bytesRead >= 8 &&
            header.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
        {
            return "png";
        }

        if (bytesRead >= 12 &&
            header.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
            header.AsSpan(8, 4).SequenceEqual("WEBP"u8))
        {
            return "webp";
        }

        return null;
    }
}

public sealed record PresentImageStorageResult(bool Success, string? ImageUrl, string? ErrorMessage, bool IsTooLarge)
{
    public static PresentImageStorageResult Saved(string imageUrl) => new(true, imageUrl, null, false);

    public static PresentImageStorageResult Failure(string errorMessage, bool isTooLarge = false) =>
        new(false, null, errorMessage, isTooLarge);
}
