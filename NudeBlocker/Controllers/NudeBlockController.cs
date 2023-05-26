using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace NudeBlocker.Controllers;

public class NudeBlockerController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public NudeBlockerController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    [Route("[controller]")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckPhoto(IFormFile photo)
    {
        // check if photo is null or empty
        if (photo == null || photo.Length == 0)
        {
            return BadRequest("Please upload a photo.");
        }

        // analyze the photo using the Computer Vision API
        using (var stream = new MemoryStream())
        {
            await photo.CopyToAsync(stream);
            stream.Position = 0;

            var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(_configuration["AzureComputerVisionKey"]))
            {
                Endpoint = _configuration["AzureComputerVisionEndpoint"]
            };

            System.Collections.Generic.IList<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Adult
            };

            var result = await client.AnalyzeImageInStreamAsync(stream, features);

            // check if the photo contains adult or racy content
            if (result.Adult.IsAdultContent || result.Adult.IsRacyContent)
            {
                return BadRequest("The uploaded photo contains inappropriate content.");
            }
        }

        // proceed with user registration
        // ...
        return Ok("Clean photo");
    }

}
