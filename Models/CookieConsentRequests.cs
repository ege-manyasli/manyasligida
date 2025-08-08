namespace manyasligida.Models
{
    public class CookieConsentRequest
    {
        public bool IsAccepted { get; set; }
        public Dictionary<int, bool> CategoryPreferences { get; set; } = new Dictionary<int, bool>();
    }

    public class CookieCategoryUpdateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }
}
