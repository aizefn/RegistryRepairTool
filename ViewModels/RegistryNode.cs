using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace RegistryRepairTool.ViewModels
{
    public class RegistryNode : ObservableObject
    {
        private bool _isExpanded;
        private bool _isSelected;
        private bool _childrenLoaded;
        private RegistryValue _selectedValue;

        public string Name { get; }
        public string FullPath => IsRoot ? Name : $"{Parent.FullPath}\\{Name}";
        public RegistryKey Key { get; }
        public RegistryNode Parent { get; }
        public bool IsRoot => Parent == null;

        public ObservableCollection<RegistryNode> Children { get; } = new ObservableCollection<RegistryNode>();
        public ObservableCollection<RegistryValue> Values { get; } = new ObservableCollection<RegistryValue>();

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value) && value && !_childrenLoaded)
                {
                    LoadChildren();
                    _childrenLoaded = true;
                }
            }
        }
        public RegistryValue SelectedValue
        {
            get => _selectedValue;
            set
            {
                if (SetProperty(ref _selectedValue, value))
                {
                    OnPropertyChanged(nameof(SelectedValue));
                }
            }
        }
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value) && value)
                {
                    LoadValues();
                }
            }
        }

       

        public RegistryNode(string name, RegistryKey key, RegistryNode parent = null)
        {
            Name = name;
            Key = key;
            Parent = parent;

            // Добавляем заглушку для отображения стрелки раскрытия
            if (!IsRoot && key.SubKeyCount > 0)
            {
                Children.Add(new RegistryNode("Loading...", null));
            }
        }

        public void LoadChildren()
        {
            Children.Clear();

            try
            {
                if (Key == null)
                {
                    Children.Add(new RegistryNode("Key not available", null));
                    return;
                }

                var subKeyNames = Key.GetSubKeyNames();
                foreach (var subKeyName in subKeyNames)
                {
                    try
                    {
                        var subKey = Key.OpenSubKey(subKeyName);
                        if (subKey != null)
                        {
                            var childNode = new RegistryNode(subKeyName, subKey, this);
                            Children.Add(childNode);
                        }
                    }
                    catch (Exception ex)
                    {
                        Children.Add(new RegistryNode($"{subKeyName} (Access denied)", null));
                    }
                }
            }
            catch (Exception ex)
            {
                Children.Add(new RegistryNode("Error loading children", null));
            }
        }

        public void LoadValues()
        {
            Values.Clear();

            try
            {
                if (Key == null)
                {
                    Values.Add(new RegistryValue { Name = "Error", Value = "Key not available", Kind = RegistryValueKind.String });
                    return;
                }

                // Получаем значения из ключа
                var valueNames = Key.GetValueNames();
                foreach (var valueName in valueNames)
                {
                    try
                    {
                        var kind = Key.GetValueKind(valueName);
                        var value = Key.GetValue(valueName);

                        Values.Add(new RegistryValue
                        {
                            Name = string.IsNullOrEmpty(valueName) ? "(По умолчанию)" : valueName,
                            Value = value,
                            Kind = kind
                        });
                    }
                    catch (Exception ex)
                    {
                        Values.Add(new RegistryValue
                        {
                            Name = valueName,
                            Value = $"Error reading value: {ex.Message}",
                            Kind = RegistryValueKind.String
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Values.Add(new RegistryValue
                {
                    Name = "Error",
                    Value = $"No access to read values: {ex.Message}",
                    Kind = RegistryValueKind.String
                });
            }
        }

        public class RegistryValue
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public RegistryValueKind Kind { get; set; }

            public override string ToString()
            {
                return $"{Name} ({Kind}): {Value}";
            }
        }
    }
}
