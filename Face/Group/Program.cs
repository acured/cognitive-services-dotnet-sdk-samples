using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace Group
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create a client.
            string apiKey = "ENTER YOUR KEY HERE";
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                Endpoint = "ENTER YOUR ENDPOINT HERE"
            };

            List<string> imageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg", "Family1-Son1.jpg" };
            Dictionary<string, string> Faces = new Dictionary<string, string>();
            List<Guid> faceIds = new List<Guid>();

            foreach (var imageFileName in imageFileNames)
            {
                // Read image file.
                using (FileStream stream = new FileStream(Path.Combine("Images", imageFileName), FileMode.Open))
                {
                    // Detect faces from image stream.
                    IList<DetectedFace> detectedFaces = client.Face.DetectWithStreamAsync(stream).Result;
                    if (detectedFaces == null || detectedFaces.Count == 0)
                    {
                        Console.WriteLine($"[Error] No face detected from image `{imageFileName}`.");
                        return;
                    }

                    Console.WriteLine($"{detectedFaces.Count} faces detected from image `{imageFileName}`.");
                    if (detectedFaces[0].FaceId == null)
                    {
                        Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for verification purpose.");
                        return;
                    }

                    // Add detected faceId to faceIds and Faces.
                    faceIds.Add(detectedFaces[0].FaceId.Value);
                    Faces.Add(detectedFaces[0].FaceId.ToString(), imageFileName);
                }
            }
            
            // Call grouping, the grouping result is a group collection, each group contains similar faces.
            var groupRes = client.Face.GroupAsync(faceIds).Result;

            // Grouping results.
            foreach (var g in groupRes.Groups)
            {
                Console.Write($"{Environment.NewLine}Find group face ");
                foreach (var fr in g)
                {
                    Console.Write($"{Faces[fr.ToString()]} ");
                }
            }

            // MessyGroup contains all faces which are not similar to any other faces.
            if (groupRes.MessyGroup.Count > 0)
            {
                Console.Write($"{Environment.NewLine}Find messy group face ");
                foreach (var fr in groupRes.MessyGroup)
                {
                    Console.Write($"{Faces[fr.ToString()]} ");
                }
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
}
