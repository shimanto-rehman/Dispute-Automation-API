namespace RemitAnnex.Models.Validation
{
    public class CommonValidationResponse
    {
        public bool IsValid { get; set; } = false;       
        public string Msg { get; set; } = string.Empty;       
    }
}
