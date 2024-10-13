namespace DataFileParser
{
    /// <summary>
    /// Представляет заголовок столбеца .csv или .txt файла, с которым сопоствленно свойство.
    /// </summary>
    /// <param name="columnName"></param>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnNameAttribute(string columnName) : Attribute
    {
        /// <summary>
        /// Представляет имя столбца, с которым сопоставленно свойство.
        /// </summary>
        public string ColumnName { get; } = columnName;
    }
}
