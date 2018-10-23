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

            //Verify.
            //Verify_FaceToFace.Run(endpoint, apiKey).Wait();
            //Verify_in_PersonGroup.Run(endpoint, apiKey).Wait();
            //Verify_in_LargePersonGroup.Run(endpoint, apiKey).Wait();

            //// Detection.
            //Detection.Run(endpoint, apiKey).Wait();

            //// Find similar.
            //FindSimilar_in_FaceIds.Run(endpoint, apiKey).Wait();
            //FindSimilar_in_FaceList.Run(endpoint, apiKey).Wait();
            //FindSimilar_in_LargeFaceList.Run(endpoint, apiKey).Wait();

            // Find Identify
            //Identify_in_PersonGroup.Run(endpoint, apiKey).Wait();
            //Identify_in_LargePersonGroup.Run(endpoint, apiKey).Wait();

            // Group
            Group.Run(endpoint, apiKey).Wait();

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
}