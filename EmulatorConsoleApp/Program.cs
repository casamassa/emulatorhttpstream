using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;

public class HttpStreamEmulator
{
    private static ConcurrentQueue<string> payloadQueue = new ConcurrentQueue<string>();

    public static async Task RunEmulator(string url, string username, string password, string sendDataUrl)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Prefixes.Add(sendDataUrl);
        listener.Start();

        Console.WriteLine($"Emulador HTTP Stream iniciado em {url} e {sendDataUrl}");

        try
        {
            // Thread para lidar com o stream HTTP
            Task.Run(async () =>
            {
                Console.WriteLine("Thread Stream HTTP iniciada.");
                while (true)
                {
                    try
                    {
                        HttpListenerContext context = await listener.GetContextAsync();
                        if (context.Request.Url.AbsolutePath == "/")
                        {
                            await HandleStream(context, username, password);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro em RunEmulator - Stream HTTP: {ex.Message}");
                    }
                }
            });

            // Thread para lidar com o endpoint /sendData
            Task.Run(async () =>
            {
                Console.WriteLine("Thread SendData iniciada.");
                while (true)
                {
                    try
                    {
                        HttpListenerContext context = await listener.GetContextAsync();
                        if (context.Request.Url.AbsolutePath == "/sendData/")
                        {
                            await HandleSendData(context, username, password);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro em RunEmulator - SendData: {ex.Message}");
                    }
                }
            });

            // Manter a thread principal ativa
            while (true)
            {
                await Task.Delay(100);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro em RunEmulator - Thread principal: {ex.Message}");
        }

    }

    private static async Task HandleSendData(HttpListenerContext context, string username, string password)
    {
        Console.WriteLine("HandleSendData iniciado.");
        try
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if (!Authenticate(request, username, password))
            {
                Console.WriteLine("Autenticação falhou.");
                response.StatusCode = 401;
                response.Close();
                return;
            }
            Console.WriteLine("Autenticação realizada.");

            if (request.HttpMethod == "POST")
            {
                Console.WriteLine("Método POST recebido.");
                using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string payload = await reader.ReadToEndAsync();
                    payloadQueue.Enqueue(payload);
                    Console.WriteLine($"Payload recebido: {payload}");
                }
                response.StatusCode = 200;
                response.Close();
                Console.WriteLine("Resposta 200 enviada.");
            }
            else
            {
                Console.WriteLine("Método não permitido.");
                response.StatusCode = 405;
                response.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro em HandleSendData: {ex.Message}");
        }
        Console.WriteLine("HandleSendData finalizado.");
    }

    private static async Task HandleStream(HttpListenerContext context, string username, string password)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        if (!Authenticate(request, username, password))
        {
            response.StatusCode = 401;
            response.Close();
            return;
        }

        response.ContentType = "application/json";
        response.SendChunked = true;

        while (true)
        {
            if (payloadQueue.TryDequeue(out string payload))
            {
                // Remover \r e \n do payload
                payload = payload.Replace("\r", "").Replace("\n", "");
                byte[] buffer = Encoding.UTF8.GetBytes(payload + "\n");
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                await response.OutputStream.FlushAsync();
                Console.WriteLine($"Payload enviado: {payload}");
            }
            await Task.Delay(100);
        }
    }

    private static bool Authenticate(HttpListenerRequest request, string username, string password)
    {
        string authHeader = request.Headers["Authorization"];
        if (authHeader == null || !authHeader.StartsWith("Basic "))
        {
            Console.WriteLine("Autenticação: Cabeçalho inválido.");
            return false;
        }

        string credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Substring(6)));
        string[] parts = credentials.Split(':');
        bool authResult = parts.Length == 2 && parts[0] == username && parts[1] == password;
        Console.WriteLine($"Autenticação: {authResult}");
        return authResult;
    }

    public static async Task Main(string[] args)
    {
        string url = "http://localhost:5099/";
        string sendDataUrl = "http://localhost:5099/sendData/";
        string username = "root";
        string password = "impinj";

        await RunEmulator(url, username, password, sendDataUrl);
    }
}