using System.Net;
// ReSharper disable InconsistentNaming

namespace KIPFINSchedule.Core.Parser.Bypass;

public class KIPHttpClient
{
    private string FileLink =
        "http://www.fa.ru/org/spo/kip/Documents/raspisanie/%D0%90%D0%A3%D0%94%D0%98%D0%A2%D0%9E%D0%A0%D0%98%D0%98.pdf";

    private static string TempLink = "";

    private readonly BypassCred _bypassCred;
    private readonly HttpClient _client;

    public KIPHttpClient(BypassCred bypassCred)
    {
        _bypassCred = bypassCred;
        _client = new HttpClient(new HttpClientHandler
        {
            Credentials = new NetworkCredential(bypassCred.Login, bypassCred.Password, "http://fa.ru"),
            UseDefaultCredentials = false
        });
    }

    public async Task<byte[]> TryGetFile()
    {
        try
        {
            var file = await _client.GetByteArrayAsync(TempLink == "" ? FileLink : TempLink);

            return file;
        }
        catch
        {
            return Array.Empty<byte>();
        }
    }

    public KIPHttpClient CreateNewInstance()
    {
        return new KIPHttpClient(_bypassCred);
    }

    public static void SetTempLink(string tempLink)
    {
        TempLink = tempLink;
    }

    public static bool TempLinkUsed() => TempLink != "";
}