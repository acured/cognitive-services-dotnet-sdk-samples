namespace Microsoft.Azure.CognitiveServices.Samples.Face
{
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public static class Verify_FaceToFace
    {
        public static async Task Run(string endpoint, string key)
        {
            // Create a client.
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg" };
            string sourceImageFileName1 = "Family1-Dad3.jpg";
            string sourceImageFileName2 = "Family1-Son1.jpg";

            List<Guid?> targetFaceIds = new List<Guid?>();
            Guid? sourceFaceId1 = new Guid?();
            Guid? sourceFaceId2 = new Guid?();

            foreach (var imageFileName in targetImageFileNames)
            {
                // Add detected faceId to targetFaceIds.
                targetFaceIds.Add((await Common.DetectedFace(client, Path.Combine("Images", imageFileName)))[0].FaceId);
            }

            // Detect sourceFaceId1 from SourceImageFileName1
            sourceFaceId1 = (await Common.DetectedFace(client, Path.Combine("Images", sourceImageFileName1)))[0].FaceId;

            // Detect sourceFaceId2 from SourceImageFileName2
            sourceFaceId2 = (await Common.DetectedFace(client, Path.Combine("Images", sourceImageFileName2)))[0].FaceId;


            // Verification example for faces of the same person.
            VerifyResult verifyResult1 = await client.Face.VerifyFaceToFaceAsync(sourceFaceId1.Value, targetFaceIds[0].Value);
            Console.WriteLine($"Faces from {sourceImageFileName1} & {targetImageFileNames[0]} are similar with confidence: {verifyResult1.Confidence}.");

            // Verification example for faces of different persons.
            VerifyResult verifyResult2 = await client.Face.VerifyFaceToFaceAsync(sourceFaceId2.Value, targetFaceIds[0].Value);
            Console.WriteLine($"Faces from {sourceImageFileName2} & {targetImageFileNames[0]} are similar with confidence: {verifyResult2.Confidence}.");
        }
    }

    public static class Verify_in_PersonGroup
    {
        public static async Task Run(string endpoint, string key)
        {
            // Create a client.
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg" };
            string sourceImageFileName = "Family1-Dad3.jpg";

            // Create a person group.
            string GroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a person group ({GroupId}).");
            await client.PersonGroup.CreateAsync(GroupId, GroupId);

            // Create a person group person.
            Person p = new Person();
            p.Name = "Dad";
            p.UserData = "Person for verification sample";
            Console.WriteLine($"Create a person group person '{p.Name}'.");
            p.PersonId = (await client.PersonGroupPerson.CreateAsync(GroupId, p.Name)).PersonId;

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Read image file. 
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Add face to the person group. 
                    PersistedFace faces = await client.PersonGroupPerson.AddFaceFromStreamAsync(GroupId, p.PersonId, stream, targetImageFileName);
                    if (faces == null)
                    {
                        throw new Exception($"No persisted face from image `{targetImageFileName}`.");
                    }

                    Console.WriteLine($"Add faces to the person group from image `{targetImageFileName}`.");
                }
            }

            // Start to train the person group.
            Console.WriteLine($"Train person group {GroupId}.");
            await client.PersonGroup.TrainAsync(GroupId);

            // Wait until the training is completed.
            while (true)
            {
                await Task.Delay(1000);
                var trainingStatus = await client.PersonGroup.GetTrainingStatusAsync(GroupId);
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

            // Detect sourceFaceId from sourceImageFileName 
            Guid? sourceFaceId = (await Common.DetectedFace(client, Path.Combine("Images", sourceImageFileName)))[0].FaceId;

            // Verification example for faces of the same person.
            VerifyResult VerifyResults = await client.Face.VerifyFaceToPersonAsync(sourceFaceId.Value, p.PersonId, GroupId);
            Console.WriteLine($"Faces from {sourceImageFileName} & {p.Name} are similar with confidence: {VerifyResults.Confidence}.");

            // Delete the person group.
            Console.WriteLine($"Delete the person group ({GroupId}).");
            await client.PersonGroup.DeleteAsync(GroupId);
        }
    }

    public static class Verify_in_LargePersonGroup
    {
        public static async Task Run(string endpoint, string key)
        {
            // Create a client.
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg" };
            string sourceImageFileName = "Family1-Dad3.jpg";

            // Create a large person group.
            string GroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a large person group ({GroupId}).");
            await client.LargePersonGroup.CreateAsync(GroupId, GroupId);

            // Create a large person group person.
            Person p = new Person();
            p.Name = "Dad";
            p.UserData = "Person for sample";
            Console.WriteLine($"Create a large person group person '{p.Name}'.");
            p.PersonId = (await client.LargePersonGroupPerson.CreateAsync(GroupId, p.Name)).PersonId;

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Read image file. 
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Add face to the large person group. 
                    Console.WriteLine($"Add faces to the large person group from image `{targetImageFileName}`.");
                    PersistedFace faces = await client.LargePersonGroupPerson.AddFaceFromStreamAsync(GroupId, p.PersonId, stream, targetImageFileName);

                    if (faces == null)
                    {
                        throw new Exception($"No persisted face from image `{targetImageFileName}`.");
                    }
                }
            }

            // Start to train the large person group.
            Console.WriteLine($"Train large person group {GroupId}.");
            await client.LargePersonGroup.TrainAsync(GroupId);

            // Wait until the training is completed.
            while (true)
            {
                await Task.Delay(1000);
                var trainingStatus = await client.LargePersonGroup.GetTrainingStatusAsync(GroupId);
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

            // Detect sourceFaceId from sourceImageFileName 
            Guid? sourceFaceId = (await Common.DetectedFace(client, Path.Combine("Images", sourceImageFileName)))[0].FaceId;

            // Verification example for faces of the same person.
            VerifyResult VerifyResults = await client.Face.VerifyFaceToPersonAsync(sourceFaceId.Value, p.PersonId, null, GroupId);
            Console.WriteLine($"Faces from {sourceImageFileName} & {p.Name} are similar with confidence: {VerifyResults.Confidence}.");

            // Delete the large person group.
            Console.WriteLine($"Delete the large person group ({GroupId})");
            await client.LargePersonGroup.DeleteAsync(GroupId);
        }    
    }
}