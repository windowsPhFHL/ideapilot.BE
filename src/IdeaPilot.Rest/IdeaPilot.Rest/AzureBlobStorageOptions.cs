namespace IdeaPilot.Rest
{
    internal class AzureBlobStorageOptions
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public string BlobName { get; set; } = string.Empty;
        public string BlobUri { get; set; } = string.Empty;
        public string BlobSasToken { get; set; } = string.Empty;
    }
}