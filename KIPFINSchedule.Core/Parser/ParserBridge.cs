using System.Security.Cryptography;
using System.Text;
using KIPFINSchedule.Core.Parser.Bypass;
using OfficeOpenXml;

namespace KIPFINSchedule.Core.Parser;

public class ParserBridge
{
    private string? _lastHash;
    private ExcelPackage? _excelPackage;
    private readonly PDFToExcel _pdfToExcel = new();
    private DateTime _lastUpdate = DateTime.Today.AddDays(-1);
    private KIPHttpClient? _httpClient;

    public ParserBridge(BypassCred bypassCred)
    {
        GetExcelPackage().GetAwaiter().GetResult();

        _httpClient = new KIPHttpClient(bypassCred);
    }

    private bool IsOutDatedByDate => _lastUpdate.AddHours(12) < DateTime.UtcNow;
    
    public async Task<ExcelPackage> GetExcelPackage()
    {
        if (!IsOutDatedByDate && KIPHttpClient.TempLinkUsed()) return _excelPackage!;

        _httpClient = _httpClient == null ? new KIPHttpClient(new BypassCred()) : _httpClient.CreateNewInstance();
        
        var bytes = await _httpClient.TryGetFile();
        var hash = Encoding.UTF8.GetString(MD5.HashData(bytes));

        if (_lastHash != null && _lastHash == hash && _excelPackage != null)
            return _excelPackage;
        
        _excelPackage = await _pdfToExcel.ParseFromFile(bytes);
        _lastHash = hash;
        
        _lastUpdate = DateTime.Today.AddDays(1);
        
        return _excelPackage;
    }
}