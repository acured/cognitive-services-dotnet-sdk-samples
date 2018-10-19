using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

        public static List<string> TargetImageFileNames =
            new List<string> { "Family1-Dad1.jpg", "Family1-Daughter1.jpg", "Family1-Mom1.jpg", "Family1-Son1.jpg", "Family2-Lady1.jpg", "Family2-Man1.jpg", "Family3-Lady1.jpg", "Family3-Man1.jpg" };

        public static string SourceImageFileName = "identification1.jpg";

        public static void Main(string[] args)
        {
            Identify_in_PersonGroup().Wait();
            Identify_in_LargePersonGroup().Wait();

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }

        public static async Task Identify_in_PersonGroup() 
        {
            // Create a person group.
            string GroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a person group ({GroupId}) {Environment.NewLine}");
            await Client.PersonGroup.CreateAsync(GroupId, GroupId);

            foreach (var targetImageFileName in TargetImageFileNames) 
            { 
                // Read image file. 
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open)) 
                {
                    // Create a person group person.
                    Person p = new Person();
                    p.Name = targetImageFileName;
                    p.UserData = "Person for sample";
                    Console.WriteLine($"Create a person group person '{p.Name}'");
                    p.PersonId = (await Client.PersonGroupPerson.CreateAsync(GroupId, p.Name)).PersonId;

                    // Add face to the person group.
                    Console.WriteLine($"Add faces to the person group from image `{targetImageFileName}`. {Environment.NewLine}");
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

            // Wait until the training is completed.
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
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for Identify purpose.");
                    return; 
                }

                // Add detected faceId to faceIds.
                foreach (var face in detectedFaces)
                {
                    faceIds.Add(face.FaceId.Value);
                }
            }

            // Identify example for get person from the person group with candidate confidence. 
            var identifyResults = await Client.Face.IdentifyAsync(faceIds, GroupId);
            if (identifyResults == null)
            {
                Console.WriteLine($"No people found in the group the same as the {SourceImageFileName}.");
                return;
            }

            foreach (var identifyResult in identifyResults)
            {
                Person person = await Client.PersonGroupPerson.GetAsync(GroupId, identifyResult.Candidates[0].PersonId);                
                Console.WriteLine($"Person from {SourceImageFileName} & group person '{person.Name}' are of the same (Positive) person, confidence: {identifyResult.Candidates[0].Confidence}.");
            }

            // Delete the person group.
            await Client.PersonGroup.DeleteAsync(GroupId);
            Console.WriteLine($"{Environment.NewLine}Delete the person group {GroupId}{Environment.NewLine}"); 
        }

        public static async Task Identify_in_LargePersonGroup()
        {
            // Create a large person group.
            string GroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a large person group ({GroupId}) {Environment.NewLine}");
            await Client.LargePersonGroup.CreateAsync(GroupId, GroupId);

            foreach (var targetImageFileName in TargetImageFileNames)
            {
                // Read image file. 
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Create a person group person.
                    Person p = new Person();
                    p.Name = targetImageFileName;
                    p.UserData = "Person for sample";
                    Console.WriteLine($"Create a person group person '{p.Name}'");
                    p.PersonId = (await Client.LargePersonGroupPerson.CreateAsync(GroupId, p.Name)).PersonId;

                    // Add face to the large person group.
                    Console.WriteLine($"Add faces to the large person group from image `{targetImageFileName}`.{Environment.NewLine}");
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

            // Wait until the training is completed.
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
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for Identify purpose.");
                    return;
                }

                // Add detected faceId to faceIds.
                foreach (var face in detectedFaces)
                {
                    faceIds.Add(face.FaceId.Value);
                }
            }

            // Identify example for get person from the person group with candidate confidence. 
            var identifyResults = await Client.Face.IdentifyAsync(faceIds, null, GroupId);
            if (identifyResults == null)
            {
                Console.WriteLine($"No people found in the group the same as the {SourceImageFileName}.");
                return;
            }

            foreach (var identifyResult in identifyResults)
            {
                Person person = await Client.LargePersonGroupPerson.GetAsync(GroupId, identifyResult.Candidates[0].PersonId);
                Console.WriteLine($"Person from {SourceImageFileName} & group person '{person.Name}' are of the same (Positive) person, confidence: {identifyResult.Candidates[0].Confidence}.");
            }

            // Delete the person group.
            await Client.LargePersonGroup.DeleteAsync(GroupId);
            Console.WriteLine($"{Environment.NewLine}Delete the large person group {GroupId}{Environment.NewLine}");
        }
    }
}
