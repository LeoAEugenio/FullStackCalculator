using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace FrontEndCalculator
{
    public class ConnectionHttpServer
    {
        public void StartServer()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/");
            listener.Start();
            Console.WriteLine("Server started");

            while (true)
            {
                var context = listener.GetContext();
                var response = context.Response;

                // CORS headers
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

                if (context.Request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Close();
                    continue;
                }

                var path = context.Request.Url?.AbsolutePath ?? "/";
                if (path.Equals("/api/calculate", StringComparison.OrdinalIgnoreCase))
                {
                    // read request body
                    string body;
                    using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                        body = reader.ReadToEnd();

                    var calculation = JsonConvert.DeserializeObject<CalculationRequest>(body);
                    if (calculation != null)
                    {
                        double result = PerformCalculation(calculation);
                        string jsonResponse = JsonConvert.SerializeObject(new { result });
                        byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

                        response.ContentType = "application/json";
                        response.ContentLength64 = buffer.Length;
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        byte[] buffer = Encoding.UTF8.GetBytes("Invalid calculation data");
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                response.Close();
            }
        }

        private static double PerformCalculation(CalculationRequest calc)
        {
            var functions = new Functions();
            switch (calc.Operation.ToLowerInvariant())
            {
                case "add":
                    return functions.Add(calc.Number1, calc.Number2);
                case "subtract":
                    return functions.Subtract(calc.Number1, calc.Number2);
                case "multiply":
                    return functions.Multiply(calc.Number1, calc.Number2);
                case "divide":
                    return functions.Divide(calc.Number1, calc.Number2);
                default:
                    throw new InvalidOperationException("Invalid operation.");
            }
        }
    }
}
