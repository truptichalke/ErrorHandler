using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AutoErrorhandler.Model
{
    [Table("ErrorDetail")]
    public class ErrorDetail
    {
        public int Id { get; set; }
        public string Exception { get; set; }
        public string Code { get; set; }
        public string? Correct_Code { get; set; }
        [Column("Error_description")]
        public string? description { get; set; }
        
        public int AutoErrorHandlerRequestId { get; set; }

        [ForeignKey("AutoErrorHandlerRequestId")]
        [JsonIgnore] // ✅ FIX
        [ValidateNever]
        public AutoErrorHandlerRequest? Request { get; set; }
    }
}
