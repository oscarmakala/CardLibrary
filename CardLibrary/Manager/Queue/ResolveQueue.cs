namespace CardLibrary.Manager;

public class ResolveQueue
{
    private Pool<CallbackQueueElement> callback_elem_pool = new Pool<CallbackQueueElement>();
    private Queue<CallbackQueueElement> callback_queue = new Queue<CallbackQueueElement>();

    public class CallbackQueueElement
    {
        public Action callback;
    }
}