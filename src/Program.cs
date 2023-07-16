using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Router;

internal static class Program
{
    private static HttpClientHandler _handler;
    private static HttpClient _client;

    public static async Task Main()
    {
        _handler = new HttpClientHandler()
        {
            UseCookies = false,
        };

        _client = new HttpClient(_handler);

        Console.WriteLine("Enter the username/password for the Admin control panel.");
        
        Console.Write("Username: ");
        string username = Console.ReadLine() ?? "admin";
        
        Console.Write("Password: ");
        string password = Console.ReadLine() ?? "password";
        
        // Get nonce from http://192.168.1.1/get_nonce.cmd
        string nonce = await GetNonce();

        Console.WriteLine("Nonce: " + nonce);

        // MD5 hash username:nonce:password
        string auth = GetAuth(username, nonce, password);

        Console.WriteLine("Auth: " + auth);

        // Post Username Auth Nonce to http://192.168.1.1/login.cgi (Sets Authorization cookie for login)
        string cookie = await GetCookie(username, auth, nonce);

        Console.WriteLine("Cookie: " + cookie);

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Cookie", cookie);

        int i = 0;
        
        // Fade on and off
        while (true)
        {
            do
            {
                await UpdateLed(true, i);
                
                i += 5;
            } while (i <= 100);

            do
            {
                await UpdateLed(true, i);

                i -= 5;
            } while (i >= 0);
        }
    }

    private static async Task UpdateLed(bool enabled, int? brightness = null)
    {
        LedPayload payload = new LedPayload()
        {
            Action = "set",
            Brightness = brightness,
            Led = enabled ? 1 : 0
        };
        
        string content = JsonSerializer.Serialize(payload, new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        
        HttpResponseMessage response = await _client.PostAsync("http://192.168.1.1/advanced_controls.cmd", new StringContent(content));

        response.EnsureSuccessStatusCode();
    }
    
    private static async Task<string> GetNonce()
    {
        HttpResponseMessage response = await _client.PostAsync("http://192.168.1.1/get_nonce.cmd", null);

        return await response.Content.ReadAsStringAsync();
    }

    private static string GetAuth(string username, string nonce, string password)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(username + ':' + nonce + ':' + password);
        
        byte[] hashBytes = MD5.HashData(inputBytes);

        return Convert.ToHexString(hashBytes).ToLower();
    }

    private static async Task<string> GetCookie(string username, string auth, string nonce)
    {
        Dictionary<string, string> authPayload = new Dictionary<string, string>
        {
            { "Username", username },
            { "auth", auth },
            { "nonce", nonce },
        };

        FormUrlEncodedContent content = new FormUrlEncodedContent(authPayload);

        HttpResponseMessage response = await _client.PostAsync("http://192.168.1.1/login.cgi", content);

        response.EnsureSuccessStatusCode();

        return response.Headers.GetValues("Set-Cookie").First();
    }
}

internal class LedPayload
{
    [JsonPropertyName("action")] 
    public string Action { get; set; } = "set";

    [JsonPropertyName("brightness")] 
    public int? Brightness { get; set; }
    
    [JsonPropertyName("led")]
    public int Led { get; set; }
}
