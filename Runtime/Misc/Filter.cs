namespace Solution.Common.Misc {

    /**
    * <summary>
    * Abstract base class representing a filter applied to a subject.
    * </summary>
    * <typeparam name="TSubject">The type of subject to filter.</typeparam>
    */
    public abstract class Filter<TSubject> {
    
        private volatile bool readyForDispose;

        public bool ReadyForDispose {
            get {
                return readyForDispose;
            } 
            protected set {
                readyForDispose = value;
            }
        }

        public abstract void DoFilter(TSubject subject);
    }
}