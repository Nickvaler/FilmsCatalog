using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace FilmsCatalog.Helpers
{
    public static class FormFileExtensions
    {
        public static bool ValidateImageSize(this IFormFile file, int maxSize)
        {
            if (!file.OpenReadStream().CanRead)
            {
                return false;
            }
            if (file.Length > maxSize)
            {
                return false;
            }
            return true;
        }
        public static bool ValidateImageExtension(this IFormFile file)
        {
            if (!string.Equals(file.ContentType, "image/jpg", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(file.ContentType, "image/jpeg", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(file.ContentType, "image/pjpeg", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(file.ContentType, "image/gif", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(file.ContentType, "image/x-png", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(file.ContentType, "image/png", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var postedFileExtension = Path.GetExtension(file.FileName);
            if (!string.Equals(postedFileExtension, ".jpg", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".png", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".gif", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
        public static bool ValidatePictureData(this IFormFile file)
        {
            try
            {
                using (var bitmap = new Bitmap(file.OpenReadStream()))
                {
                }
            }
            catch (Exception)
            {
                return true;
            }
            finally
            {
                file.OpenReadStream().Position = 0;
            }
            return false;
        }

        public static byte[] GetImageByteArr(this IFormFile file)
        {
            byte[] imageData = null;
            if (file != null)
            {
                using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)file.Length);
                }
            }
            return imageData;
        }

    }
}
