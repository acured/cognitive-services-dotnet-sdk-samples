using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace Verify
{
    public class Program
    {
        // Create a client. 
        //static string _apiKey = "ENTER YOUR KEY HERE";
        //static IFaceClient _client = new FaceClient(new ApiKeyServiceClientCredentials(_apiKey))
        //{
        //    Endpoint = "ENTER YOUR ENDPOINT HERE"
        //};
        static string _apiKey = "793315696cd04d5ab78232a792136610";
        static IFaceClient _client = new FaceClient(new ApiKeyServiceClientCredentials(_apiKey))
        {
            Endpoint = "https://westus.api.cognitive.microsoft.com"
        };

        public static void Main(string[] args)
        {
            Verify_in_PersonGroup();
            Console.ReadLine();
        }

        static async void Verify_in_PersonGroup()
        {
            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg", "Family1-Dad3.jpg" };
            string sourceImageFileName = "Family1-Dad1.jpg";

            //create PersonGroup
            string GroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create PersonGroup ({GroupId}) {Environment.NewLine}");
            await _client.PersonGroup.CreateAsync(GroupId, GroupId);

            //create PersonGroup Person
            Person p = new Person();
            p.Name = "Dad";
            p.UserData = "Person for sample";
            Console.WriteLine($"Create PersonGroup Person '{p.Name}' {Environment.NewLine}");
            p.PersonId = (await _client.PersonGroupPerson.CreateAsync(GroupId, p.Name)).PersonId;

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Read image file. 
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // add face to PersonGroup. 
                    Console.WriteLine($"add faces to PersonGroup from image `{targetImageFileName}`.");
                    PersistedFace faces = _client.PersonGroupPerson.AddFaceFromStreamAsync(GroupId, p.PersonId, stream, targetImageFileName).Result;

                    if (faces == null)
                    {
                        Console.WriteLine($"[Error] No persisted face from image `{targetImageFileName}`.");
                        return;
                    }
                }
            }

            //Train PersonGroup
            await _client.PersonGroup.TrainAsync(GroupId);

            List<Guid> faceIds = new List<Guid>();
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
                    Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for Identify purpose.");
                    return;
                }

                faceIds.Add(detectedFaces[0].FaceId.Value);
            }

            // Identify example for get person from PersonGroup with Candidate confidence. 
            var identifyResults = _client.Face.VerifyFaceToPersonAsync(faceIds[0], p.PersonId, GroupId).Result;
            //foreach (var identifyResult in identifyResults)
            //{
            //    Person person = await _client.PersonGroupPerson.GetAsync(GroupId, identifyResult.Candidates[0].PersonId);
            //    Console.WriteLine($"Person from {sourceImageFileName} & Group person '{person.Name}' are of the same (Positive) person, confidence: {identifyResult.Candidates[0].Confidence}.");
            //}

            //delete PersonGroup
            await _client.PersonGroup.DeleteAsync(GroupId);
            Console.WriteLine($"{Environment.NewLine}Delete PersonGroup {GroupId}");

            Console.WriteLine("\nPress ENTER to exit.");
        }
    }
}
