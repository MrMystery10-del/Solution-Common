namespace Solution.Common.Misc {

    /**
    * <summary>
    * Represents a container that holds a single item of type <typeparamref name="T"/>.
    * </summary>
    */
    public interface IHolder<T> {
    
        protected T Item { get; set; }

        void Hold(T item) => Item = item;
    }
}