using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace Identify
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
            Identify_in_PersonGroup();
            Identify_in_LargePersonGroup();
            Console.ReadLine();
        }

        public static async void Identify_in_PersonGroup() 
        {
            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg", "Family1-Dad3.jpg" };
            string sourceImageFileName = "Family1-Dad1.jpg";

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
                    PersistedFace faces = Client.PersonGroupPerson.AddFaceFromStreamAsync(GroupId, p.PersonId, stream, targetImageFileName).Result; 

                    if (faces == null) 
                    { 
                        Console.WriteLine($"[Error] No persisted face from image `{targetImageFileName}`."); 
                        return; 
                    } 
                } 
            }

            // Start to train the person group.
            await Client.PersonGroup.TrainAsync(GroupId);

            List<Guid> faceIds = new List<Guid>();
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
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for Identify purpose.");
                    return; 
                }

                // Add detected faceId to faceIds.
                faceIds.Add(detectedFaces[0].FaceId.Value);                
            }

            // Identify example for get person from the person group with candidate confidence. 
            var identifyResults = Client.Face.IdentifyAsync(faceIds, GroupId).Result;
            foreach (var identifyResult in identifyResults)
            {
                Person person = await Client.PersonGroupPerson.GetAsync(GroupId, identifyResult.Candidates[0].PersonId);                
                Console.WriteLine($"Person from {sourceImageFileName} & group person '{person.Name}' are of the same (Positive) person, confidence: {identifyResult.Candidates[0].Confidence}."); 
            }

            // Delete the person group.
            await Client.PersonGroup.DeleteAsync(GroupId); 
            Console.WriteLine($"{Environment.NewLine}Delete the person group {GroupId}"); 

            Console.WriteLine("\nPress ENTER to exit."); 
        }

        public static async void Identify_in_LargePersonGroup()
        {
            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg", "Family1-Dad3.jpg" };
            string sourceImageFileName = "Family1-Dad1.jpg";

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
                    PersistedFace faces = Client.LargePersonGroupPerson.AddFaceFromStreamAsync(GroupId, p.PersonId, stream, targetImageFileName).Result;

                    if (faces == null)
                    {
                        Console.WriteLine($"[Error] No persisted face from image `{targetImageFileName}`.");
                        return;
                    }
                }
            }

            // Start to train the large person group.
            await Client.LargePersonGroup.TrainAsync(GroupId);

            List<Guid> faceIds = new List<Guid>();
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
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for Identify purpose.");
                    return;
                }

                // Add detected faceId to faceIds.
                faceIds.Add(detectedFaces[0].FaceId.Value);
            }

            // Identify example for get person from the large person group with candidate confidence. 
            var identifyResults = Client.Face.IdentifyAsync(faceIds, null, GroupId).Result;
            foreach (var identifyResult in identifyResults)
            {
                Person person = await Client.LargePersonGroupPerson.GetAsync(GroupId, identifyResult.Candidates[0].PersonId);
                Console.WriteLine($"Person from {sourceImageFileName} & large person group person '{person.Name}' are of the same (Positive) person, confidence: {identifyResult.Candidates[0].Confidence}.");
            }

            // Delete the person group.
            await Client.LargePersonGroup.DeleteAsync(GroupId);
            Console.WriteLine($"{Environment.NewLine}Delete the large person group {GroupId}");

            Console.WriteLine("\nPress ENTER to exit.");
        }
    }
}
