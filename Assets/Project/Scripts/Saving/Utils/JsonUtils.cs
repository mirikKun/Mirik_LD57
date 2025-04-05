using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Project.Scripts.Saving.Utils
{
    public static class JsonUtils
    {
        public static JObject FindDiff(JToken newJson, JToken oldJson)
        {
            JObject diff1 = new JObject();
            if (JToken.DeepEquals(newJson, oldJson))
                return diff1;
            switch (newJson)
            {
                case JObject jobject2:
                    if (oldJson is JObject jobject1)
                    {
                        string[] array1 = jobject2.Properties().Select<JProperty, string>(c => c.Name).Except<string>(jobject1.Properties().Select<JProperty, string>(c => c.Name)).ToArray<string>();
                        string[] array2 = jobject1.Properties().Select<JProperty, string>((Func<JProperty, string>)(c => c.Name)).Except<string>(jobject2.Properties().Select<JProperty, string>((Func<JProperty, string>)(c => c.Name))).ToArray<string>();
                        string[] array3 = jobject2.Properties().Where<JProperty>((Func<JProperty, bool>)(c => JToken.DeepEquals(c.Value, oldJson[(object)c.Name]))).Select<JProperty, string>((Func<JProperty, string>)(c => c.Name)).ToArray<string>();
                        foreach (string str in array1)
                            diff1[str] = (JToken)new JObject()
                            {
                                ["new"] = newJson[(object)str]
                            };
                        
                        foreach (string str in array2)
                            diff1[str] = (JToken)new JObject()
                            {
                                ["old"] = oldJson[(object)str]
                            };
                        
                        using (IEnumerator<string> enumerator = jobject2.Properties()
                                   .Select<JProperty, string>((Func<JProperty, string>)(c => c.Name))
                                   .Except<string>((IEnumerable<string>)array1)
                                   .Except<string>((IEnumerable<string>)array3).GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                string current = enumerator.Current;
                                JObject diff2 = JsonUtils.FindDiff(jobject2[current], jobject1[current]);
                                if (diff2.HasValues)
                                    diff1[current] = (JToken)diff2;
                            }

                            break;
                        }
                    }
                    else
                        goto default;
                case JArray jarray4:
                    if (oldJson is JArray jarray3)
                    {
                        JArray jarray1 = new JArray((object)jarray4.Except<JToken>((IEnumerable<JToken>)jarray3, (IEqualityComparer<JToken>)new JTokenEqualityComparer()));
                        JArray jarray2 = new JArray((object)jarray3.Except<JToken>((IEnumerable<JToken>)jarray4, (IEqualityComparer<JToken>)new JTokenEqualityComparer()));
                        if (jarray1.HasValues)
                            diff1["new"] = (JToken)jarray1;
                        if (jarray2.HasValues)
                        {
                            diff1["old"] = (JToken)jarray2;
                            break;
                        }

                        break;
                    }

                    goto default;
                default:
                    diff1["new"] = newJson;
                    diff1["old"] = oldJson;
                    break;
            }

            return diff1;
        }
    }
}