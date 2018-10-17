﻿using System;
using System.Collections.Generic;
using System.IO;
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
            FindSimilar_in_FaceIds();
            FindSimilar_in_FaceList();
            FindSimilar_in_LargeFaceList();
            Console.ReadLine();
        }

        public static void FindSimilar_in_FaceIds()
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
                    IList<DetectedFace> detectedFaces = Client.Face.DetectWithStreamAsync(stream).Result;
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

                    // Add detected faceId to faceIds.
                    faceIds.Add(detectedFaces[0].FaceId);
                }
            }

            // Find similar example for find similar faces from faceIds with similar confidence.
            IList<SimilarFace> similarResults = Client.Face.FindSimilarAsync(faceIds[0].Value, null, null, faceIds).Result;
            foreach (var similarResult in similarResults)
            {
                Console.WriteLine($"Faces from {faceIds[0].Value} & {similarResult.FaceId} are of the same (Positive) person, similarity confidence: {similarResult.Confidence}.");
            }

            Console.WriteLine("\nPress ENTER to exit.");
        }

        public static async void FindSimilar_in_FaceList()
        {
            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg", "Family1-Son1.jpg" };
            string sourceImageFileName = "Family1-Dad1.jpg";

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
                    Console.WriteLine($"add faces to face list from image `{targetImageFileName}`.");
                    var faces = Client.FaceList.AddFaceFromStreamAsync(faceListId, stream, targetImageFileName).Result;
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
                    Console.WriteLine($"Faces from {sourceImageFileName} & {persistedFaces.Find(p => p.PersistedFaceId == similarResult.PersistedFaceId).UserData} are of the same (Positive) person, similarity confidence: {similarResult.Confidence}.");
                }
            }

            // Delete the face list.
            await Client.FaceList.DeleteAsync(faceListId);
            Console.WriteLine($"{Environment.NewLine}Delete FaceList {faceListId}");

            Console.WriteLine("\nPress ENTER to exit.");
        }

        public static async void FindSimilar_in_LargeFaceList()
        {
            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg", "Family1-Son1.jpg" };
            string sourceImageFileName = "Family1-Dad1.jpg";

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
                    Console.WriteLine($"add faces to large face list from image `{targetImageFileName}`.");
                    var faces = Client.LargeFaceList.AddFaceFromStreamAsync(largeFaceListId, stream, targetImageFileName).Result;
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
                    Console.WriteLine($"Faces from {sourceImageFileName} & {persistedFaces.Find(p => p.PersistedFaceId == similarResult.PersistedFaceId).UserData} are of the same (Positive) person, similarity confidence: {similarResult.Confidence}.");
                }
            }

            // Delete the large face list.
            await Client.LargeFaceList.DeleteAsync(largeFaceListId);
            Console.WriteLine($"{Environment.NewLine}Delete FaceList {largeFaceListId}");

            Console.WriteLine("\nPress ENTER to exit.");
        }
    }
}
