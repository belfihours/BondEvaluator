using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BondEvaluator.API.IntegrationTest;

public class BondEvaluatorControllerTest
{
    private const string CorrectCsvTetsPath = "./TestData/bond_positions_sample.csv";
    private const string CorruptedCsvTetsPath = "./TestData/bond_positions_sample_corrupted.csv";
    
    [Fact]
    public async Task WhenFileIsCorrect_ThenReturnsResult()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        var file = await File.ReadAllTextAsync(CorrectCsvTetsPath);
        var content = new MultipartFormDataContent();
        var csvBytes = Encoding.UTF8.GetBytes(file);
        var fileContent = new ByteArrayContent(csvBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");
        content.Add(fileContent, "file", "test.csv");

        // Act
        var response = await client.PostAsync("/bondevaluator/bondevaluations", content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
    }
    
    [Fact]
    public async Task WhenFileIsCorrect_ThenReturnsBadRequest()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        var file = await File.ReadAllTextAsync(CorruptedCsvTetsPath);
        var content = new MultipartFormDataContent();
        var csvBytes = Encoding.UTF8.GetBytes(file);
        var fileContent = new ByteArrayContent(csvBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");
        content.Add(fileContent, "file", "test.csv");

        // Act
        var response = await client.PostAsync("/bondevaluator/bondevaluations", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}