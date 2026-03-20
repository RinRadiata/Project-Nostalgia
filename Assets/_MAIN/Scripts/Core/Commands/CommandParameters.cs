using System.Collections.Generic;


namespace COMMANDS
{
    public class CommandParameters
    {
        private const char PARAMETER_IDENTIFIER = '-';

        private Dictionary<string, string> parameters = new Dictionary<string, string>();
        private List<string> unlabledParameters = new List<string>();

        public CommandParameters(string[] parameterArray, int startingIndex = 0)
        {
            for (int i = startingIndex; i < parameterArray.Length; i++)
            {
                // Check if the parameter starts with the identifier and is not a number, we can assume it's an identifier
                if (parameterArray[i].StartsWith(PARAMETER_IDENTIFIER) && !float.TryParse(parameterArray[i], out _))
                {
                    string pName = parameterArray[i];
                    string pValue = "";

                    // Check if the next parameter is a value or another parameter
                    if (i + 1 < parameterArray.Length && !parameterArray[i + 1].StartsWith(PARAMETER_IDENTIFIER))
                    {
                        pValue = parameterArray[i + 1];
                        i++;
                    }

                    // Add the parameter to the dictionary
                    parameters.Add(pName, pValue);
                }
                else
                    unlabledParameters.Add(parameterArray[i]);
            }
        }

        public bool TryGetValue<T>(string parameterName, out T value, T defaultValue = default(T)) => TryGetValue(new string[] { parameterName }, out value, defaultValue);

        public bool TryGetValue<T>(string[] parameterNames, out T value, T defaultValue = default(T))
        {
            foreach (string parameterName in parameterNames)
            {
                if (parameters.TryGetValue(parameterName, out string parameterValue))
                {
                    if (TryCastParameter(parameterValue, out value))
                    {
                        return true;
                    }
                }
            }

            // if we reach here, no match was found in the identified parameters so search the unlabeled ones if present
            foreach (string parameterName in unlabledParameters)
            {
                if (TryCastParameter(parameterName, out value))
                {
                    unlabledParameters.Remove(parameterName); // remove the parameter from the unlabeled list as it has been used
                    return true;
                }
            }

            value = defaultValue;
            return false;
        }

        private bool TryCastParameter<T>(string parameterValue, out T value)
        {
            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(parameterValue, out bool boolValue))
                {
                    value = (T)(object)boolValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (int.TryParse(parameterValue, out int intValue))
                {
                    value = (T)(object)intValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(float))
            {
                if (float.TryParse(parameterValue, out float floatValue))
                {
                    value = (T)(object)floatValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(string))
            {
                value = (T)(object)parameterValue;
                return true;
            }

            value = default(T);
            return false;
        }

    }

}