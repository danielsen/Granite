namespace Granite.Core
{
    public class LocalOptions<T> : Options<T>
    {
        public SerializationType SerializationType { get; set; }

        public LocalOptions()
        {
            SerializationType = SerializationType.NewtonsoftJson;
        }
    }
}