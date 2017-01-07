public interface IListChangeListener<T> {
    void onListItemAdded(T newObject);
    void onListItemRemoved(T oldObject);
}
