using Azure.Storage.Blobs;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace IdeaPilot.Rest.Services
{
    public class PdfService
    {
        private readonly IConfiguration _configuration;

        public PdfService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GeneratePdfAsync(string content, string fileName)
        {
            // 1. Generate PDF locally
            var converter = new SynchronizedConverter(new PdfTools());
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = { PaperSize = DinkToPdf.PaperKind.A4 },
                Objects = { new ObjectSettings() { HtmlContent = content } }
            };

            byte[] pdf = converter.Convert(doc);

            // 2. Upload to Azure Blob Storage 🔥🔥
            string blobUrl = await UploadPdfToBlobAsync(pdf, fileName);

            return blobUrl;
        }

        private async Task<string> UploadPdfToBlobAsync(byte[] pdf, string fileName)
        {
            string connectionString = _configuration["AzureBlobStorage:ConnectionString"];
            string containerName = _configuration["AzureBlobStorage:ContainerName"];

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Make sure the container exists
            await containerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = new MemoryStream(pdf))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.AbsoluteUri;
        }
    }
}
