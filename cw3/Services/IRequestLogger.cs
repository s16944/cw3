namespace cw3.Services
{
    public interface IRequestLogger
    {
        void Log(string method, string path, string query, string body);
    }
}