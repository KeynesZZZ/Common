namespace Match3.Interfaces
{
    public interface IItemsPool<TItem>
    {
        TItem GetItem();
        void ReturnItem(TItem item);
    }
}