using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

public static class Base64ImageConverter
{
    public static async Task<byte[]> ConvertFromBase64StringAsync(string base64String)
    {
        if (base64String.Contains(","))
        {
            base64String = base64String.Split(',')[1];
        }

        byte[] imageBytes = Convert.FromBase64String(base64String);

        return await Task.FromResult(imageBytes);
    }
}
