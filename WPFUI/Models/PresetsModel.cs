using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFUI.Models
{
    public class PresetsModel
    {
        StringCollection _presets;

        public PresetsModel(StringCollection presets)
        {
            _presets = presets;
        }

        // 1. enter the name of the preset to the StringCollection aray
        // 2. build the string that contains the data
        // 3. add the units of the preset to the StringCollection
        public void SaveNewPreset(string name, double[] vals, string units)
        {
            // string name, double[] vals, string units
            _presets.Add(name);

            string data = "";

            for (int i = 0; i < vals.Length; i++)
            {
                data += vals[i] + ",";
            }

            _presets.Add(data);
            _presets.Add(units);
        }

        public void UpdatePreset(string name, double[] vals, string units)
        {
            int index = _presets.IndexOf(name);
            string temp = "";
            for (int i = 0; i < vals.Length; i++)
            {
                temp += vals[i] + ",";
            }

            _presets[index + 1] = temp;
            _presets[index + 2] = units;
        }

        public void DeletePreset(string presetName)
        {
            int presetIndex = _presets.IndexOf(presetName);
            _presets.RemoveAt(presetIndex);
            _presets.RemoveAt(presetIndex);
            _presets.RemoveAt(presetIndex);
        }

        public void DeletePresetAtIndex(int index)
        {
            _presets.RemoveAt(index);
        }

        public ObservableCollection<string> GetPresetNames()
        {
            ObservableCollection<string> names = new ObservableCollection<string>();
            for (int i = 0; i < _presets.Count; i += 3)
            {
                names.Add(_presets[i]);
            }
            return names;
        }

        // first iterate through each string in the presets list looking for the preset that matches the parameter presetName
        // then iterates through the next string (trajectory data string), adding individual chars (or one character strings) to a temporary string until it hits a ","
        // once it hits the comma, it should Parse the temp string to a double and add that double to the list of values
        public double[] GetValues(string presetName)
        {
            List<double> values = new List<double>();
            for (int i = 0; i < _presets.Count; i++)
            {
                if (_presets[i].Equals(presetName))
                {
                    //found the preset name
                    //time to iterate through the following string
                    string temp = "";
                    string trajectoryData = _presets[i + 1];
                    //iterate through trajectoryData string
                    for (int v = 0; v < trajectoryData.Length; v++)
                    {
                        //unless we have found a comma "value separator" add the current digit to temp
                        if (trajectoryData[v] != ',')
                        {
                            temp += trajectoryData[v];
                        }
                        //we have hit a value separator and need to add temp to our array of values as a double
                        else
                        {
                            values.Add(Convert.ToDouble(temp));
                            temp = "";
                        }
                    }
                    break;
                }
            }

            return values.ToArray();
        }

        public string GetUnits(string presetName)
        {
            string units = "";
            for (int i = 0; i < _presets.Count; i++)
            {
                if (_presets[i].Equals(presetName))
                {
                    units = _presets[i + 2];
                }
            }
            return units;
        }

        public bool ContainsName(string presetName)
        {
            return _presets.Contains(presetName);
        }

        public void ClearAllPresets()
        {
            _presets.Clear();
        }

    }
}
