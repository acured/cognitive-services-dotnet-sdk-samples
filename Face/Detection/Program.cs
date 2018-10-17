using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace Detection
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create a client.
            string apiKey = "ENTER YOUR KEY HERE";
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                Endpoint = "ENTER YOUR ENDPOINT HERE"
            };

            List<string> imageFileNames =
                new List<string> { "detection1.jpg", "detection2.jpg", "detection3.jpg", "detection4.jpg", "detection5.jpg", "detection6.jpg" };

            foreach (var imageFileName in imageFileNames)
            {
                // Read image file.
                using (FileStream stream = new FileStream(Path.Combine("Images", imageFileName), FileMode.Open))
                {
                    // Detect faces with all attributes from image stream.
                    IList<DetectedFace> detectedFaces = client.Face.DetectWithStreamAsync(
                        stream,
                        false,
                        true,
                        new List<FaceAttributeType>
                        {
                            FaceAttributeType.Accessories,
                            FaceAttributeType.Age,
                            FaceAttributeType.Blur,
                            FaceAttributeType.Emotion,
                            FaceAttributeType.Exposure,
                            FaceAttributeType.FacialHair,
                            FaceAttributeType.Gender,
                            FaceAttributeType.Glasses,
                            FaceAttributeType.Hair,
                            FaceAttributeType.HeadPose,
                            FaceAttributeType.Makeup,
                            FaceAttributeType.Noise,
                            FaceAttributeType.Occlusion,
                            FaceAttributeType.Smile
                        }).Result;

                    if (detectedFaces == null || detectedFaces.Count == 0)
                    {
                        Console.WriteLine($"[Error] No face detected from image `{imageFileName}`.");
                        return;
                    }

                    Console.WriteLine($"{detectedFaces.Count} faces detected from image `{imageFileName}`.");
                    if (detectedFaces[0].FaceAttributes == null)
                    {
                        Console.WriteLine("[Error] Parameter `returnFaceAttributes` of `DetectWithStreamAsync` must be set to get face attributes.");
                        return;
                    }

                    // all attributes of faces 
                    foreach (var face in detectedFaces)
                    {
                        Console.WriteLine($"Face attributes of {imageFileName}   Rectangle(Left/Top/Width/Height) : {face.FaceRectangle.Left} {face.FaceRectangle.Top} {face.FaceRectangle.Width} {face.FaceRectangle.Height}");
                        Console.WriteLine($"Face attributes of {imageFileName}   Accessories : {GetAccessories(face.FaceAttributes.Accessories)}");
                        Console.WriteLine($"Face attributes of {imageFileName}   Age : {face.FaceAttributes.Age}");
                        Console.WriteLine($"Face attributes of {imageFileName}   Blur : {face.FaceAttributes.Blur.BlurLevel}");
                        Console.WriteLine($"Face attributes of {imageFileName}   Emotion : {GetEmotion(face.FaceAttributes.Emotion)}");
                        Console.WriteLine($"Face attributes of {imageFileName}   Exposure : {face.FaceAttributes.Exposure.ExposureLevel}");
                        Console.WriteLine($"Face attributes of {imageFileName}   FacialHair : {string.Format("{0}", face.FaceAttributes.FacialHair.Moustache + face.FaceAttributes.FacialHair.Beard + face.FaceAttributes.FacialHair.Sideburns > 0 ? "Yes" : "No")}");
                        Console.WriteLine($"Face attributes of {imageFileName}   Gender : {face.FaceAttributes.Gender}");
                        Console.WriteLine($"Face attributes of {imageFileName}   Glasses : {face.FaceAttributes.Glasses}");
                        Console.WriteLine($"Face attributes of {imageFileName}   Hair : {GetHair(face.FaceAttributes.Hair)}");
                        Console.WriteLine($"Face attributes of {imageFileName}   HeadPose : {string.Format("Pitch: {0}, Roll: {1}, Yaw: {2}", Math.Round(face.FaceAttributes.HeadPose.Pitch, 2), Math.Round(face.FaceAttributes.HeadPose.Roll, 2), Math.Round(face.FaceAttributes.HeadPose.Yaw, 2))}");
                        Console.WriteLine($"Face attributes of {imageFileName}   Makeup : {string.Format("{0}", ((face.FaceAttributes.Makeup.EyeMakeup || face.FaceAttributes.Makeup.LipMakeup) ? "Yes" : "No"))}");
                        Console.WriteLine($"Face attributes of {imageFileName}   Noise : {face.FaceAttributes.Noise.NoiseLevel}");
                        Console.WriteLine($"Face attributes of {imageFileName}   Occlusion : {string.Format("EyeOccluded: {0}", ((face.FaceAttributes.Occlusion.EyeOccluded) ? "Yes" : "No"))}   {string.Format("ForeheadOccluded: {0}", ((face.FaceAttributes.Occlusion.ForeheadOccluded) ? "Yes" : "No"))}   {string.Format("MouthOccluded: {0}", ((face.FaceAttributes.Occlusion.MouthOccluded) ? "Yes" : "No"))}");
                        Console.WriteLine($"Face attributes of {imageFileName}   Smile : {face.FaceAttributes.Smile}");
                        Console.WriteLine();
                    }
                }
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }

        private static string GetAccessories(IList<Accessory> accessories)
        {
            if (accessories.Count == 0)
            {
                return "NoAccessories";
            }

            string[] accessoryArray = new string[accessories.Count];

            for (int i = 0; i < accessories.Count; ++i)
            {
                accessoryArray[i] = accessories[i].Type.ToString();
            }

            return string.Join(",", accessoryArray);
        }

        private static string GetEmotion(Emotion emotion)
        {
            string emotionType = string.Empty;
            double emotionValue = 0.0;
            if (emotion.Anger > emotionValue)
            {
                emotionValue = emotion.Anger;
                emotionType = "Anger";
            }

            if (emotion.Contempt > emotionValue)
            {
                emotionValue = emotion.Contempt;
                emotionType = "Contempt";
            }

            if (emotion.Disgust > emotionValue)
            {
                emotionValue = emotion.Disgust;
                emotionType = "Disgust";
            }

            if (emotion.Fear > emotionValue)
            {
                emotionValue = emotion.Fear;
                emotionType = "Fear";
            }

            if (emotion.Happiness > emotionValue)
            {
                emotionValue = emotion.Happiness;
                emotionType = "Happiness";
            }

            if (emotion.Neutral > emotionValue)
            {
                emotionValue = emotion.Neutral;
                emotionType = "Neutral";
            }

            if (emotion.Sadness > emotionValue)
            {
                emotionValue = emotion.Sadness;
                emotionType = "Sadness";
            }

            if (emotion.Surprise > emotionValue)
            {
                emotionValue = emotion.Surprise;
                emotionType = "Surprise";
            }

            return $"{emotionType}";
        }

        private static string GetHair(Hair hair)
        {
            if (hair.HairColor.Count == 0)
            {
                return hair.Invisible ? "Invisible" : "Bald";
            }

            HairColorType returnColor = HairColorType.Unknown;
            double maxConfidence = 0.0f;

            for (int i = 0; i < hair.HairColor.Count; ++i)
            {
                if (hair.HairColor[i].Confidence > maxConfidence)
                {
                    maxConfidence = hair.HairColor[i].Confidence;
                    returnColor = hair.HairColor[i].Color;
                }
            }

            return returnColor.ToString();
        }
    }
}
