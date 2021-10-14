
namespace libSyslogServer
{


    /// <summary>
    /// Commonly used static methods.
    /// </summary>
    public static class Common
    { 


        public static string SerializeJson(object obj)
        {
            if (obj == null) return null;
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(
                obj,
                Newtonsoft.Json.Formatting.Indented,
                new Newtonsoft.Json.JsonSerializerSettings
                {
                    DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc
                });

            return json;
        }
          
        public static T DeserializeJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) throw new System.ArgumentNullException(nameof(json));

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            catch (System.Exception)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("Exception while deserializing:");
                System.Console.WriteLine(json);
                System.Console.WriteLine("");
                throw;
            }
        }

        public static T DeserializeJson<T>(byte[] data)
        {
            if (data == null || data.Length < 1) throw new System.ArgumentNullException(nameof(data));
            return DeserializeJson<T>(System.Text.Encoding.UTF8.GetString(data));
        }
          
        public static bool InputBoolean(string question, bool yesDefault)
        {
            System.Console.Write(question);

            if (yesDefault) System.Console.Write(" [Y/n]? ");
            else System.Console.Write(" [y/N]? ");

            string userInput = System.Console.ReadLine();

            if (string.IsNullOrEmpty(userInput))
            {
                if (yesDefault) return true;
                return false;
            }

            userInput = userInput.ToLower();

            if (yesDefault)
            {
                if (
                       (System.String.Compare(userInput, "n") == 0)
                    || (System.String.Compare(userInput, "no") == 0)
                   )
                {
                    return false;
                }

                return true;
            }
            else
            {
                if (
                       (System.String.Compare(userInput, "y") == 0)
                    || (System.String.Compare(userInput, "yes") == 0)
                   )
                {
                    return true;
                }

                return false;
            }
        }

        public static string InputString(string question, string defaultAnswer, bool allowNull)
        {
            while (true)
            {
                System.Console.Write(question);

                if (!string.IsNullOrEmpty(defaultAnswer))
                {
                    System.Console.Write(" [" + defaultAnswer + "]");
                }

                System.Console.Write(" ");

                string userInput = System.Console.ReadLine();

                if (string.IsNullOrEmpty(userInput))
                {
                    if (!string.IsNullOrEmpty(defaultAnswer)) return defaultAnswer;
                    if (allowNull) return null;
                    else continue;
                }

                return userInput;
            }
        }

        public static int InputInteger(string question, int defaultAnswer, bool positiveOnly, bool allowZero)
        {
            while (true)
            {
                System.Console.Write(question);
                System.Console.Write(" [" + defaultAnswer + "] ");

                string userInput = System.Console.ReadLine();

                if (string.IsNullOrEmpty(userInput))
                {
                    return defaultAnswer;
                }

                int ret = 0;
                if (!int.TryParse(userInput, out ret))
                {
                    System.Console.WriteLine("Please enter a valid integer.");
                    continue;
                }

                if (ret == 0)
                {
                    if (allowZero)
                    {
                        return 0;
                    }
                }

                if (ret < 0)
                {
                    if (positiveOnly)
                    {
                        System.Console.WriteLine("Please enter a value greater than zero.");
                        continue;
                    }
                }

                return ret;
            }
        } 
    }
}
