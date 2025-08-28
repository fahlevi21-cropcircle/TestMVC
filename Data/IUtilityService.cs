namespace TestMVC.Data
{
    public interface IUtilityService
    {
        public byte[] ExportExcel<T>(Dictionary<string, string> headerKeys,IEnumerable<T> list, string filename);
        public byte[] ExportPDF<T>(Dictionary<string, string> headerKeys, IEnumerable<T> list, string filename);
        public Task SendEmail(string recipent, string subject, string body);
        //import excel
    }
}
