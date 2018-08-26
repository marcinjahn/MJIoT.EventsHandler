namespace MjIot.EventsHandler.ValueModifiers
{
    public class CustomCalculationInput<T>
    {
        public CustomCalculationInput(T x)
        {
            this.x = x;
        }

        public T x { get; set; }
    }
}