namespace Solution.Common.Misc {

    /**
    * <summary>
    * Defines a contract for objects that can be paused and resumed.
    * Includes a <see cref="Paused"/> state and default implementations for pause control.
    * </summary>
    */
    public interface IPausable {

        /**
        * <summary>
        * Gets or sets whether the object is currently paused.
        * </summary>
        * <value>
        * <c>true</c> if the object is paused; otherwise, <c>false</c>.
        * </value>
        */
        bool Paused { get; set; }

        /**
        * <summary>
        * Pauses the object by setting <see cref="Paused"/> to <c>true</c>.
        * </summary>
        */
        void OnPause() {
            Paused = true;
        }

        /**
        * <summary>
        * Resumes the object by setting <see cref="Paused"/> to <c>false</c>.
        * </summary>
        */
        void OnContinue() {
            Paused = false;
        }
    }
}
