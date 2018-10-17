using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace Verify
{
    public class Program
    {
        // Create a client. 
        static string ApiKey = "ENTER YOUR KEY HERE";
        static IFaceClient Client = new FaceClient(new ApiKeyServiceClientCredentials(ApiKey))
        {
            Endpoint = "ENTER YOUR ENDPOINT HERE"
        };

        public static void Main(string[] args)
        {
            Verify_FaceToFace().Wait();
            Verify_in_PersonGroup().Wait();
            Verify_in_LargePersonGroup().Wait();
            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }

        public static async Task Verify_FaceToFace()
        {
            List<string> imageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg", "Family1-Son1.jpg" };
            List<Guid> faceIds = new List<Guid>();

            foreach (var imageFileName in imageFileNames)
            {
                // Read image file.
                using (FileStream stream = new FileStream(Path.Combine("Images", imageFileName), FileMode.Open))
                {
                    // Detect faces from image stream.
                    IList<DetectedFace> detectedFaces = await Client.Face.DetectWithStreamAsync(stream);
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

                    faceIds.Add(detectedFaces[0].FaceId.Value);
                }
            }

            // Verification example for faces of the same person.
            VerifyResult verifyResult1 = await Client.Face.VerifyFaceToFaceAsync(faceIds[0], faceIds[1]);
            Console.WriteLine(
                verifyResult1.IsIdentical
                    ? $"Faces from {imageFileNames[0]} & {imageFileNames[1]} are of the same (Positive) person, similarity confidence: {verifyResult1.Confidence}."
                    : $"Faces from {imageFileNames[0]} & {imageFileNames[1]} are of different (Negative) persons, similarity confidence: {verifyResult1.Confidence}.");

            // Verification example for faces of different persons.
            VerifyResult verifyResult2 = await Client.Face.VerifyFaceToFaceAsync(faceIds[1], faceIds[2]);
            Console.WriteLine(
                verifyResult2.IsIdentical
                    ? $"Faces from {imageFileNames[1]} & {imageFileNames[2]} are of the same (Negative) person, similarity confidence: {verifyResult2.Confidence}."
                    : $"Faces from {imageFileNames[1]} & {imageFileNames[2]} are of different (Positive) persons, similarity confidence: {verifyResult2.Confidence}.");
        }

        public static async Task Verify_in_PersonGroup()
        {
            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg" };
            string sourceImageFileName = "Family1-Dad3.jpg";

            // Create a person group.
            string GroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a person group ({GroupId}) {Environment.NewLine}");
            await Client.PersonGroup.CreateAsync(GroupId, GroupId);

            // Create a person group person.
            Person p = new Person();
            p.Name = "Dad";
            p.UserData = "Person for sample";
            Console.WriteLine($"Create a person group person '{p.Name}' {Environment.NewLine}");
            p.PersonId = (await Client.PersonGroupPerson.CreateAsync(GroupId, p.Name)).PersonId;

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Read image file. 
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Add face to the person group. 
                    Console.WriteLine($"Add faces to the person group from image `{targetImageFileName}`.");
                    PersistedFace faces = await Client.PersonGroupPerson.AddFaceFromStreamAsync(GroupId, p.PersonId, stream, targetImageFileName);

                    if (faces == null)
                    {
                        Console.WriteLine($"[Error] No persisted face from image `{targetImageFileName}`.");
                        return;
                    }
                }
            }

            // Start to train the person group.
            await Client.PersonGroup.TrainAsync(GroupId);
            // Wait until train completed. 
            while (true)
            {
                await Task.Delay(1000);
                var status = await Client.PersonGroup.GetTrainingStatusAsync(GroupId);
                Console.WriteLine($"Response: Success. Group \"{GroupId}' training process is {status.Status}");
                if (status.Status != TrainingStatusType.Running)
                {
                    break;
                }
            }

            List<Guid> faceIds = new List<Guid>();
            // Read image file. 
            using (FileStream stream = new FileStream(Path.Combine("Images", sourceImageFileName), FileMode.Open))
            {
                // Detect faces from image stream. 
                IList<DetectedFace> detectedFaces = await Client.Face.DetectWithStreamAsync(stream);
                if (detectedFaces == null || detectedFaces.Count == 0)
                {
                    Console.WriteLine($"[Error] No face detected from image `{sourceImageFileName}`.");
                    return;
                }

                Console.WriteLine($"{detectedFaces.Count} faces detected from image `{sourceImageFileName}`.");
                if (detectedFaces[0].FaceId == null)
                {
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for Identify purpose.");
                    return;
                }

                // Add detected faceId to faceIds.
                faceIds.Add(detectedFaces[0].FaceId.Value);
            }

            // Verification example for faces of the same person.
            VerifyResult VerifyResults = await Client.Face.VerifyFaceToPersonAsync(faceIds[0], p.PersonId, GroupId);
            Console.WriteLine(
                VerifyResults.IsIdentical
                    ? $"Faces from {sourceImageFileName} & {p.Name} are of the same (Positive) person, similarity confidence: {VerifyResults.Confidence}."
                    : $"Faces from {sourceImageFileName} & {p.Name} are of different (Negative) persons, similarity confidence: {VerifyResults.Confidence}.");

            // Delete the person group.
            Console.WriteLine($"{Environment.NewLine}Delete the person group ({GroupId})");
            await Client.PersonGroup.DeleteAsync(GroupId);
        }

        public static async Task Verify_in_LargePersonGroup()
        {
            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg" };
            string sourceImageFileName = "Family1-Dad3.jpg";

            // Create a large person group.
            string GroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a large person group ({GroupId}) {Environment.NewLine}");
            await Client.LargePersonGroup.CreateAsync(GroupId, GroupId);

            // Create a large person group person.
            Person p = new Person();
            p.Name = "Dad";
            p.UserData = "Person for sample";
            Console.WriteLine($"Create a large person group person '{p.Name}' {Environment.NewLine}");
            p.PersonId = (await Client.LargePersonGroupPerson.CreateAsync(GroupId, p.Name)).PersonId;

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Read image file. 
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Add face to the large person group. 
                    Console.WriteLine($"Add faces to the large person group from image `{targetImageFileName}`.");
                    PersistedFace faces = await Client.LargePersonGroupPerson.AddFaceFromStreamAsync(GroupId, p.PersonId, stream, targetImageFileName);

                    if (faces == null)
                    {
                        Console.WriteLine($"[Error] No persisted face from image `{targetImageFileName}`.");
                        return;
                    }
                }
            }

            // Start to train the large person group.
            await Client.LargePersonGroup.TrainAsync(GroupId);
            // Wait until train completed.
            while (true)
            {
                await Task.Delay(1000);
                var status = await Client.LargePersonGroup.GetTrainingStatusAsync(GroupId);
                Console.WriteLine($"Response: Success. Group \"{GroupId}' training process is {status.Status}");
                if (status.Status != TrainingStatusType.Running)
                {
                    break;
                }
            }

            List<Guid> faceIds = new List<Guid>();
            // Read image file. 
            using (FileStream stream = new FileStream(Path.Combine("Images", sourceImageFileName), FileMode.Open))
            {
                // Detect faces from image stream. 
                IList<DetectedFace> detectedFaces = await Client.Face.DetectWithStreamAsync(stream);
                if (detectedFaces == null || detectedFaces.Count == 0)
                {
                    Console.WriteLine($"[Error] No face detected from image `{sourceImageFileName}`.");
                    return;
                }

                Console.WriteLine($"{detectedFaces.Count} faces detected from image `{sourceImageFileName}`.");
                if (detectedFaces[0].FaceId == null)
                {
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for Identify purpose.");
                    return;
                }

                // Add detected faceId to faceIds.
                faceIds.Add(detectedFaces[0].FaceId.Value);
            }

            // Verification example for faces of the same person.
            VerifyResult VerifyResults = await Client.Face.VerifyFaceToPersonAsync(faceIds[0], p.PersonId, null, GroupId);
            Console.WriteLine(
                VerifyResults.IsIdentical
                    ? $"Faces from {sourceImageFileName} & {p.Name} are of the same (Positive) person, similarity confidence: {VerifyResults.Confidence}."
                    : $"Faces from {sourceImageFileName} & {p.Name} are of different (Negative) persons, similarity confidence: {VerifyResults.Confidence}.");

            // Delete the large person group.
            Console.WriteLine($"{Environment.NewLine}Delete the large person group ({GroupId})");
            await Client.LargePersonGroup.DeleteAsync(GroupId);
        }
    }
}
