using System.Text.Json;

public static class StreamExtensions
{
    public static async Task<string> ReadAsStringAsync(this Stream stream)
    {
        using (var reader = new StreamReader(stream))
            return await reader.ReadToEndAsync();
    }
    public static async Task<string> ReadAsJsonAsync(this Stream stream)
    {
        return (string)JsonSerializer.Deserialize(await ReadAsStringAsync(stream), typeof(string), (JsonSerializerOptions)null);
    }
}