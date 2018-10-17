using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace FindSimilar
{
    public class Program
    {
        // Create a client.
        static string _apiKey = "ENTER YOUR KEY HERE";
        static IFaceClient _client = new FaceClient(new ApiKeyServiceClientCredentials(_apiKey))
        {
            Endpoint = "ENTER YOUR ENDPOINT HERE"
        };

        static void Main(string[] args)
        {
            FindSimilar_in_FaceIds();
            //FindSimilar_in_FaceList();
            //FindSimilar_in_LargeFaceList();
            Console.ReadLine();
        }

        static void FindSimilar_in_FaceIds()
        {
            List<string> imageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg", "Family1-Son1.jpg" };
            IList<Guid?> faceIds = new List<Guid?>();

            foreach (var imageFileName in imageFileNames)
            {
                // Read image file.
                using (FileStream stream = new FileStream(Path.Combine("Images", imageFileName), FileMode.Open))
                {
                    // Detect faces from image stream.
                    IList<DetectedFace> detectedFaces = _client.Face.DetectWithStreamAsync(stream).Result;
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

                    // add face to faceIDs.
                    faceIds.Add(detectedFaces[0].FaceId);
                }
            }

            // Find similar example for find similar faces from faceids with similar confidence.
            IList<SimilarFace> similarResults = _client.Face.FindSimilarAsync(faceIds[0].Value, null, null, faceIds).Result;
            foreach (var similarResult in similarResults)
            {
                Console.WriteLine($"Faces from {faceIds[0].Value} & {similarResult.FaceId} are of the same (Positive) person, similarity confidence: {similarResult.Confidence}.");
            }

            Console.WriteLine("\nPress ENTER to exit.");
        }

        static async void FindSimilar_in_FaceList()
        {
            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg", "Family1-Son1.jpg" };
            string sourceImageFileName = "Family1-Dad1.jpg";

            //create facelist
            string faceListId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create FaceList {faceListId} {Environment.NewLine}");
            await _client.FaceList.CreateAsync(faceListId, faceListId, "face list for sample");

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Read image file.
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // add face to faceList.
                    Console.WriteLine($"add faces to face list from image `{targetImageFileName}`.");
                    var faces = _client.FaceList.AddFaceFromStreamAsync(faceListId, stream, targetImageFileName).Result;
                    if (faces == null)
                    {
                        Console.WriteLine($"[Error] No persisted face from image `{targetImageFileName}`.");
                        return;
                    }
                }
            }

            //Get persisted faces
            List<PersistedFace> PersistedFaces = _client.FaceList.GetAsync(faceListId).Result.PersistedFaces as List<PersistedFace>;

            // Read image file.
            using (FileStream stream = new FileStream(Path.Combine("Images", sourceImageFileName), FileMode.Open))
            {
                // Detect faces from image stream.
                IList<DetectedFace> detectedFaces = _client.Face.DetectWithStreamAsync(stream).Result;
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

                // Find similar example for find similar faces from faceList with similar confidence.
                var similarResults = _client.Face.FindSimilarAsync(detectedFaces[0].FaceId.Value, faceListId).Result;
                foreach (var similarResult in similarResults)
                {
                    Console.WriteLine($"Faces from {sourceImageFileName} & {PersistedFaces.Find(p => p.PersistedFaceId == similarResult.PersistedFaceId).UserData} are of the same (Positive) person, similarity confidence: {similarResult.Confidence}.");
                }
            }

            //delete facelist
            await _client.FaceList.DeleteAsync(faceListId);
            Console.WriteLine($"{Environment.NewLine}Delete FaceList {faceListId}");

            Console.WriteLine("\nPress ENTER to exit.");
        }

        static async void FindSimilar_in_LargeFaceList()
        {
            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg", "Family1-Son1.jpg" };
            string sourceImageFileName = "Family1-Dad1.jpg";

            //create facelist
            string LargeFaceListId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create large face list {LargeFaceListId} {Environment.NewLine}");
            await _client.LargeFaceList.CreateAsync(LargeFaceListId, LargeFaceListId, "large face list for sample");

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Read image file.
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // add face to faceList.
                    Console.WriteLine($"add faces to large face list from image `{targetImageFileName}`.");
                    var faces = _client.LargeFaceList.AddFaceFromStreamAsync(LargeFaceListId, stream, targetImageFileName).Result;
                    if (faces == null)
                    {
                        Console.WriteLine($"[Error] No persisted face from image `{targetImageFileName}`.");
                        return;
                    }
                }
            }

            // Start train large face list.
            Console.WriteLine($"Train large face list {LargeFaceListId} {Environment.NewLine}");
            await _client.LargeFaceList.TrainAsync(LargeFaceListId);

            // Get persisted faces
            List<PersistedFace> PersistedFaces = _client.LargeFaceList.ListFacesAsync(LargeFaceListId).Result as List<PersistedFace>;

            // Read image file.
            using (FileStream stream = new FileStream(Path.Combine("Images", sourceImageFileName), FileMode.Open))
            {
                // Detect faces from image stream.
                IList<DetectedFace> detectedFaces = _client.Face.DetectWithStreamAsync(stream).Result;
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

                // Find similar example for find similar faces from faceList with similar confidence.
                var similarResults = _client.Face.FindSimilarAsync(detectedFaces[0].FaceId.Value, null , LargeFaceListId).Result;
                foreach (var similarResult in similarResults)
                {
                    Console.WriteLine($"Faces from {sourceImageFileName} & {PersistedFaces.Find(p => p.PersistedFaceId == similarResult.PersistedFaceId).UserData} are of the same (Positive) person, similarity confidence: {similarResult.Confidence}.");
                }
            }

            //delete facelist
            await _client.LargeFaceList.DeleteAsync(LargeFaceListId);
            Console.WriteLine($"{Environment.NewLine}Delete FaceList {LargeFaceListId}");

            Console.WriteLine("\nPress ENTER to exit.");
        }
    }
}
