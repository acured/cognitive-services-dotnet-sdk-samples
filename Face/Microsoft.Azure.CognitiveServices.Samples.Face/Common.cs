namespace Microsoft.Azure.CognitiveServices.Samples.Face
{
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    public static class Common
    {
        public static async Task<List<DetectedFace>> DetectedFace(IFaceClient faceClient ,string imagePath)
        {             
            // Read image file.
            using (FileStream stream = new FileStream(imagePath, FileMode.Open))
            {
                // Detect faces from image stream.
                IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithStreamAsync(stream);
                if (detectedFaces == null || detectedFaces.Count == 0)
                {
                    throw new Exception($"No face detected from image `{imagePath}`.");
                }

                Console.WriteLine($"{detectedFaces.Count} faces detected from image `{imagePath}`.");
                if (detectedFaces[0].FaceId == null)
                {
                    throw new Exception($"Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default).");
                }

                return detectedFaces.ToList();
            }
        }
    }
}
