﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace Verify
{
    public class Program
    {
        // Create a client. 
        //static string ApiKey  = "ENTER YOUR KEY HERE";
        //static IFaceClient Client  = new FaceClient(new ApiKeyServiceClientCredentials(ApiKey))
        //{
        //    Endpoint = "ENTER YOUR ENDPOINT HERE"
        //};
        static string ApiKey = "793315696cd04d5ab78232a792136610";
        static IFaceClient Client = new FaceClient(new ApiKeyServiceClientCredentials(ApiKey))
        {
            Endpoint = "https://westus.api.cognitive.microsoft.com"
        };

        public static void Main(string[] args)
        {
            //Verify_in_PersonGroup();
            Verify_in_LargePersonGroup();
            Console.ReadLine();
        }

        public static async void Verify_in_PersonGroup()
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

            // Verification example for faces of the same person.
            VerifyResult VerifyResults = Client.Face.VerifyFaceToPersonAsync(faceIds[0], p.PersonId, GroupId).Result;
            Console.WriteLine(
                VerifyResults.IsIdentical
                    ? $"Faces from {sourceImageFileName} & {p.Name} are of the same (Positive) person, similarity confidence: {VerifyResults.Confidence}."
                    : $"Faces from {sourceImageFileName} & {p.Name} are of different (Negative) persons, similarity confidence: {VerifyResults.Confidence}.");

            // Delete the person group.
            await Client.PersonGroup.DeleteAsync(GroupId);
            Console.WriteLine($"{Environment.NewLine}Delete the person group ({GroupId})");

            Console.WriteLine("\nPress ENTER to exit.");
        }

        public static async void Verify_in_LargePersonGroup()
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

            // Verification example for faces of the same person.
            VerifyResult VerifyResults = Client.Face.VerifyFaceToPersonAsync(faceIds[0], p.PersonId, null, GroupId).Result;
            Console.WriteLine(
                VerifyResults.IsIdentical
                    ? $"Faces from {sourceImageFileName} & {p.Name} are of the same (Positive) person, similarity confidence: {VerifyResults.Confidence}."
                    : $"Faces from {sourceImageFileName} & {p.Name} are of different (Negative) persons, similarity confidence: {VerifyResults.Confidence}.");

            // Delete the large person group.
            await Client.LargePersonGroup.DeleteAsync(GroupId);
            Console.WriteLine($"{Environment.NewLine}Delete the large person group ({GroupId})");

            Console.WriteLine("\nPress ENTER to exit.");
        }
    }
}
