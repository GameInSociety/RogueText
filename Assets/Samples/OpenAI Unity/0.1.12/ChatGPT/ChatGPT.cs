using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        public static ChatGPT Instance;

        private OpenAIApi openai = new OpenAIApi();

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "Don't mention anything about AI. Don't mention you're an AI. Without removing or changing words, give me a short, straight forward description of this :";

        public delegate void OnSendReply(string text);
        public OnSendReply onSendReply;

        private void Awake()
        {
            Instance = this;
        }

        public string GetText()
        {
            return messages[messages.Count - 1].Content;
        }

        public async void SendReply(string input)
        {
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = input
            };

            Debug.Log("sending message to ai : ");
            Debug.Log(input);

            if (messages.Count == 0) newMessage.Content = prompt + "\n" + input; 
            
            messages.Add(newMessage);

            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0301",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();
                
                messages.Add(message);

                Debug.Log("received : " + message.Content);

                TextInterpretor.InterpretText(message.Content);

                if (onSendReply != null)
                {
                    onSendReply(message.Content);
                }

            }
            else
            {
                Debug.LogError("No text was generated from this prompt.");
            }
        }
    }
}
