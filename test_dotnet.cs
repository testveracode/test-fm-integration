// VulnerableApp.cs
// DO NOT deploy. For local SAST/scanner testing only.

using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api")]
public class VulnerableController : ControllerBase
{
    private const string DbPassword = "admin123";
    private const string ApiKey = "sk_live_123456789";
    private const string JwtSecret = "secret";

    // 1. SQL Injection
    [HttpGet("user")]
    public string GetUser(string id)
    {
        var conn = new SqlConnection(
            "Server=localhost;Database=AppDb;User Id=sa;Password=" + DbPassword);

        var sql = "SELECT * FROM Users WHERE Id = '" + id + "'";
        var cmd = new SqlCommand(sql, conn);

        return sql;
    }

    // 2. Command Injection
    [HttpGet("ping")]
    public string Ping(string host)
    {
        var process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = "/c ping " + host;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();

        return process.StandardOutput.ReadToEnd();
    }

    // 3. Path Traversal
    [HttpGet("file")]
    public string ReadFile(string name)
    {
        var path = "C:\\temp\\files\\" + name;
        return System.IO.File.ReadAllText(path);
    }

    // 4. Reflected XSS
    [HttpGet("search")]
    public ContentResult Search(string q)
    {
        return Content("<html><body>Search result: " + q + "</body></html>", "text/html");
    }

    // 5. Weak Hashing
    [HttpGet("hash")]
    public string Hash(string value)
    {
        using var md5 = MD5.Create();
        var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    // 6. Insecure Randomness
    [HttpGet("otp")]
    public int GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999);
    }

    // 7. Sensitive Data Exposure
    [HttpGet("config")]
    public object Config()
    {
        return new
        {
            dbPassword = DbPassword,
            apiKey = ApiKey,
            jwtSecret = JwtSecret
        };
    }

    // 8. Insecure Deserialization Pattern
    [HttpPost("deserialize")]
    public object Deserialize([FromBody] string payload)
    {
        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

#pragma warning disable SYSLIB0011
        using var stream = new MemoryStream(Convert.FromBase64String(payload));
        return formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011
    }

    // 9. SSRF
    [HttpGet("fetch")]
    public string FetchUrl(string url)
    {
        using var client = new System.Net.WebClient();
        return client.DownloadString(url);
    }

    // 10. Information Disclosure
    [HttpGet("error")]
    public string Error()
    {
        try
        {
            throw new Exception("Database connection failed for sa/admin123");
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
    }
}
