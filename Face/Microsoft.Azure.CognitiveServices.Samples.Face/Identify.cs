namespace Microsoft.Azure.CognitiveServices.Samples.Face
{
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public static class Identify_in_PersonGroup
    {
        public static async Task Run(string endpoint, string key)
        {
            // Create a client.
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Daughter1.jpg", "Family1-Mom1.jpg", "Family1-Son1.jpg", "Family2-Lady1.jpg", "Family2-Man1.jpg", "Family3-Lady1.jpg", "Family3-Man1.jpg" };

            string sourceImageFileName = "identification1.jpg";

            // Create a person group.
            string GroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a person group ({GroupId}).");
            await client.PersonGroup.CreateAsync(GroupId, GroupId);

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Read target image files.
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Create a person group person.
                    Person p = new Person();
                    p.Name = targetImageFileName;
                    p.UserData = "Person for sample";
                    Console.WriteLine($"Create a person group person '{p.Name}'");
                    p.PersonId = (await client.PersonGroupPerson.CreateAsync(GroupId, p.Name)).PersonId;

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

            // Detect sourceFaceIds from sourceImageFileName.
            List<Guid> sourceFaceIds = (await Common.DetectedFace(client, Path.Combine("Images", sourceImageFileName))).Select((f) => { return f.FaceId.Value; }).ToList();

            // Identify example of faceId to person group.
            var identifyResults = await client.Face.IdentifyAsync(sourceFaceIds, GroupId);
            if (identifyResults == null)
            {
                Console.WriteLine($"No people found in the group the same as the {sourceImageFileName}.");
                return;
            }

            foreach (var identifyResult in identifyResults)
            {
                Person person = await client.PersonGroupPerson.GetAsync(GroupId, identifyResult.Candidates[0].PersonId);
                Console.WriteLine($"Person from {sourceImageFileName} & group person '{person.Name}' are of the same (Positive) person with confidence: {identifyResult.Candidates[0].Confidence}.");
            }

            // Delete the person group.
            await client.PersonGroup.DeleteAsync(GroupId);
            Console.WriteLine($"Delete the person group {GroupId}.");
        }
    }

    public static class Identify_in_LargePersonGroup
    {
        public static async Task Run(string endpoint, string key)
        {
            // Create a client.
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Daughter1.jpg", "Family1-Mom1.jpg", "Family1-Son1.jpg", "Family2-Lady1.jpg", "Family2-Man1.jpg", "Family3-Lady1.jpg", "Family3-Man1.jpg" };
            string sourceImageFileName = "identification1.jpg";

            // Create a large person group.
            string GroupId = Guid.NewGuid().ToString();
            Console.WriteLine($"Create a large person group ({GroupId}).");
            await client.LargePersonGroup.CreateAsync(GroupId, GroupId);

            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Read target image files.
                using (FileStream stream = new FileStream(Path.Combine("Images", targetImageFileName), FileMode.Open))
                {
                    // Create a large person group person.
                    Person p = new Person();
                    p.Name = targetImageFileName;
                    p.UserData = "Person for sample";
                    Console.WriteLine($"Create a large person group person '{p.Name}'.");
                    p.PersonId = (await client.LargePersonGroupPerson.CreateAsync(GroupId, p.Name)).PersonId;

                    // Add face to the large person group.
                    PersistedFace faces = await client.LargePersonGroupPerson.AddFaceFromStreamAsync(GroupId, p.PersonId, stream, targetImageFileName);
                    if (faces == null)
                    {
                        throw new Exception($"No persisted face from image `{targetImageFileName}`.");
                    }

                    Console.WriteLine($"Add faces to the large person group from image `{targetImageFileName}`.");
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

            // Detect sourceFaceIds from sourceImageFileName
            List<Guid> sourceFaceIds = (await Common.DetectedFace(client, Path.Combine("Images", sourceImageFileName))).Select((f) => { return f.FaceId.Value; }).ToList();

            // Identify example of faceId to large person group.
            var identifyResults = await client.Face.IdentifyAsync(sourceFaceIds, null, GroupId);
            if (identifyResults == null)
            {
                Console.WriteLine($"No people found in the group the same as the {sourceImageFileName}.");
                return;
            }

            foreach (var identifyResult in identifyResults)
            {
                Person person = await client.LargePersonGroupPerson.GetAsync(GroupId, identifyResult.Candidates[0].PersonId);
                Console.WriteLine($"Person from {sourceImageFileName} & group person '{person.Name}' are of the same (Positive) person with confidence: {identifyResult.Candidates[0].Confidence}.");
            }

            // Delete the large person group.
            await client.LargePersonGroup.DeleteAsync(GroupId);
            Console.WriteLine($"Delete the large person group {GroupId}.");
        }
    }    
}
