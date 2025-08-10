using System;

namespace manyasligida.Services
{
    /// <summary>
    /// Türkiye saat dilimi (UTC+3) için DateTime helper sınıfı
    /// Sunucu Avrupa'da olduğu için tüm DateTime işlemlerini Türkiye saati ile standardize eder
    /// </summary>
    public static class DateTimeHelper
    {
        // Türkiye saat dilimi UTC+3
        private static readonly TimeZoneInfo TurkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        
        /// <summary>
        /// Şu anki Türkiye saati
        /// </summary>
        public static DateTime NowTurkey => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyTimeZone);
        
        /// <summary>
        /// Şu anki Türkiye saati (sadece tarih)
        /// </summary>
        public static DateTime TodayTurkey => NowTurkey.Date;
        
        /// <summary>
        /// UTC zamanı Türkiye saatine çevir
        /// </summary>
        /// <param name="utcDateTime">UTC zaman</param>
        /// <returns>Türkiye saati</returns>
        public static DateTime ConvertFromUtc(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TurkeyTimeZone);
        }
        
        /// <summary>
        /// Türkiye saatini UTC'ye çevir
        /// </summary>
        /// <param name="turkeyDateTime">Türkiye saati</param>
        /// <returns>UTC zaman</returns>
        public static DateTime ConvertToUtc(DateTime turkeyDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(turkeyDateTime, TurkeyTimeZone);
        }
        
        /// <summary>
        /// Türkiye saatinde string formatı
        /// </summary>
        /// <param name="format">Format string (varsayılan: yyyy-MM-dd HH:mm:ss)</param>
        /// <returns>Formatlanmış Türkiye saati</returns>
        public static string NowTurkeyString(string format = "yyyy-MM-dd HH:mm:ss")
        {
            return NowTurkey.ToString(format);
        }
        
        /// <summary>
        /// Email verification için Türkiye saatinde expire time
        /// </summary>
        /// <param name="minutesToAdd">Eklenecek dakika (varsayılan: 15)</param>
        /// <returns>Türkiye saatinde expire time</returns>
        public static DateTime GetEmailVerificationExpiry(int minutesToAdd = 15)
        {
            return NowTurkey.AddMinutes(minutesToAdd);
        }
        
        /// <summary>
        /// Session için Türkiye saatinde expire time
        /// </summary>
        /// <param name="hoursToAdd">Eklenecek saat (varsayılan: 2)</param>
        /// <returns>Türkiye saatinde expire time</returns>
        public static DateTime GetSessionExpiry(int hoursToAdd = 2)
        {
            return NowTurkey.AddHours(hoursToAdd);
        }
        
        /// <summary>
        /// Database'e kaydetmek için UTC'ye çevrilmiş Türkiye saati
        /// </summary>
        public static DateTime NowForDatabase => ConvertToUtc(NowTurkey);
        
        /// <summary>
        /// Bir DateTime'ın Türkiye saatinde olup olmadığını kontrol et
        /// </summary>
        /// <param name="dateTime">Kontrol edilecek tarih</param>
        /// <returns>True if Turkey time, false if UTC</returns>
        public static bool IsTurkeyTime(DateTime dateTime)
        {
            // Basit bir kontrol: UTC'den çevirdiğimizde aynı değeri veriyorsa Turkey time'dır
            var converted = ConvertFromUtc(ConvertToUtc(dateTime));
            return Math.Abs((converted - dateTime).TotalMinutes) < 1;
        }
    }
}
