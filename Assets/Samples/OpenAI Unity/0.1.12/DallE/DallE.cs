using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace OpenAI {

    public class DallE : MonoBehaviour {
        public static DallE Instance;

        private void Awake() {
            Instance = this;
        }

        [SerializeField] private Image image;

        private OpenAIApi openai = new OpenAIApi();

        public async void SendImageRequest(string prompt) {
            image.sprite = null;

            prompt = "a human ham breaking up with dog pineapple";

            var response = await openai.CreateImage(new CreateImageRequest {
                Prompt = prompt,
                Size = ImageSize.Size1024
            });

            if (response.Data != null && response.Data.Count > 0) {
                using (var request = new UnityWebRequest(response.Data[0].Url)) {
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Access-Control-Allow-Origin", "*");
                    _ = request.SendWebRequest();

                    while (!request.isDone) await Task.Yield();

                    var texture = new Texture2D(2, 2);
                    _ = texture.LoadImage(request.downloadHandler.data);
                    var sprite = Sprite.Create(texture, new Rect(0, 0, 1024, 1024), Vector2.zero, 1f);
                    image.sprite = sprite;
                }
            } else {
                Debug.LogWarning("No image was created from this prompt.");
            }

            Debug.Log("image finished loading");

        }
    }
}
