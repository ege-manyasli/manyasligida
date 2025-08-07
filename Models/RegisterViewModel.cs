using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad gereklidir")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olmalıdır")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Soyad gereklidir")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olmalıdır")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olmalıdır")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Telefon gereklidir")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olmalıdır")]
        [Display(Name = "Telefon")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Şifre gereklidir")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6, en fazla 100 karakter olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Şifre onayı gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Onayı")]
        [Compare("Password", ErrorMessage = "Şifre ve şifre onayı uyuşmuyor")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "Adres en fazla 200 karakter olmalıdır")]
        [Display(Name = "Adres")]
        public string? Address { get; set; }
        
        [StringLength(50, ErrorMessage = "Şehir en fazla 50 karakter olmalıdır")]
        [Display(Name = "Şehir")]
        public string? City { get; set; }
        
        [StringLength(10, ErrorMessage = "Posta kodu en fazla 10 karakter olmalıdır")]
        [Display(Name = "Posta Kodu")]
        public string? PostalCode { get; set; }
        
        [Required(ErrorMessage = "Kullanım şartlarını kabul etmelisiniz")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Kullanım şartlarını kabul etmelisiniz")]
        [Display(Name = "Kullanım şartlarını kabul ediyorum")]
        public bool AcceptTerms { get; set; } = false;
    }
} 