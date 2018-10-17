using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace Verify
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create a client.
            string apiKey = "793315696cd04d5ab78232a792136610";
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                Endpoint = "https://westus.api.cognitive.microsoft.com"
            };
        }
    }
}
