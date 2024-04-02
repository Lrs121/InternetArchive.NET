namespace InternetArchiveTests;

[TestClass]
public class MetadataTests
{
    private static string _testItem = null!;

    [ClassInitialize()]
    public static async Task ClassInit(TestContext _)
    {
        _testItem = await CreateTestItemAsync();
    }

    [TestMethod]
    public async Task ReadMetadataAsync()
    {
        var response = await _client.Metadata.ReadAsync(_testItem);
        
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.DataNodePrimary);
        Assert.IsNotNull(response.DataNodeSecondary);
        Assert.IsNull(response.DataNodeSolo);
        Assert.IsNotNull(response.DateCreated);
        Assert.IsNotNull(response.DateLastUpdated);
        Assert.IsNotNull(response.Dir);
        Assert.IsNotNull(response.Files);
        Assert.IsNotNull(response.Metadata);
        Assert.IsNotNull(response.Size);
        Assert.IsNotNull(response.Uniq);

        Assert.IsNotNull(response.WorkableServers);
        Assert.IsTrue(response.WorkableServers.Any());
        // Assert.IsNull(response.ServersUnavailable); may be null or not

        Assert.IsTrue(response.Metadata.HasValue);
        var collection = response.Metadata.Value.EnumerateObject().Where(x => x.NameEquals("collection")).SingleOrDefault();
        Assert.IsNotNull(collection);

        var file = response.Files.Where(x => x.Format == "Text" && x.Name == _config.RemoteFilename).SingleOrDefault();

        Assert.IsNotNull(file);
        Assert.IsNotNull(file.Crc32);
        Assert.IsNotNull(file.Format);
        Assert.IsNotNull(file.Md5);
        Assert.IsNotNull(file.ModificationDate);
        Assert.IsNotNull(file.Name);
        Assert.IsNotNull(file.Sha1);
        Assert.IsNotNull(file.Size);
        Assert.IsNotNull(file.Source);
        Assert.IsNotNull(file.VirusCheckDate);
    }

    [TestMethod]
    public async Task WriteMetadataAsync()
    {
        var readResponse1 = await _client.Metadata.ReadAsync(_testItem);

        var patch = new JsonPatchDocument();
        string value;

        if (readResponse1?.Metadata?.TryGetProperty("testkey", out var element) == true)
        {
            value = element.GetString() == "flop" ? "flip" : "flop";
            patch.Replace("/testkey", value);
        }
        else
        {
            value = "flip";
            patch.Add("/testkey", value);
        }

        var writeResponse = await _client.Metadata.WriteAsync(_testItem, patch);
        
        Assert.IsNotNull(writeResponse);
        Assert.IsTrue(writeResponse.Success);
        Assert.IsNull(writeResponse.Error);
        Assert.IsNotNull(writeResponse.Log);
        Assert.IsNotNull(writeResponse.TaskId);

        var readResponse2 = await _client.Metadata.ReadAsync(_testItem);
        Assert.AreEqual(value, readResponse2?.Metadata?.GetProperty("testkey").GetString());
    }
}