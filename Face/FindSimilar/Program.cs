using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace FindSimilar
{
    public class Program
    {
        // Create a client.
        public static string ApiKey = "ENTER YOUR KEY HERE";

        public static IFaceClient Client = new FaceClient(new ApiKeyServiceClientCredentials(ApiKey))
        {
            Endpoint = "ENTER YOUR ENDPOINT HERE"
        };

        public static void Main(string[] args)
        {
            FindSimilar_in_FaceIds().Wait();
            FindSimilar_in_FaceList().Wait();
            FindSimilar_in_LargeFaceList().Wait();
            Console.ReadLine();
        }

        public static async Task FindSimilar_in_FaceIds()
        {
            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Daughter1.jpg", "Family1-Mom1.jpg", "Family1-Son1.jpg", "Family2-Lady1.jpg", "Family2-Man1.jpg", "Family3-Lady1.jpg", "Family3-Man1.jpg" };
            IList<Guid?> faceIds = new List<Guid?>();

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Read image file.
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Detect faces from image stream.
                    IList<DetectedFace> detectedFaces = Client.Face.DetectWithStreamAsync(stream).Result;
                    if (detectedFaces == null || detectedFaces.Count == 0)
                    {
                        Console.WriteLine($"[Error] No face detected from image `{targetImageFileName}`.");
                        return;
                    }

                    Console.WriteLine($"{detectedFaces.Count} faces detected from image `{targetImageFileName}`.");
                    if (detectedFaces[0].FaceId == null)
                    {
                        Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for verification purpose.");
                        return;
                    }

                    // Add detected faceId to faceIds.
                    faceIds.Add(detectedFaces[0].FaceId);
                }
            }

            // Find similar example for find similar faces from faceIds with similar confidence.
            IList<SimilarFace> similarResults = Client.Face.FindSimilarAsync(faceIds[0].Value, null, null, faceIds).Result;
            if (similarResults.Count == 0)
            {
                Console.WriteLine($"Not found the same with `{faceIds[0].Value}` from similar result.");
            }
            foreach (var similarResult in similarResults)
            {
                Console.WriteLine(
                    similarResult.Confidence>0.5
                        ? $"Faces from {faceIds[0].Value} & {similarResult.FaceId} are of the same (Positive) person, similarity confidence: {similarResult.Confidence}."
                        : $"Faces from {faceIds[0].Value} & {similarResult.FaceId} are of different (Negative) persons, similarity confidence: {similarResult.Confidence}.");
            }

            Console.WriteLine("\nPress ENTER to exit.");
        }

        public static async Task FindSimilar_in_FaceList()
        {
            List<string> targetImageFileNames =
                            new List<string> { "Family1-Dad1.jpg", "Family1-Daughter1.jpg", "Family1-Mom1.jpg", "Family1-Son1.jpg", "Family2-Lady1.jpg", "Family2-Man1.jpg", "Family3-Lady1.jpg", "Family3-Man1.jpg" };
            string sourceImageFileName = "findsimilar.jpg";

            // Create a face list.
            string faceListId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create FaceList {faceListId} {Environment.NewLine}");
            await Client.FaceList.CreateAsync(faceListId, faceListId, "face list for sample");

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Read image file.
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Add face to face list.
                    Console.WriteLine($"Add faces to face list from image `{targetImageFileName}`.");
                    var faces = await Client.FaceList.AddFaceFromStreamAsync(faceListId, stream, targetImageFileName);
                    if (faces == null)
                    {
                        Console.WriteLine($"[Error] No persisted face from image `{targetImageFileName}`.");
                        return;
                    }
                }
            }

            // Get persisted faces.
            List<PersistedFace> persistedFaces = Client.FaceList.GetAsync(faceListId).Result.PersistedFaces as List<PersistedFace>;

            // Read image file.
            using (FileStream stream = new FileStream(Path.Combine("Images", sourceImageFileName), FileMode.Open))
            {
                // Detect faces from image stream.
                IList<DetectedFace> detectedFaces = Client.Face.DetectWithStreamAsync(stream).Result;
                if (detectedFaces == null || detectedFaces.Count == 0)
                {
                    Console.WriteLine($"[Error] No face detected from image `{sourceImageFileName}`.");
                    return;
                }

                Console.WriteLine($"{detectedFaces.Count} faces detected from image `{sourceImageFileName}`.");
                if (detectedFaces[0].FaceId == null)
                {
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for verification purpose.");
                    return;
                }

                // Find similar example for find similar faces from the face list with similar confidence.
                var similarResults = Client.Face.FindSimilarAsync(detectedFaces[0].FaceId.Value, faceListId).Result;
                foreach (var similarResult in similarResults)
                {
                    PersistedFace pf = persistedFaces.Find(p => p.PersistedFaceId == similarResult.PersistedFaceId);
                    if (pf == null)
                    {
                        Console.WriteLine("Persisted face not found in similar result.");
                        continue;
                    }
                    Console.WriteLine($"Faces from {sourceImageFileName} & {pf.UserData} are of the same (Positive) person, similarity confidence: {similarResult.Confidence}.");
                    Console.WriteLine(
                        similarResult.Confidence > 0.5
                            ? $"Faces from {sourceImageFileName} & {pf.UserData} are of the same (Positive) person, similarity confidence: {similarResult.Confidence}."
                            : $"Faces from {sourceImageFileName} & {pf.UserData} are of different (Negative) persons, similarity confidence: {similarResult.Confidence}.");
                }
            }

            // Delete the face list.
            await Client.FaceList.DeleteAsync(faceListId);
            Console.WriteLine($"{Environment.NewLine}Delete FaceList {faceListId}");

            Console.WriteLine("\nPress ENTER to exit.");
        }

        public static async Task FindSimilar_in_LargeFaceList()
        {
            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Daughter1.jpg", "Family1-Mom1.jpg", "Family1-Son1.jpg", "Family2-Lady1.jpg", "Family2-Man1.jpg", "Family3-Lady1.jpg", "Family3-Man1.jpg" };
            string sourceImageFileName = "findsimilar.jpg";

            // Create a large face list.
            string largeFaceListId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create large face list {largeFaceListId} {Environment.NewLine}");
            await Client.LargeFaceList.CreateAsync(largeFaceListId, largeFaceListId, "large face list for sample");

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Read image file.
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Add face to the large face list.
                    Console.WriteLine($"Add faces to large face list from image `{targetImageFileName}`.");
                    var faces = await Client.LargeFaceList.AddFaceFromStreamAsync(largeFaceListId, stream, targetImageFileName);
                    if (faces == null)
                    {
                        Console.WriteLine($"[Error] No persisted face from image `{targetImageFileName}`.");
                        return;
                    }
                }
            }

            // Start to train the large face list.
            Console.WriteLine($"Train large face list {largeFaceListId} {Environment.NewLine}");
            await Client.LargeFaceList.TrainAsync(largeFaceListId);
            // Wait until train completed 
            while (true)
            {
                await Task.Delay(1000);
                var status = await Client.LargeFaceList.GetTrainingStatusAsync(largeFaceListId);              
                Console.WriteLine($"Response: Success. Group \"{largeFaceListId}' training process is {status.Status}");
                if (status.Status != TrainingStatusType.Running)  
                {
                    break;
                }
            }

            // Get persisted faces.
            List<PersistedFace> persistedFaces = Client.LargeFaceList.ListFacesAsync(largeFaceListId).Result as List<PersistedFace>;

            // Read image file.
            using (FileStream stream = new FileStream(Path.Combine("Images", sourceImageFileName), FileMode.Open))
            {
                // Detect faces from image stream.
                IList<DetectedFace> detectedFaces = Client.Face.DetectWithStreamAsync(stream).Result;
                if (detectedFaces == null || detectedFaces.Count == 0)
                {
                    Console.WriteLine($"[Error] No face detected from image `{sourceImageFileName}`.");
                    return;
                }

                Console.WriteLine($"{detectedFaces.Count} faces detected from image `{sourceImageFileName}`.");
                if (detectedFaces[0].FaceId == null)
                {
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for verification purpose.");
                    return;
                }

                // Find similar example for find similar faces from the large face list with similar confidence.
                var similarResults = Client.Face.FindSimilarAsync(detectedFaces[0].FaceId.Value, null, largeFaceListId).Result;
                foreach (var similarResult in similarResults)
                {
                    PersistedFace pf = persistedFaces.Find(p => p.PersistedFaceId == similarResult.PersistedFaceId);
                    if (pf == null)
                    {
                        Console.WriteLine("Persisted face not found in similar result.");
                        continue;
                    }
                    Console.WriteLine($"Faces from {sourceImageFileName} & {pf.UserData} are of the same (Positive) person, similarity confidence: {similarResult.Confidence}.");
                    Console.WriteLine(
                        similarResult.Confidence > 0.5
                            ? $"Faces from {sourceImageFileName} & {pf.UserData} are of the same (Positive) person, similarity confidence: {similarResult.Confidence}."
                            : $"Faces from {sourceImageFileName} & {pf.UserData} are of different (Negative) persons, similarity confidence: {similarResult.Confidence}.");
                }
            }

            // Delete the large face list.
            await Client.LargeFaceList.DeleteAsync(largeFaceListId);
            Console.WriteLine($"{Environment.NewLine}Delete FaceList {largeFaceListId}");

            Console.WriteLine("\nPress ENTER to exit.");
        }
    }
}
