using System.Collections.Generic;
using System.IO;
using System;

namespace lab2.Models;

public class Encrypt
{
    //списки одной длины 
    // сделать сохранение только первых и последних 64 бит// 8 + 8 байт
    public List<byte> Input { get; set; } 
    public List<byte> Result { get; set; }
    public List<byte> Key { get; set; }
    private readonly int BlockSize = 4096;
    public Encrypt()
    {
        Input = new List<byte>();
        Result = new List<byte>();
        Key = new List<byte>();
    }
    public void Convert(string inf, string outf, uint seed)
    {
        byte[] buffer = new byte[BlockSize];
        int bytesRead;
        LSFR lsfr = new LSFR(seed);
        
        string inputExtension = Path.GetExtension(inf);
        string outputPath = outf;
        if (string.IsNullOrEmpty(Path.GetExtension(outf)))
        {
            // Если расширения нет, добавляем расширение входного файла
            outputPath = outf + inputExtension;
        }
        else if (!Path.GetExtension(outf).Equals(inputExtension, StringComparison.OrdinalIgnoreCase))
        {
            // Если расширение отличается, заменяем его на расширение входного файла
            outputPath = Path.ChangeExtension(outf, inputExtension);
        }
        
        bool sameFile = string.Equals(Path.GetFullPath(inf), Path.GetFullPath(outputPath), StringComparison.OrdinalIgnoreCase);
        string tempOutf = outputPath;
        
        if (sameFile)
        {
            string directory = Path.GetDirectoryName(outputPath);
            string fileName = Path.GetFileName(outputPath);
            tempOutf = Path.Combine(directory, $".{fileName}");
        }
        
        using (FileStream inputFile = new FileStream(inf, FileMode.Open, FileAccess.Read))
        using (FileStream outputFile = new FileStream(tempOutf, FileMode.Create, FileAccess.Write))
        {
            while ((bytesRead = inputFile.Read(buffer, 0, BlockSize)) > 0)
            {
                byte[] encryptedBuffer = new byte[bytesRead];
                for (int i = 0; i < bytesRead; i++)
                {
                    byte keyByte = lsfr.ShiftLeftByte();
                    encryptedBuffer[i] = (byte)(buffer[i] ^ keyByte);

                    if (Input.Count < 32)
                    {
                        Input.Add(buffer[i]);
                        Result.Add(encryptedBuffer[i]);
                        Key.Add(keyByte);
                    }
                    else
                    {
                        Input.RemoveAt(16);
                        Input.Add(buffer[i]);
                        Result.RemoveAt(16);
                        Result.Add(encryptedBuffer[i]);
                        Key.RemoveAt(16);
                        Key.Add(keyByte);
                    }
                }
                outputFile.Write(encryptedBuffer, 0, bytesRead);
            }
        }
        if (sameFile)
        {
            File.Move(tempOutf, outputPath, overwrite: true);
            // Удаляем временный файл (на случай, если File.Move не перезаписал его полностью)
            if (File.Exists(tempOutf))
            {
                File.Delete(tempOutf);
            }
        }
    }
}