using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using RegistryRepairTool.Views;

namespace RegistryRepairTool.ViewModels
{
    public class RegistryEditViewModel : ObservableObject
    {
        private static RegistryEditViewModel _instance;
        public static RegistryEditViewModel Instance => _instance ??= new RegistryEditViewModel();

        private RegistryNode _selectedNode;
        private string _currentPath;
        private bool _isAdmin;

        public ObservableCollection<RegistryNode> RootNodes { get; } = new ObservableCollection<RegistryNode>();
        public ICommand NavigateCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CreateKeyCommand { get; }
        public ICommand DeleteKeyCommand { get; }
        public ICommand CreateValueCommand { get; }
        public ICommand EditValueCommand { get; }
        public ICommand DeleteValueCommand { get; }
        public ICommand ExportCommand { get; }
        public bool IsAdmin => _isAdmin;

        public RegistryNode SelectedNode
        {
            get => _selectedNode;
            set => SetProperty(ref _selectedNode, value);
        }

        public string CurrentPath
        {
            get => _currentPath;
            set => SetProperty(ref _currentPath, value);
        }

        public RegistryEditViewModel()
        {
            CopyPathCommand = new RelayCommand(CopySelectedPath, () => SelectedNode != null);

            NavigateCommand = new RelayCommand(NavigateToPath);
            RefreshCommand = new RelayCommand(LoadRegistry);
            CreateKeyCommand = new RelayCommand(CreateKey, CanEditRegistry);
            DeleteKeyCommand = new RelayCommand(DeleteKey, CanDeleteKey);
            CreateValueCommand = new RelayCommand(CreateValue, CanEditRegistry);
            EditValueCommand = new RelayCommand(EditValue, CanEditValue);
            DeleteValueCommand = new RelayCommand(DeleteValue, CanDeleteValue);
            ExportCommand = new RelayCommand(ExportRegistry);
            // Проверка прав администратора
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            _isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!_isAdmin)
            {
                MessageBox.Show("Для полного функционала рекомендуется запустить приложение от имени администратора.",
                               "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            LoadRegistry();
        }

        private bool CanEditRegistry() => SelectedNode != null;

        private void LoadRegistry()
        {
            RootNodes.Clear();

            var hives = new[] {
                Registry.ClassesRoot,
                Registry.CurrentUser,
                Registry.LocalMachine,
                Registry.Users,
                Registry.CurrentConfig
            };

            foreach (var hive in hives)
            {
                var node = new RegistryNode(hive.Name, hive);
                RootNodes.Add(node);
            }
        }

        private void NavigateToPath()
        {
            if (string.IsNullOrWhiteSpace(CurrentPath))
                return;

            try
            {
                var pathParts = CurrentPath.Split('\\');
                if (pathParts.Length == 0)
                    return;

                // Удаляем возможные пробелы в начале/конце
                CurrentPath = CurrentPath.Trim();

                RegistryKey baseKey = GetBaseKey(pathParts[0]);
                if (baseKey == null)
                {
                    MessageBox.Show($"Неизвестный корневой раздел: {pathParts[0]}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Находим соответствующий корневой узел в дереве
                var rootNode = RootNodes.FirstOrDefault(n => n.Key == baseKey);
                if (rootNode == null)
                {
                    MessageBox.Show($"Корневой раздел не найден в дереве: {pathParts[0]}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Раскрываем корневой узел
                rootNode.IsExpanded = true;
                rootNode.IsSelected = true;

                RegistryNode targetNode = rootNode;

                // Переходим по пути
                foreach (var part in pathParts.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(part))
                        continue;

                    // Загружаем дочерние элементы, если они еще не загружены
                    if (!targetNode.Children.Any())
                    {
                        targetNode.LoadChildren();
                    }

                    var nextNode = targetNode.Children.FirstOrDefault(c =>
                        c.Name.Equals(part, StringComparison.OrdinalIgnoreCase));

                    if (nextNode == null)
                    {
                        MessageBox.Show($"Раздел '{part}' не найден в '{targetNode.FullPath}'", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    nextNode.IsExpanded = true;
                    nextNode.IsSelected = true;
                    targetNode = nextNode;
                }

                // Устанавливаем выбранный узел
                SelectedNode = targetNode;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перехода по пути: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ICommand CopyPathCommand { get; }
        private void CopySelectedPath()
        {
            if (SelectedNode != null && !string.IsNullOrEmpty(SelectedNode.FullPath))
            {
                try
                {
                    Clipboard.SetText(SelectedNode.FullPath);

                    // Вызываем событие для показа уведомления
                    RequestShowCopyNotification?.Invoke(this, EventArgs.Empty);
                }
                catch
                {
                    // Обработка ошибок
                }
            }
        }
        public event EventHandler RequestShowCopyNotification;

        private RegistryKey GetBaseKey(string hiveName)
        {
            return hiveName.ToUpper() switch
            {
                "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                "HKEY_CURRENT_USER" => Registry.CurrentUser,
                "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                "HKEY_USERS" => Registry.Users,
                "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
                _ => null
            };
        }

        private void CreateKey()
        {
            if (SelectedNode == null)
            {
                MessageBox.Show("Не выбран раздел для создания ключа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var newKeyName = Microsoft.VisualBasic.Interaction.InputBox("Введите имя нового раздела:", "Создание раздела");
                if (string.IsNullOrWhiteSpace(newKeyName))
                {
                    MessageBox.Show("Имя раздела не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (SelectedNode.Key.GetSubKeyNames().Contains(newKeyName))
                {
                    MessageBox.Show($"Раздел '{newKeyName}' уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newKey = SelectedNode.Key.CreateSubKey(newKeyName);
                if (newKey == null)
                {
                    MessageBox.Show("Не удалось создать раздел", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                SelectedNode.LoadChildren();
                MessageBox.Show("Раздел успешно создан", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Недостаточно прав для создания раздела. Запустите приложение от имени администратора.",
                              "Ошибка прав доступа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания раздела: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDeleteKey() => SelectedNode != null && !SelectedNode.IsRoot && IsAdmin;

        private void DeleteKey()
        {
            if (SelectedNode == null || SelectedNode.IsRoot)
                return;

            try
            {
                if (MessageBox.Show($"Вы уверены, что хотите удалить раздел '{SelectedNode.Name}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    SelectedNode.Parent.Key.DeleteSubKeyTree(SelectedNode.Name);
                    SelectedNode.Parent.LoadChildren();
                    SelectedNode = SelectedNode.Parent;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления раздела: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateValue()
        {
            if (SelectedNode == null)
                return;

            var dialog = new ValueEditDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Получаем корневой раздел
                    var rootHive = GetRootHive(SelectedNode.Key.Name);

                    // Открываем базовый ключ с правами на запись
                    using (var baseKey = RegistryKey.OpenBaseKey(rootHive, RegistryView.Default))
                    {
                        // Получаем путь подраздела (удаляем имя корневого раздела из полного пути)
                        string subKeyPath = SelectedNode.Key.Name.Substring(SelectedNode.Key.Name.IndexOf('\\') + 1);

                        // Открываем подраздел с правами на запись
                        using (var key = baseKey.OpenSubKey(subKeyPath, true))
                        {
                            if (key != null)
                            {
                                key.SetValue(dialog.ValueName, dialog.Value, dialog.ValueKind);
                                SelectedNode.LoadValues();
                                MessageBox.Show("Значение успешно создано", "Успех",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Не удалось открыть раздел для записи",
                                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Недостаточно прав для создания значения. Убедитесь, что приложение запущено от имени администратора.",
                                  "Ошибка прав доступа", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка создания значения: {ex.Message}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Вспомогательный метод для определения корневого раздела
        private RegistryHive GetRootHive(string fullPath)
        {
            if (fullPath.StartsWith("HKEY_LOCAL_MACHINE"))
                return RegistryHive.LocalMachine;
            if (fullPath.StartsWith("HKEY_CURRENT_USER"))
                return RegistryHive.CurrentUser;
            if (fullPath.StartsWith("HKEY_CLASSES_ROOT"))
                return RegistryHive.ClassesRoot;
            if (fullPath.StartsWith("HKEY_USERS"))
                return RegistryHive.Users;
            if (fullPath.StartsWith("HKEY_CURRENT_CONFIG"))
                return RegistryHive.CurrentConfig;

            return RegistryHive.CurrentUser; // По умолчанию
        }

        private void EditValue()
        {
            if (SelectedNode?.SelectedValue == null)
                return;

            var dialog = new ValueEditDialog(
                SelectedNode.SelectedValue.Name,
                SelectedNode.SelectedValue.Value,
                SelectedNode.SelectedValue.Kind);

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Получаем корневой раздел
                    var rootHive = GetRootHive(SelectedNode.Key.Name);

                    // Открываем базовый ключ с правами на запись
                    using (var baseKey = RegistryKey.OpenBaseKey(rootHive, RegistryView.Default))
                    {
                        // Получаем путь подраздела
                        string subKeyPath = SelectedNode.Key.Name.Substring(SelectedNode.Key.Name.IndexOf('\\') + 1);

                        // Открываем подраздел с правами на запись
                        using (var key = baseKey.OpenSubKey(subKeyPath, true))
                        {
                            if (key != null)
                            {
                                // Если имя значения изменилось, сначала удаляем старое
                                if (dialog.ValueName != SelectedNode.SelectedValue.Name)
                                {
                                    key.DeleteValue(SelectedNode.SelectedValue.Name, false);
                                }

                                key.SetValue(dialog.ValueName, dialog.Value, dialog.ValueKind);
                                SelectedNode.LoadValues();
                                MessageBox.Show("Значение успешно изменено", "Успех",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Не удалось открыть раздел для записи",
                                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Недостаточно прав для изменения значения. Запустите приложение от имени администратора.",
                                  "Ошибка прав доступа", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка изменения значения: {ex.Message}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanEditValue() => SelectedNode != null && SelectedNode.SelectedValue != null && IsAdmin;
        private bool CanDeleteValue() => SelectedNode != null && SelectedNode.SelectedValue != null && IsAdmin;

        private void DeleteValue()
        {
            if (SelectedNode?.SelectedValue == null)
                return;

            try
            {
                // Request registry write permissions explicitly
                var key = SelectedNode.Key;
                var rk = RegistryKey.OpenBaseKey(key.Name.StartsWith("HKEY_LOCAL_MACHINE") ?
                        RegistryHive.LocalMachine :
                        RegistryHive.CurrentUser,
                        RegistryView.Default);

                using (var writableKey = rk.OpenSubKey(
                    key.Name.Replace("HKEY_LOCAL_MACHINE\\", "").Replace("HKEY_CURRENT_USER\\", ""),
                    RegistryKeyPermissionCheck.ReadWriteSubTree,
                    RegistryRights.FullControl))
                {
                    if (writableKey != null)
                    {
                        writableKey.DeleteValue(SelectedNode.SelectedValue.Name);
                        SelectedNode.LoadValues();
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Access denied. Try taking ownership of the key first.\n{ex.Message}",
                               "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting value: {ex.Message}",
                               "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportRegistry()
        {
            if (SelectedNode == null)
                return;

            var saveDialog = new SaveFileDialog
            {
                Filter = "Файлы реестра (*.reg)|*.reg|Все файлы (*.*)|*.*",
                FileName = $"{SelectedNode.Name}.reg"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    using (var writer = new StreamWriter(saveDialog.FileName))
                    {
                        writer.WriteLine("Windows Registry Editor Version 5.00");
                        writer.WriteLine();
                        ExportNode(writer, SelectedNode);
                    }

                    MessageBox.Show("Экспорт завершен успешно", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportNode(StreamWriter writer, RegistryNode node)
        {
            writer.WriteLine($"[{node.FullPath}]");

            foreach (var value in node.Values)
            {
                var valueStr = value.Kind switch
                {
                    RegistryValueKind.String => $"\"{value.Value}\"",
                    RegistryValueKind.ExpandString => $"\"{value.Value}\"",
                    RegistryValueKind.DWord => $"dword:{Convert.ToInt32(value.Value):x8}",
                    RegistryValueKind.QWord => $"qword:{Convert.ToInt64(value.Value):x16}",
                    RegistryValueKind.MultiString => $"hex(7):{string.Join(",", ((string[])value.Value).Select(s => BitConverter.ToString(System.Text.Encoding.Unicode.GetBytes(s + "\0")).Replace("-", "")))}",
                    _ => $"hex:{BitConverter.ToString((byte[])value.Value).Replace("-", "")}"
                };

                writer.WriteLine($"\"{value.Name}\"={valueStr}");
            }

            writer.WriteLine();

            foreach (var child in node.Children)
            {
                ExportNode(writer, child);
            }
        }
    }
}