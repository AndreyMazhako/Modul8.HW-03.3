using System;
using System.IO;

public class CleanAndMeasure
{
    public static long CalculateFolderSize(string directoryPath)
    {
        long totalSize = 0;

        // Проверяем, существует ли папка
        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine($"Папка '{directoryPath}' не существует.");
            return 0;
        }

        // Проверяем права доступа к папке
        try
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
            if (!dirInfo.Exists)
            {
                Console.WriteLine($"Папка '{directoryPath}' не существует.");
                return 0;
            }

            if (dirInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                Console.WriteLine($"Нет прав доступа к '{directoryPath}' для чтения.");
                return 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка доступа к папке '{directoryPath}': {ex.Message}");
            return 0;
        }

        // Получаем список файлов в текущей папке
        try
        {
            string[] files = Directory.GetFiles(directoryPath);

            // Считаем размер всех файлов
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                totalSize += fileInfo.Length;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении списка файлов в '{directoryPath}': {ex.Message}");
        }

        // Рекурсивно обрабатываем подпапки, 
        // игнорируя папку $Recycle.Bin и другие папки, 
        // к которым нет доступа
        try
        {
            string[] subdirectories = Directory.GetDirectories(directoryPath);
            foreach (string subdirectory in subdirectories)
            {
                // Проверяем, не $Recycle.Bin ли это
                if (subdirectory.StartsWith("$Recycle.Bin"))
                {
                    continue; // Пропускаем эту папку
                }

                totalSize += CalculateFolderSize(subdirectory);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении списка подпапок в '{directoryPath}': {ex.Message}");
        }

        return totalSize;
    }

    public static void CleanFolderRecursive(string folderPath, TimeSpan threshold)
    {
        // Получаем список файлов и папок в текущей папке
        string[] files = Directory.GetFiles(folderPath);
        string[] directories = Directory.GetDirectories(folderPath);

        // Проверяем файлы
        foreach (string file in files)
        {
            // Проверяем, был ли файл изменен более threshold
            FileInfo fileInfo = new FileInfo(file);
            if (DateTime.Now - fileInfo.LastWriteTime > threshold)
            {
                // Удаляем файл
                File.Delete(file);
                Console.WriteLine($"Удален файл: {file}");
            }
        }

        // Проверяем папки
        foreach (string directory in directories)
        {
            // Проверяем, была ли папка изменена более threshold
            DirectoryInfo dirInfo = new DirectoryInfo(directory);
            if (DateTime.Now - dirInfo.LastWriteTime > threshold)
            {
                // Удаляем папку
                Directory.Delete(directory, true); // true - recursively
                Console.WriteLine($"Удалена папка: {directory}");
            }
            else
            {
                // Рекурсивно очищаем подпапки
                CleanFolderRecursive(directory, threshold);
            }
        }
    }

    public static void Main(string[] args)
    {
        // Устанавливаем путь к папке на диске C
        string directoryPath = "C:\\"; // Измените путь, если нужно

        // 1. Выводим размер папки до очистки
        Console.WriteLine("Размер папки до очистки:");
        long sizeBefore = CalculateFolderSize(directoryPath);
        Console.WriteLine($"  {sizeBefore} байт");

        // 2. Очищаем папку
        Console.WriteLine("Очистка папки:");
        int deletedFilesCount = 0;
        CleanFolderRecursive(directoryPath, TimeSpan.FromMinutes(30));

        // 3. Выводим информацию об удаленных файлах
        Console.WriteLine("Удалено файлов: {0}", deletedFilesCount);

        // 4. Вычисляем размер папки после очистки
        long sizeAfter = CalculateFolderSize(directoryPath);
        Console.WriteLine("Размер папки после очистки:");
        Console.WriteLine($"  {sizeAfter} байт");

        // 5. Выводим информацию об освобожденном пространстве
        Console.WriteLine("Освобождено места: {0} байт", sizeBefore - sizeAfter);
    }
}