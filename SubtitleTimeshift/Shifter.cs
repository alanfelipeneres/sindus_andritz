using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SubtitleTimeshift
{
    public class Shifter
    {
        async static public Task Shift(Stream input, Stream output, TimeSpan timeSpan, Encoding encoding, int bufferSize = 1024, bool leaveOpen = false)
        {
            using (var reader = new StreamReader(input, encoding, true, bufferSize, leaveOpen))
            using (var writer = new StreamWriter(output, encoding, bufferSize, leaveOpen))
            {
                string line;
                //Lendo linha a linha do arquivo
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    // Verificando se a linha que está sendo lida refere-se ao tempo da legenda
                    if (Regex.IsMatch(line, @"\d{2}:\d{2}:\d{2},\d{3} --> \d{2}:\d{2}:\d{2},\d{3}"))
                    {
                        // Dividindo o 'tempo inicial' e o 'tempo final' que estão separados por '-->'
                        var parts = line.Split(new[] { " --> " }, StringSplitOptions.None);
                        var startTime = TimeSpan.Parse(parts[0].Replace(',', '.'));
                        var endTime = TimeSpan.Parse(parts[1].Replace(',', '.'));

                        // Adicionando o tempo adicional (timeSpan) ao 'tempo inicial' e ao 'tempo final'
                        startTime += timeSpan;
                        endTime += timeSpan;

                        // Escrevendo o novos 'tempo inicial' e o 'tempo final' no formato do arquivo de legenda
                        var shiftedLine = $"{startTime.ToString(@"hh\:mm\:ss\.fff")} --> {endTime.ToString(@"hh\:mm\:ss\.fff")}";

                        // Escrevendo a linha com os novos tempos formatados no arquivo final
                        await writer.WriteLineAsync(shiftedLine);
                    }
                    else
                    {
                        // Caso não seja um linha refente ao tempo de legenda, escreva-a no arquivo
                        await writer.WriteLineAsync(line);
                    }
                }
            }
        }
    }
}
