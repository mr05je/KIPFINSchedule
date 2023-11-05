using System.Globalization;
using System.Text;
using OfficeOpenXml;
using Serilog;

namespace KIPFINSchedule.Core.Parser;

public class ScheduleParser
{
    private readonly ParserBridge _bridge;
    private readonly ILogger _logger;
    
    public ScheduleParser(ParserBridge bridge, ILogger logger)
    {
        _bridge = bridge;
        _logger = logger;
    }

    public async Task<string> GenSchedule(string group, string messageFormat, string itemFormat)
    {
        var excelPackage = await _bridge.GetExcelPackage();
        
        var schedule = new StringBuilder();
        
        var date = DateTime.Today;
        var ci = new CultureInfo("ru-RU");
        
        try
        {
            date = DateTime.ParseExact(
                string.Concat(excelPackage.Workbook.Worksheets[0].Cells["A1"].GetValue<string>().Split(' ').SkipLast(1)
                    .Last()
                    .Where(x => char.IsDigit(x) || '.' == x)), "dd.MM.yyyy", ci);
        }
        catch
        {
            _logger.Warning("!!!Can't parse date!!!");
        }
        
        var audiences = excelPackage.Workbook.Worksheets[0].Cells["B:XFD"]
            .Where(x => x.Value is string value && value.Contains(group));

        audiences = audiences.OrderBy(x => x.LocalAddress[0]).ToList();
        
        
        if (!audiences.Any())
        {
            schedule.AppendLine("У тебя сегодня нет пар🥳");
            return schedule.ToString();
        }

        var items = new LinkedList<string>();

        var lastItem = "";

        foreach (var audience in audiences)
        {
            var time = SwitchPair(audience.LocalAddress[0].ToString(), group.Trim()[0].ToString(), date);

            var index = int.Parse(string.Join("", audience.LocalAddress.Where(char.IsDigit)));

            var audienceNum = excelPackage.Workbook.Worksheets[0].Cells[audience.LocalAddress[0] + (index + 1).ToString()].GetValue<string>();
            var teacherName = excelPackage.Workbook.Worksheets[0].Cells[$"A{index}"].GetValue<string>();

            var item = itemFormat.Replace("{{audience}}", audienceNum)
                .Replace("{{teacher}}", teacherName);
            
            if (lastItem == time)
            {
                items.AddLast(item);
            }
            else
            {
                items.AddLast(time);
                items.AddLast(item);
            }
            
            lastItem = time;
        }
        
        foreach (var item in items)
        {
            schedule.AppendLine(item);
        }

        var message = messageFormat
            .Replace("{{schedule}}", schedule.ToString())
            .Replace("{{date}}", date.ToString("dd MMMM yyyy", ci));
        
        return message;
    }
    
    private static string SwitchPair(string index, string course, DateTime dateParsed, string timeFormat = "{{index}} пара {{time}}")
    {
        return index.ToLower() switch
        {
            "b" => timeFormat.Replace("{{index}}", "1").Replace("{{time}}", dateParsed.DayOfWeek == DayOfWeek.Monday ? "(09:00-10:00)" : "(08:30-10:00)"),
            "c" => timeFormat.Replace("{{index}}", "2").Replace("{{time}}", "(10:10-11:40)"),
            "d" => timeFormat.Replace("{{index}}", "3").Replace("{{time}}", "(12:20-13:50)"),
            "e" => timeFormat.Replace("{{index}}", "4").Replace("{{time}}", dateParsed.DayOfWeek == DayOfWeek.Thursday ? course is "1" or "2" ? "(14:00-15:30)" : "(14:00-14:45)" : "(14:00-15:30)"),
            "f" => timeFormat.Replace("{{index}}", "5").Replace("{{time}}", dateParsed.DayOfWeek == DayOfWeek.Thursday ? course is "1" or "2" ? "(15:50-17:20)" : "(15:00-16:30)" : "(15:50-17:20)"),
            "g" => timeFormat.Replace("{{index}}", "6").Replace("{{time}}", dateParsed.DayOfWeek == DayOfWeek.Thursday ? "(16:40-18:10)" : "(17:30-19:00)"),
            "h" => timeFormat.Replace("{{index}}", "6").Replace("{{time}}", dateParsed.DayOfWeek == DayOfWeek.Thursday ? "неизвестная пара" : "(19:10-20:40)"),
            _ => "неизвестная пара"
        };
    }
}