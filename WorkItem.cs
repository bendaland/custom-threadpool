namespace CustomThreadpool
{
    public delegate void WorkDelegate(object WorkObject);

    public class WorkItem
    {
        public object WorkObject;
        public WorkDelegate Delegate;
    }
}
