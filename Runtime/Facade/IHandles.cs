namespace Solution.Common.Facade {
    
    /**
    * <summary>
    * Marker that this sub-service handles a specific payload type.
    * Implement on your sub-service class to signal which payload it handles.
    * </summary>
    */
    public interface IHandles<TPayload> where TPayload : ScriptablePayload { }
}