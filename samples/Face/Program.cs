namespace Microsoft.Azure.CognitiveServices.Samples.Face
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            // Create a client.
            //string apiKey = "ENTER YOUR KEY HERE";
            //string endpoint = "ENTER YOUR ENDPOINT HERE";
            string apiKey = "793315696cd04d5ab78232a792136610";
            string endpoint = "https://westus.api.cognitive.microsoft.com";

            VerifyFaceToFace.Run(endpoint, apiKey).Wait();
            VerifyInPersonGroup.Run(endpoint, apiKey).Wait();
            VerifyInLargePersonGroup.Run(endpoint, apiKey).Wait();

            Detection.Run(endpoint, apiKey).Wait();

            FindSimilarInFaceIds.Run(endpoint, apiKey).Wait();
            FindSimilarInFaceList.Run(endpoint, apiKey).Wait();
            FindSimilarInLargeFaceList.Run(endpoint, apiKey).Wait();

            Group.Run(endpoint, apiKey).Wait();

            IdentifyInPersonGroup.Run(endpoint, apiKey).Wait();
            IdentifyInLargePersonGroup.Run(endpoint, apiKey).Wait();

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
}
