using System.ComponentModel;

namespace SubReddit.Common
{
    public static class ExportToCSV 
    {
        public static void ExportPosts<T>(T[] data, string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path += "Statistics";
            var lines = new List<String>();
            IEnumerable<PropertyDescriptor> props = TypeDescriptor.GetProperties(typeof(T)).OfType<PropertyDescriptor>();
            var header = string.Join(",", props.ToList().Select(x => x.Name));
            lines.Add(header);
            var valueLines = data.Select(row => string.Join(",", header.Split(',').Select(a => row.GetType().GetProperty(a).GetValue(row, null))));
            lines.AddRange(valueLines);
            using (var writer = new StreamWriter(path))
            {
                foreach (var line in lines)
                    writer.WriteLine(line);
                writer.Close();
            }
        }
    }
}
