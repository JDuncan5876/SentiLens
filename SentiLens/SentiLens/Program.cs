using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace System.Net
{
    public class FacialData
    {
        public int left;
        public int top;
        public int width;
        public int height;
        public string emotion;

        public FacialData(int left, int top, int width, int height, string emotion)
        {
            this.left = left;
            this.top = top;
            this.width = width;
            this.height = height;
            this.emotion = emotion;
        }

        public override string ToString()
        {
            return left + " " + top + " " + width + " " + height + " " + emotion;
        }
    }
    public class RequestSentiment
    {
        //_apiKey: Replace this with your own Project Oxford Emotion API key, please do not use my key. I inlcude it here so you can get up and running quickly but you can get your own key for free at https://www.projectoxford.ai/emotion 
        public const string _apiKey = "ee71e10f3f454f03a1bbc4c033f205a3";
        //_apiUrl: The base URL for the API. Find out what this is for other APIs via the API documentation
        public const string _apiUrl = "https://api.projectoxford.ai/emotion/v1.0/recognize";

        public static async Task<FacialData> AsyncFaceRequest(byte[] data)
        {
            using (var client = new HttpClient())
            {
                //setup HttpClient
                client.BaseAddress = new Uri(_apiUrl);
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                //setup data object
                HttpContent content = new StreamContent(new MemoryStream(data));
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/octet-stream");
                //make request
                var response = await client.PostAsync(_apiUrl, content);
                //read response and write to view
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
                dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);
                dynamic faceObj = json[0].faceRectangle;
                dynamic scoreObj = json[0].scores;
                double[] scores = { scoreObj.anger, scoreObj.contempt, scoreObj.disgust,
                    scoreObj.fear, scoreObj.happiness, scoreObj.neutral, scoreObj.sadness, scoreObj.surprise };
                double max = 0;
                int maxIndex = -1;
                for (int i = 0; i < scores.Length; i++)
                {
                    if (scores[i] > max)
                    {
                        maxIndex = i;
                        max = scores[i];
                    }
                }
                /*List<double> scoreList = new List<double>(scores);
                int maxIndex = scoreList.IndexOf(scoreList.Max());*/

                string emotion;
                switch (maxIndex) {
                    case 0:
                        emotion = "anger";
                        break;
                    case 1:
                        emotion = "contempt";
                        break;
                    case 2:
                        emotion = "disgust";
                        break;
                    case 3:
                        emotion = "fear";
                        break;
                    case 4:
                        emotion = "happiness";
                        break;
                    case 5:
                        emotion = "neutral";
                        break;
                    case 6:
                        emotion = "sadness";
                        break;
                    case 7:
                        emotion = "surprise";
                        break;
                    default:
                        emotion = "";
                        break;
                }

                FacialData toReturn = new FacialData((int) faceObj.left, (int) faceObj.top, (int) faceObj.width, (int) faceObj.height, emotion);
                return toReturn;
            }

        }

        static async Task AsyncMethod(byte[] img)
        {
            Console.WriteLine(await AsyncFaceRequest(img));
        }

        public static void Main(string[] args)
        {
            try
            {
                byte[] img = imageToByteArray(Image.FromFile("face.jpg"));
                AsyncMethod(img).Wait();
            }
            catch (FileNotFoundException f)
            {
                Console.WriteLine(f);
            }
            
        }

        public static byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }
    }
}