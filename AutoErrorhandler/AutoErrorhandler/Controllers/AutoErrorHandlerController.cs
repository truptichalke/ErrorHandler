using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using AutoErrorhandler.Model;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace AutoErrorhandler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoErrorHandlerController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AutoErrorHandlerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] AutoErrorHandlerRequest errorhandler)
        {
            //if (errorhandler == null || errorhandler.fkProjectid == 0)
            //{
            //    return BadRequest("Invalid Project Id");
            //}
            string errorException = errorhandler.Error[0].Exception;
            string errorCode = errorhandler.Error[0].Code;

            string promptUnderstand =
                "INSTRUCTION: YOU are doing the Error handling of Language " +
                errorhandler.language +
                ". Now this is my Error is : " +
                errorException +
                " and my error code is : " +
                errorCode +
                " Help me to give solution in JSON format with keynames : {'wrong_code':<wrong coode line>,'correct_code':'<correct code>','description': <description of error>} and give only json resonse and no extra text";

            var promptSolution = await CallOllamaAPI(promptUnderstand);
            var jsonStart = promptSolution.IndexOf("{");

            if (jsonStart == -1)
            {
                return BadRequest("AI did not return valid JSON.");
            }

            var json = promptSolution.Substring(jsonStart);

            var fixResult = JsonConvert.DeserializeObject<OllamaFixResponse>(promptSolution);

            if (fixResult != null)
            {
                // Read file content
                string fileContent = await System.IO.File.ReadAllTextAsync(errorhandler.SourceFilePath);

                // Replace wrong code with correct code
                string updatedContent = fileContent.Replace(fixResult.wrong_code, fixResult.correct_code + Environment.NewLine + "//" + fixResult.description);

                // Copy file to destination
                System.IO.File.Copy(errorhandler.SourceFilePath, errorhandler.DestinationFilePath, true);

                //var destinationFile = Path.Combine(errorhandler.DestinationFilePath, errorhandler.Filename);

                //if (!Directory.Exists(errorhandler.DestinationFilePath))
                //{
                //    Directory.CreateDirectory(errorhandler.DestinationFilePath);
                //}
                // Write updated content to copied file
                await System.IO.File.WriteAllTextAsync(errorhandler.DestinationFilePath, updatedContent);

            }

            //string promptSolution =
            //    "INSTRUCTION: YOU are doing the Error handling of Language " +
            //    errorhandler.language +
            //    ". Now this is my Error is : " +
            //    errorhandler.Error +
            //    " Help me to give solution of code so that i can resolved it. give only corrected code.  ";

            //var understandSolution = await CallOllamaAPI(promptSolution);

            string cs = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"INSERT INTO tbl_AutoErrorHandlers
                (fkProjectid,SourceFilePath,DestinationFilePath,Filename,ProjectName,language)
                VALUES
                (@fkProjectid,@SourceFilePath,@DestinationFilePath,@Filename,@ProjectName,@language)
                SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@fkProjectid", errorhandler.fkProjectid);
                cmd.Parameters.AddWithValue("@SourceFilePath", errorhandler.SourceFilePath);
                cmd.Parameters.AddWithValue("@DestinationFilePath", errorhandler.DestinationFilePath);
                cmd.Parameters.AddWithValue("@Filename", errorhandler.Filename);
                cmd.Parameters.AddWithValue("@ProjectName", errorhandler.ProjectName);
                cmd.Parameters.AddWithValue("@language", errorhandler.language);

                await con.OpenAsync();
                int parentId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
               // await cmd.ExecuteNonQueryAsync();
               // await con.CloseAsync();

                foreach (var error in errorhandler.Error)
                {
                    string childQuery = @"INSERT INTO ErrorDetail
                    ( Exception,Wrong_Code,Correct_Code,Error_description,AutoErrorHandlerRequestId)
                    VALUES
                    (@Exception,@Code, @Correct_Code,@Error_description, @AutoErrorHandlerRequestId)";

                    SqlCommand childCmd = new SqlCommand(childQuery, con);

                    promptSolution = promptSolution.Replace("'", "\"");
                    // Convert JSON → Object
                    var solution = JsonConvert.DeserializeObject<ErrorDetail>(promptSolution);

                    childCmd.Parameters.AddWithValue("@Exception", error.Exception);
                    childCmd.Parameters.AddWithValue("@Code", error.Code);
                    childCmd.Parameters.AddWithValue("@Correct_Code", solution.Correct_Code);
                    childCmd.Parameters.AddWithValue("@Error_description", solution.description);
                    childCmd.Parameters.AddWithValue("@AutoErrorHandlerRequestId", parentId); // ⭐ FK

                    await childCmd.ExecuteNonQueryAsync();
                }
                await con.CloseAsync();
            }
            try
            {
                string batFilePath = @"E:\Projects\WEB-API\Development\WEB-API\git_push.bat";

                var process = new Process();
                process.StartInfo.FileName = "cmd.exe";

                // Pass commit message to .bat
                //process.StartInfo.Arguments = $"/c \"{batFilePath}\" \"Resolved error and updated\"";
                process.StartInfo.Arguments = $"/c \"\"{batFilePath}\" \"Resolved error and updated\"\"";

                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                var result = new
                {
                    success = process.ExitCode == 0,
                    exitCode = process.ExitCode,
                    output,
                    error,
                    message = process.ExitCode == 0 ? "Git push successful" : "Git push failed"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            return Ok(promptSolution);
        }

        public async Task<string> CallOllamaAPI(string prompt)
        {
            using (HttpClient client = new HttpClient())
            {
                var request = new
                {
                    model = "llama3.2:3b",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    stream = false
                };

                var json = JsonConvert.SerializeObject(request);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://192.168.1.10:11434/api/chat", content);

                var result = await response.Content.ReadAsStringAsync();

                JObject obj = JObject.Parse(result);

                string aiContent = obj["message"]["content"].ToString();

                return aiContent;
            }
        }
    }
}
