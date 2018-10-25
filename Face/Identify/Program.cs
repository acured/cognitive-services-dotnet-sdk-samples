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
        public static string ApiKey = "ENTER YOUR KEY HERE";

        public static IFaceClient Client = new FaceClient(new ApiKeyServiceClientCredentials(ApiKey))
        {
            Endpoint = "ENTER YOUR ENDPOINT HERE"
        };

        public static Dictionary<string, string[]> TargetImageFileDictionary = new Dictionary<string, string[]>
        {
            { "Family1-Dad", new string[]{ "Family1-Dad1.jpg", "Family1-Dad2.jpg"}},
            { "Family1-Mom",new string[] { "Family1-Mom1.jpg", "Family1-Mom2.jpg"}},
            { "Family1-Son",new string[]{ "Family1-Son1.jpg", "Family1-Son2.jpg" }},
            { "Family1-Daughter",new string[] { "Family1-Daughter1.jpg", "Family1-Daughter2.jpg"}},
            { "Family2-Lady",new string[]{"Family2-Lady1.jpg", "Family2-Lady2.jpg"}},
            { "Family2-Man",new string[] {"Family2-Man1.jpg","Family2-Man2.jpg" }
        } };            

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
            Console.WriteLine("Sample of Identify faces in person group.");

            // Create a person group.
            string personGroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a person group ({personGroupId}).");
            await Client.PersonGroup.CreateAsync(personGroupId, personGroupId);

            foreach (var targetImageFileDictionaryName in TargetImageFileDictionary.Keys) 
            {
                // Create a person group person.
                Person p = new Person { Name = targetImageFileDictionaryName, UserData = "Person for sample" };
                p.PersonId = (await Client.PersonGroupPerson.CreateAsync(personGroupId, p.Name)).PersonId;
                Console.WriteLine($"Create a person group person '{p.Name}'.");

                foreach (var targetImageFileName in TargetImageFileDictionary[targetImageFileDictionaryName])
                {
                    // Read target image file. 
                    using (FileStream stream = new FileStream(Path.Combine("Images", TargetImageFileDictionary[targetImageFileDictionaryName][0]), FileMode.Open))
                    {
                        // Add face to the person group person.
                        Console.WriteLine($"Add face to the person group person({targetImageFileDictionaryName}) from image `{targetImageFileName}`.");
                        PersistedFace face = await Client.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId, p.PersonId, stream, targetImageFileName);

                        if (face == null)
                        {
                            Console.WriteLine($"[Error] No persisted face from image `{targetImageFileName}`.");
                            return;
                        }
                    }
                }
            }

            // Start to train the person group.
            Console.WriteLine($"Train person group {personGroupId}.");
            await Client.PersonGroup.TrainAsync(personGroupId);

            // Wait until the training is completed.
            while (true)
            {
                await Task.Delay(1000);
                var trainingStatus = await Client.PersonGroup.GetTrainingStatusAsync(personGroupId);
                Console.WriteLine($"Training status is {trainingStatus.Status}.");
                if (trainingStatus.Status != TrainingStatusType.Running)
                {
                    if (trainingStatus.Status == TrainingStatusType.Failed)
                    {
                        throw new Exception($"Training failed with message {trainingStatus.Message}.");
                    }

                    break;
                }
            }

            List<Guid> sourceFaceIds = new List<Guid>();

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
                    if (face.FaceId != null)
                    {
                        sourceFaceIds.Add(face.FaceId.Value);
                    }
                }
            }

            // Identify example for get person from the person group with candidate confidence. 
            var identifyResults = await Client.Face.IdentifyAsync(sourceFaceIds, personGroupId);
            if (identifyResults == null)
            {
                Console.WriteLine($"No people found in the group the same as the {SourceImageFileName}.");
                return;
            }

            foreach (var identifyResult in identifyResults)
            {
                Person person = await Client.PersonGroupPerson.GetAsync(personGroupId, identifyResult.Candidates[0].PersonId);                
                Console.WriteLine($"Person '{person.Name}' is identified for faces in {SourceImageFileName}, confidence: {identifyResult.Candidates[0].Confidence}.");
            }

            // Delete the person group.
            await Client.PersonGroup.DeleteAsync(personGroupId);
            Console.WriteLine($"Delete the person group {personGroupId}.");
            Console.WriteLine();
        }

        public static async Task Identify_in_LargePersonGroup()
        {
            Console.WriteLine("Sample of Identify faces in large person group.");

            // Create a large person group.
            string largePersonGroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a large person group ({largePersonGroupId}).");
            await Client.LargePersonGroup.CreateAsync(largePersonGroupId, largePersonGroupId);

            foreach (var targetImageFileDictionaryName in TargetImageFileDictionary.Keys)
            {
                // Create a large person group person.
                Person p = new Person { Name = targetImageFileDictionaryName, UserData = "Person for sample" };
                p.PersonId = (await Client.LargePersonGroupPerson.CreateAsync(largePersonGroupId, p.Name)).PersonId;
                Console.WriteLine($"Create a large person group person '{p.Name}'.");

                foreach (var targetImageFileName in TargetImageFileDictionary[targetImageFileDictionaryName])
                {
                    // Read target image file. 
                    using (FileStream stream = new FileStream(Path.Combine("Images", TargetImageFileDictionary[targetImageFileDictionaryName][0]), FileMode.Open))
                    {
                        // Add face to the large person group person.
                        Console.WriteLine($"Add face to the large person group person({targetImageFileDictionaryName}) from image `{targetImageFileName}`.");
                        PersistedFace face = await Client.LargePersonGroupPerson.AddFaceFromStreamAsync(largePersonGroupId, p.PersonId, stream, targetImageFileName);

                        if (face == null)
                        {
                            Console.WriteLine($"[Error] No persisted face from image `{targetImageFileName}`.");
                            return;
                        }
                    }
                }
            }

            // Start to train the large person group.
            Console.WriteLine($"Train large person group {largePersonGroupId}.");
            await Client.LargePersonGroup.TrainAsync(largePersonGroupId);

            // Wait until the training is completed.
            while (true)
            {
                await Task.Delay(1000);
                var trainingStatus = await Client.LargePersonGroup.GetTrainingStatusAsync(largePersonGroupId);
                Console.WriteLine($"Training status is {trainingStatus.Status}.");
                if (trainingStatus.Status != TrainingStatusType.Running)
                {
                    if (trainingStatus.Status == TrainingStatusType.Failed)
                    {
                        throw new Exception($"Training failed with message {trainingStatus.Message}.");
                    }

                    break;
                }
            }

            List<Guid> sourceFaceIds = new List<Guid>();

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
                    if (face.FaceId != null)
                    {
                        sourceFaceIds.Add(face.FaceId.Value);
                    }
                }
            }

            // Identify example for get person from the person group with candidate confidence. 
            var identifyResults = await Client.Face.IdentifyAsync(sourceFaceIds, null, largePersonGroupId);
            if (identifyResults == null)
            {
                Console.WriteLine($"No people found in the group the same as the {SourceImageFileName}.");
                return;
            }

            foreach (var identifyResult in identifyResults)
            {
                Person person = await Client.LargePersonGroupPerson.GetAsync(largePersonGroupId, identifyResult.Candidates[0].PersonId);
                Console.WriteLine($"Person '{person.Name}' is identified for faces in {SourceImageFileName}, confidence: {identifyResult.Candidates[0].Confidence}.");
            }

            // Delete the person group.
            await Client.LargePersonGroup.DeleteAsync(largePersonGroupId);
            Console.WriteLine($"Delete the large person group {largePersonGroupId}.");
            Console.WriteLine();
        }
    }
}
