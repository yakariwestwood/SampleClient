namespace SampleClient.Helpers
{
    public static class ConvertHelper
    {
        public static short BoolToShort(bool value)
        {
            return value ? (short) 1 : (short) 0;
        }

        public static short BoolToShort(bool? value)
        {
            if (!value.HasValue)
                return 0;

            return value.Value ? (short) 1 : (short) 0;
        }
    }
}