using System.Data.SqlClient;

namespace cw3.DAL.Parsers
{
    public interface SqlRowParser<out T>
    {
        T Parse(SqlDataReader reader, int currentIndex);
    }
}