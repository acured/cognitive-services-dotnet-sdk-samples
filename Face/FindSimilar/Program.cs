using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static List<string> TargetImageFileNames =
            new List<string> { "Family1-Dad1.jpg", "Family1-Daughter1.jpg", "Family1-Mom1.jpg", "Family1-Son1.jpg", "Family2-Lady1.jpg", "Family2-Man1.jpg", "Family3-Lady1.jpg", "Family3-Man1.jpg" };

        public static string SourceImageFileName = "findsimilar.jpg";

        public static void Main(string[] args)
        {
            FindSimilar_in_FaceIds().Wait();
            FindSimilar_in_FaceList().Wait();
            FindSimilar_in_LargeFaceList().Wait();

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }

        public static async Task FindSimilar_in_FaceIds()
        {
            IList<Guid?> targetFaceIds = new List<Guid?>();
            foreach (var targetImageFileName in TargetImageFileNames)
            {
                // Read target image files.
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Detect faces from image stream.
                    IList<DetectedFace> detectedFaces = await Client.Face.DetectWithStreamAsync(stream);
                    if (detectedFaces == null || detectedFaces.Count == 0)
                    {
                        Console.WriteLine($"[Error] No face detected from image `{targetImageFileName}`.");
                        return;
                    }

                    Console.WriteLine($"{detectedFaces.Count} faces detected from image `{targetImageFileName}`.");
                    if (detectedFaces[0].FaceId == null)
                    {
                        Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for recognition purpose.");
                        return;
                    }

                    // Add detected faceId to targetFaceIds.
                    targetFaceIds.Add(detectedFaces[0].FaceId);
                }
            }

            // Read source image file.
            using (FileStream stream = new FileStream(Path.Combine("Images", SourceImageFileName), FileMode.Open))
            {
                // Detect faces from image stream.
                IList<DetectedFace> detectedFaces = await Client.Face.DetectWithStreamAsync(stream);
                if (detectedFaces == null || detectedFaces.Count == 0)
                {
                    Console.WriteLine($"[Error] No face detected from image `{SourceImageFileName}`.");
                    return;
                }

                Console.WriteLine($"{detectedFaces.Count} faces detected from image `{SourceImageFileName}`.");
                if (detectedFaces[0].FaceId == null)
                {
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for recognition purpose.");
                    return;
                }

                // Find similar example of faceId to faceIds.
                IList<SimilarFace> similarResults = await Client.Face.FindSimilarAsync(detectedFaces[0].FaceId.Value, null, null, targetFaceIds);
                if (similarResults.Count == 0)
                {
                    Console.WriteLine($"No similar faces to {SourceImageFileName}.{Environment.NewLine}");
                }

                foreach (var similarResult in similarResults)
                {
                    Console.WriteLine($"Faces from {detectedFaces[0].FaceId} & {similarResult.FaceId} are similar with confidence: {similarResult.Confidence}.");
                }
            }
        }

        public static async Task FindSimilar_in_FaceList()
        {
            // Create a face list.
            string faceListId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create FaceList {faceListId}.");
            await Client.FaceList.CreateAsync(faceListId, "face list for FindSimilar sample", "face list for FindSimilar sample");

            foreach (var targetImageFileName in TargetImageFileNames)
            {
                // Read target image files.
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Add face to face list.
                    var faces = await Client.FaceList.AddFaceFromStreamAsync(faceListId, stream, targetImageFileName);
                    if (faces == null)
                    {
                        Console.WriteLine($"[Error] No face detected from image `{targetImageFileName}`.");
                        return;
                    }

                    Console.WriteLine($"Face from image {targetImageFileName} is successfully added to the face list.");
                }
            }

            // Get persisted faces from the face list.
            List<PersistedFace> persistedFaces = (await Client.FaceList.GetAsync(faceListId)).PersistedFaces.ToList();
            if (persistedFaces.Count == 0)
            {
                Console.WriteLine($"[Error] No persisted face in face list '{ faceListId}'.");
                return;
            }

            // Read source image file.
            using (FileStream stream = new FileStream(Path.Combine("Images", SourceImageFileName), FileMode.Open))
            {
                // Detect faces from image stream.
                IList<DetectedFace> detectedFaces = await Client.Face.DetectWithStreamAsync(stream);
                if (detectedFaces == null || detectedFaces.Count == 0)
                {
                    Console.WriteLine($"[Error] No face detected from image `{SourceImageFileName}`.");
                    return;
                }

                Console.WriteLine($"{detectedFaces.Count} faces detected from image `{SourceImageFileName}`.");
                if (detectedFaces[0].FaceId == null)
                {
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for recognition purpose.");
                    return;
                }

                // Find similar example of faceId to face list.
                var similarResults = await Client.Face.FindSimilarAsync(detectedFaces[0].FaceId.Value, faceListId);
                foreach (var similarResult in similarResults)
                {
                    PersistedFace pf = persistedFaces.Find(p => p.PersistedFaceId == similarResult.PersistedFaceId);
                    if (pf == null)
                    {
                        Console.WriteLine("Persisted face not found in similar result.");
                        continue;
                    }

                    Console.WriteLine($"Faces from {SourceImageFileName} & {pf.UserData} are similar with confidence: {similarResult.Confidence}.");
                }
            }

            // Delete the face list.
            await Client.FaceList.DeleteAsync(faceListId);
            Console.WriteLine($"Delete FaceList {faceListId}.");
        }

        public static async Task FindSimilar_in_LargeFaceList()
        {
            // Create a large face list.
            string largeFaceListId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create large face list {largeFaceListId}.");
            await Client.LargeFaceList.CreateAsync(largeFaceListId, "large face list for FindSimilar sample", "large face list for FindSimilar sample");

            foreach (var targetImageFileName in TargetImageFileNames)
            {
                // Read target image files.
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Add face to the large face list.
                    var faces = await Client.LargeFaceList.AddFaceFromStreamAsync(largeFaceListId, stream, targetImageFileName);
                    if (faces == null)
                    {
                        Console.WriteLine($"[Error] No face detected from image `{targetImageFileName}`.");
                        return;
                    }

                    Console.WriteLine($"Face from image {targetImageFileName} is successfully added to the large face list.");
                }
            }

            // Start to train the large face list.
            Console.WriteLine($"Train large face list {largeFaceListId}.");
            await Client.LargeFaceList.TrainAsync(largeFaceListId);

            // Wait until the training is completed.
            while (true)
            {
                await Task.Delay(1000);
                var trainingStatus = await Client.LargeFaceList.GetTrainingStatusAsync(largeFaceListId);
                Console.WriteLine($"Training status is {trainingStatus.Status}.");
                if (trainingStatus.Status != TrainingStatusType.Running)
                {
                    if (trainingStatus.Status == TrainingStatusType.Failed)
                    {
                        Console.WriteLine($"[Error] Training failed with message {trainingStatus.Message}.");
                        return;
                    }

                    break;
                }
            }

            // Get persisted faces from the large face list.
            List<PersistedFace> persistedFaces = (await Client.LargeFaceList.ListFacesAsync(largeFaceListId)).ToList();
            if (persistedFaces.Count == 0)
            {
                Console.WriteLine($"[Error] No persisted face in large face list '{ largeFaceListId}'.");
                return;
            }

            // // Read source image file.
            using (FileStream stream = new FileStream(Path.Combine("Images", SourceImageFileName), FileMode.Open))
            {
                // Detect faces from image stream.
                IList<DetectedFace> detectedFaces = await Client.Face.DetectWithStreamAsync(stream);
                if (detectedFaces == null || detectedFaces.Count == 0)
                {
                    Console.WriteLine($"[Error] No face detected from image `{SourceImageFileName}`.");
                    return;
                }

                Console.WriteLine($"{detectedFaces.Count} faces detected from image `{SourceImageFileName}`.");
                if (detectedFaces[0].FaceId == null)
                {
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for recognition purpose.");
                    return;
                }

                // Find similar example of faceId to large face list.
                var similarResults = await Client.Face.FindSimilarAsync(detectedFaces[0].FaceId.Value, null, largeFaceListId);
                foreach (var similarResult in similarResults)
                {
                    PersistedFace pf = persistedFaces.Find(p => p.PersistedFaceId == similarResult.PersistedFaceId);
                    if (pf == null)
                    {
                        Console.WriteLine("Persisted face not found in similar result.");
                        continue;
                    }

                    Console.WriteLine($"Faces from {SourceImageFileName} & {pf.UserData} are similar with confidence: {similarResult.Confidence}.");
                }
            }

            // Delete the large face list.
            await Client.LargeFaceList.DeleteAsync(largeFaceListId);
            Console.WriteLine($"Delete LargeFaceList {largeFaceListId}.");
        }
    }
}
