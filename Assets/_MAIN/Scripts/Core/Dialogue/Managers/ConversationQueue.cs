using System.Collections.Generic;

namespace DIALOGUE
{
    public class ConversationQueue
    {
        private Queue<Conversation> conversationQueue = new Queue<Conversation>();
        public Conversation top => conversationQueue.Peek();//look and see the first item is

        public void Enqueue(Conversation conversation) => conversationQueue.Enqueue(conversation);
        public void EnqueuePriority(Conversation conversation)
        {
            Queue<Conversation> queue = new Queue<Conversation>();//new temporary queue
            queue.Enqueue(conversation);//add the new conversation to the temporary queue

            while (conversationQueue.Count > 0)
                queue.Enqueue(conversationQueue.Dequeue());

            conversationQueue = queue;//replace the old queue with the new one to the top of the queue
        }

        public void Dequeue()
        {
            if (conversationQueue.Count > 0)
                conversationQueue.Dequeue();
        }

        public bool IsEmpty() => conversationQueue.Count == 0;

        public void Clear() => conversationQueue.Clear(); 

        public Conversation[] GetReadOnly() => conversationQueue.ToArray(); 
    }
}