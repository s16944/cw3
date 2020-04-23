using System;
using System.Data.SqlClient;
using cw3.Models;

namespace cw3.DAL.Parsers
{
    public class StudiesSqlRowParser : SqlRowParser<Studies>
    {
        public Studies Parse(SqlDataReader reader, int currentIndex) =>
            new Studies
            {
                IdStudies = int.Parse(reader["IdStudy"].ToString()),
                Name = reader["Name"].ToString()
            };
    }
}