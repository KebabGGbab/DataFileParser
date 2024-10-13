using System.Reflection;
using System.Text;

namespace DataFileParser
{
    /// <summary>
    /// Конвертирует данные из .csv и .txt файлов в объекты на основании сопоставления столбцов файла свойствам объекта
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FileColumnParser<T> where T : class, new()
    {
        static FileColumnParser()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Инициализирует объекты на основании данных из файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <param name="encoding">Название кодировки, в которой будет прочитан файл.</param>
        /// <param name="separator">Строка, являющаяся разделителем столбцов файла.</param>
        /// <returns>Коллекция объектов.</returns>
        public static ICollection<T> GetObjects(string filePath, string encoding, string separator)
        {
            return GetObjects(filePath, Encoding.GetEncoding(encoding), separator);
        }

        /// <summary>
        /// Инициализирует объекты на основании данных из файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <param name="encoding">Кодировка, в которой будет прочитан файл.</param>
        /// <param name="separator">Строка, являющаяся разделителем столбцов файла.</param>
        /// <returns>Коллекция объектов.</returns>
        public static ICollection<T> GetObjects(string filePath, Encoding encoding, string separator)
        {
            ValidationFile(filePath);
            using StreamReader sr = new(filePath, encoding);
            string[] nameColumns = GetNameColumns(sr, separator);
            ICollection<T> collection = [];
            PropertyInfo[] properties = FileColumnParser<T>.ComparePropertiesAndColumns(nameColumns);
            while (!sr.EndOfStream)
            {
                string? line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                string[] lineSplit = line.Split(separator);
                T obj = new();
                for (int i = 0; i < properties.Length && i < lineSplit.Length; i++)
                {
                    properties[i].SetValue(obj, lineSplit[i]);
                }

                collection.Add(obj);
            }

            return collection;
        }

        /// <summary>
        /// Получает названия столбцов в файле.
        /// </summary>
        /// <param name="sr">Объект StreamReader.</param>
        /// <param name="separator">Строка, являющаяся разделителем столбцов файла</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static string[] GetNameColumns(StreamReader sr, string separator)
        {
            string? line = sr.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                throw new Exception("Первая строка не должна быть пустой, она должна содержать имена столбцов.");
            }
            return line.Split(separator);
        }

        /// <summary>
        /// Сопоставляет имена свойств объекта названиям столбцов файла для формирования правильной последовательности.
        /// </summary>
        /// <param name="nameColumns"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static PropertyInfo[] ComparePropertiesAndColumns(string[] nameColumns)
        {
            Dictionary<string, PropertyInfo> properties = typeof(T).GetProperties().Where(property => property.GetCustomAttribute<ColumnNameAttribute>() != null)
                            .ToDictionary(property => property.GetCustomAttribute<ColumnNameAttribute>()!.ColumnName, property => property);

            return nameColumns.Select(column => properties.TryGetValue(column, out PropertyInfo? prop)
                            ? prop
                            : throw new Exception($"Не найдено соответствующее свойство для столбца '{column}'"))
                            .ToArray();
        }

        /// <summary>
        /// Производит валидацию файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private static void ValidationFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл не найден.", filePath);
            }

            if (!Path.GetExtension(filePath).Equals(".csv", StringComparison.CurrentCultureIgnoreCase) && !Path.GetExtension(filePath).Equals(".txt", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new ArgumentException("Расширение файла должно быть '.csv' или '.txt'.", filePath);
            }
        }
    }
}
