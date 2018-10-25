using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace Group
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Group().Wait();

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }

        public static async Task Group()
        {
            Console.WriteLine("Sample of grouping faces.");

            // Create a client.
            string apiKey = "ENTER YOUR KEY HERE";
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                Endpoint = "ENTER YOUR ENDPOINT HERE"
            };

            List<string> imageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg", "Family3-Lady1.jpg", "Family1-Daughter1.jpg", "Family1-Daughter2.jpg", "Family1-Daughter3.jpg" };
            Dictionary<string, string> faces = new Dictionary<string, string>();
            List<Guid> faceIds = new List<Guid>();

            foreach (var imageFileName in imageFileNames)
            {
                // Read image file.
                using (FileStream stream = new FileStream(Path.Combine("Images", imageFileName), FileMode.Open))
                {
                    // Detect faces from image stream.
                    IList<DetectedFace> detectedFaces = await client.Face.DetectWithStreamAsync(stream);
                    if (detectedFaces == null || detectedFaces.Count == 0)
                    {
                        Console.WriteLine($"[Error] No face detected from image `{imageFileName}`.");
                        return;
                    }

                    Console.WriteLine($"{detectedFaces.Count} faces detected from image `{imageFileName}`.");
                    if (detectedFaces[0].FaceId == null)
                    {
                        Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for grouping purpose.");
                        return;
                    }

                    // Add detected faceId to faceIds and faces.
                    faceIds.Add(detectedFaces[0].FaceId.Value);
                    faces.Add(detectedFaces[0].FaceId.ToString(), imageFileName);
                }
            }

            // Call grouping, the grouping result is a group collection, each group contains similar faces.
            var groupResult = await client.Face.GroupAsync(faceIds);

            // Grouping results.
            for (int i = 0; i < groupResult.Groups.Count; i++)
            {
                Console.Write($"Find face group {i + 1}: ");
                foreach (var faceId in groupResult.Groups[i])
                {
                    Console.Write($"{faces[faceId.ToString()]} ");
                }

                Console.WriteLine();
            }

            // MessyGroup contains all faces which are not similar to any other faces.
            if (groupResult.MessyGroup.Count > 0)
            {
                Console.Write("Find messy face group: ");
                foreach (var faceId in groupResult.MessyGroup)
                {
                    Console.Write($"{faces[faceId.ToString()]} ");
                }
            }

            Console.WriteLine();
        }
    }
}
