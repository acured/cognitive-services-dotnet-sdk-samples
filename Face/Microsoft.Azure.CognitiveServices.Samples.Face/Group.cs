namespace Microsoft.Azure.CognitiveServices.Samples.Face
{
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public static class Group
    {
        public static async Task Run(string endpoint, string key)
        {
            // Create a client.
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            List<string> targetImageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg", "Family3-Lady1.jpg", "Family1-Daughter1.jpg", "Family1-Daughter2.jpg", "Family1-Daughter3.jpg" };

            Dictionary<string, string> faces = new Dictionary<string, string>();
            List<Guid> faceIds = new List<Guid>();

            foreach (var imageFileName in targetImageFileNames)
            {
                Guid? faceid = (await Common.DetectedFace(client, Path.Combine("Images", imageFileName)))[0].FaceId;

                // Add detected faceId to faceIds.
                faceIds.Add(faceid.Value);

                // Add detected faceId to faces.
                faces.Add(faceid.Value.ToString(), imageFileName);
            }

            // Call grouping, the grouping result is a group collection, each group contains similar faces.
            var groupRes = client.Face.GroupAsync(faceIds).Result;

            // Grouping results.
            foreach (var g in groupRes.Groups)
            {
                Console.Write($"{Environment.NewLine}Find group face: ");
                foreach (var fr in g)
                {
                    Console.Write($"{faces[fr.ToString()]}\t");
                }
            }

            // MessyGroup contains all faces which are not similar to any other faces.
            if (groupRes.MessyGroup.Count > 0)
            {
                Console.Write($"{Environment.NewLine}Find messy group face: ");
                foreach (var fr in groupRes.MessyGroup)
                {
                    Console.Write($"{faces[fr.ToString()]}\t");
                }
            }
        }
    }
}
