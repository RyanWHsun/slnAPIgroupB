using Newtonsoft.Json.Linq;

namespace prjGroupB {
    public static class EnvironmentConfig {
        private static JObject _configData;

        static EnvironmentConfig() {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "environment", "environment.txt");

            if (File.Exists(filePath)) {
                string jsonContent = File.ReadAllText(filePath);

                try {
                    _configData = JObject.Parse(jsonContent);
                }
                catch (Exception ex) {
                    Console.WriteLine($"解析 environment.txt 時發生錯誤: {ex.Message}");
                    _configData = new JObject();
                }
            }
            else {
                Console.WriteLine("environment.txt 未找到！");
                _configData = new JObject();
            }
        }

        public static string GetValue(string section, string key) {
            return _configData[section]?[key]?.ToString();
        }
    }
}
