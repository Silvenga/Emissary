namespace Emissary
{
    public static class EmissaryExtensions
    {
        public static string ToShortContainerName(this string containerId)
        {
            return containerId?.Substring(0, 12);
        }
    }
}