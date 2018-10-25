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
        public static string ApiKey = "ENTER YOUR KEY HERE";

        public static IFaceClient Client = new FaceClient(new ApiKeyServiceClientCredentials(ApiKey))
        {
            Endpoint = "ENTER YOUR ENDPOINT HERE"
        };

        public static List<string> TargetImageFileNames =
            new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg" };
        
        public static string SourceImageFileName1 = "Family1-Dad3.jpg";

        public static string SourceImageFileName2 = "Family1-Son1.jpg";

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
            Console.WriteLine("Sample of verify face to face.");

            List<Guid> targetFaceIds = new List<Guid>();
            Guid sourceFaceId1;
            Guid sourceFaceId2;

            foreach (var imageFileName in TargetImageFileNames)
            {
                // Read target image files.
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

                    targetFaceIds.Add(detectedFaces[0].FaceId.Value);
                }
            }

            // Read source image file 1.
            using (FileStream stream = new FileStream(Path.Combine("Images", SourceImageFileName1), FileMode.Open))
            {
                // Detect faces from image stream.
                IList<DetectedFace> detectedFaces = await Client.Face.DetectWithStreamAsync(stream);
                if (detectedFaces == null || detectedFaces.Count == 0)
                {
                    Console.WriteLine($"[Error] No face detected from image `{SourceImageFileName1}`.");
                    return;
                }

                Console.WriteLine($"{detectedFaces.Count} faces detected from image `{SourceImageFileName1}`.");
                if (detectedFaces[0].FaceId == null)
                {
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for verification purpose.");
                    return;
                }

                sourceFaceId1 = detectedFaces[0].FaceId.Value;
            }

            // Read source image file 2.
            using (FileStream stream = new FileStream(Path.Combine("Images", SourceImageFileName2), FileMode.Open))
            {
                // Detect faces from image stream.
                IList<DetectedFace> detectedFaces = await Client.Face.DetectWithStreamAsync(stream);
                if (detectedFaces == null || detectedFaces.Count == 0)
                {
                    Console.WriteLine($"[Error] No face detected from image `{SourceImageFileName2}`.");
                    return;
                }

                Console.WriteLine($"{detectedFaces.Count} faces detected from image `{SourceImageFileName2}`.");
                if (detectedFaces[0].FaceId == null)
                {
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for verification purpose.");
                    return;
                }

                sourceFaceId2 = detectedFaces[0].FaceId.Value;
            }
            
            // Verification example for faces of the same person.
            VerifyResult verifyResult1 = await Client.Face.VerifyFaceToFaceAsync(sourceFaceId1, targetFaceIds[0]);
            Console.WriteLine(
                verifyResult1.IsIdentical
                    ? $"Faces from {SourceImageFileName1} & {TargetImageFileNames[0]} are of the same (Positive) person, similarity confidence: {verifyResult1.Confidence}."
                    : $"Faces from {SourceImageFileName1} & {TargetImageFileNames[0]} are of different (Negative) persons, similarity confidence: {verifyResult1.Confidence}.");

            // Verification example for faces of different persons.
            VerifyResult verifyResult2 = await Client.Face.VerifyFaceToFaceAsync(sourceFaceId2, targetFaceIds[0]);
            Console.WriteLine(
                verifyResult2.IsIdentical
                    ? $"Faces from {SourceImageFileName2} & {TargetImageFileNames[0]} are of the same (Negative) person, similarity confidence: {verifyResult2.Confidence}."
                    : $"Faces from {SourceImageFileName2} & {TargetImageFileNames[0]} are of different (Positive) persons, similarity confidence: {verifyResult2.Confidence}.");

            Console.WriteLine();
        }

        public static async Task Verify_in_PersonGroup()
        {
            Console.WriteLine("Sample of verify face to person group.");

            // Create a person group.
            string personGroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a person group ({personGroupId}).");
            await Client.PersonGroup.CreateAsync(personGroupId, personGroupId);

            // Create a person group person.
            Person p = new Person { Name = "Dad", UserData = "Person for sample" };
            Console.WriteLine($"Create a person group person '{p.Name}'.");
            p.PersonId = (await Client.PersonGroupPerson.CreateAsync(personGroupId, p.Name)).PersonId;

            foreach (var targetImageFileName in TargetImageFileNames)
            {
                // Read target image files.
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Add face to the person group. 
                    Console.WriteLine($"Add face to the person group person({p.Name}) from image `{targetImageFileName}`.");
                    PersistedFace faces = await Client.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId, p.PersonId, stream, targetImageFileName);

                    if (faces == null)
                    {
                        Console.WriteLine($"[Error] No persisted face from image `{targetImageFileName}`.");
                        return;
                    }
                }
            }

            List<Guid> faceIds = new List<Guid>();

            // Read image file. 
            using (FileStream stream = new FileStream(Path.Combine("Images", SourceImageFileName1), FileMode.Open))
            {
                // Detect faces from image stream. 
                IList<DetectedFace> detectedFaces = await Client.Face.DetectWithStreamAsync(stream);
                if (detectedFaces == null || detectedFaces.Count == 0)
                {
                    Console.WriteLine($"[Error] No face detected from image `{SourceImageFileName1}`.");
                    return;
                }

                Console.WriteLine($"{detectedFaces.Count} faces detected from image `{SourceImageFileName1}`.");
                if (detectedFaces[0].FaceId == null)
                {
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for Identify purpose.");
                    return;
                }

                // Add detected faceId to faceIds.
                faceIds.Add(detectedFaces[0].FaceId.Value);
            }

            // Verification example for faces of the same person.
            VerifyResult verifyResult = await Client.Face.VerifyFaceToPersonAsync(faceIds[0], p.PersonId, personGroupId);
            Console.WriteLine(
                verifyResult.IsIdentical
                    ? $"Faces from {SourceImageFileName1} & {p.Name} are of the same (Positive) person, similarity confidence: {verifyResult.Confidence}."
                    : $"Faces from {SourceImageFileName1} & {p.Name} are of different (Negative) persons, similarity confidence: {verifyResult.Confidence}.");

            // Delete the person group.
            Console.WriteLine($"Delete the person group ({personGroupId}).");
            await Client.PersonGroup.DeleteAsync(personGroupId);

            Console.WriteLine();
        }

        public static async Task Verify_in_LargePersonGroup()
        {
            Console.WriteLine("Sample of verify face to large person group.");

            // Create a large person group.
            string largePersonGroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a large person group ({largePersonGroupId}).");
            await Client.LargePersonGroup.CreateAsync(largePersonGroupId, largePersonGroupId);

            // Create a large person group person.
            Person p = new Person { Name = "Dad", UserData = "Person for sample" };
            Console.WriteLine($"Create a large person group person '{p.Name}'.");
            p.PersonId = (await Client.LargePersonGroupPerson.CreateAsync(largePersonGroupId, p.Name)).PersonId;

            foreach (var targetImageFileName in TargetImageFileNames)
            {
                // Read target image files.
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Add face to the large person group. 
                    Console.WriteLine($"Add face to the large person group person({p.Name}) from image {targetImageFileName}.");
                    PersistedFace faces = await Client.LargePersonGroupPerson.AddFaceFromStreamAsync(largePersonGroupId, p.PersonId, stream, targetImageFileName);

                    if (faces == null)
                    {
                        Console.WriteLine($"[Error] No persisted face from image `{targetImageFileName}`.");
                        return;
                    }
                }
            }

            List<Guid> faceIds = new List<Guid>();

            // Read image file. 
            using (FileStream stream = new FileStream(Path.Combine("Images", SourceImageFileName1), FileMode.Open))
            {
                // Detect faces from image stream. 
                IList<DetectedFace> detectedFaces = await Client.Face.DetectWithStreamAsync(stream);
                if (detectedFaces == null || detectedFaces.Count == 0)
                {
                    Console.WriteLine($"[Error] No face detected from image `{SourceImageFileName1}`.");
                    return;
                }

                Console.WriteLine($"{detectedFaces.Count} faces detected from image `{SourceImageFileName1}`.");
                if (detectedFaces[0].FaceId == null)
                {
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for Identify purpose.");
                    return;
                }

                // Add detected faceId to faceIds.
                faceIds.Add(detectedFaces[0].FaceId.Value);
            }

            // Verification example for faces of the same person.
            VerifyResult verifyResult = await Client.Face.VerifyFaceToPersonAsync(faceIds[0], p.PersonId, null, largePersonGroupId);
            Console.WriteLine(
                verifyResult.IsIdentical
                    ? $"Faces from {SourceImageFileName1} & {p.Name} are of the same (Positive) person, similarity confidence: {verifyResult.Confidence}."
                    : $"Faces from {SourceImageFileName1} & {p.Name} are of different (Negative) persons, similarity confidence: {verifyResult.Confidence}.");

            // Delete the large person group.
            Console.WriteLine($"Delete the large person group ({largePersonGroupId}).");
            await Client.LargePersonGroup.DeleteAsync(largePersonGroupId);

            Console.WriteLine();
        }
    }
}
