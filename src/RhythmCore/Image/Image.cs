using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhythm.Image
{
    public class Image
    {
        private Image(){}

        /// <summary>
        /// Converts an input image to a base64 (string) representation.
        /// </summary>
        /// <param name="imagePath">The image to convert.</param>
        /// <returns name"base64String"></returns>
        public static string ConvertToBase64(string imagePath)
        {
            byte[] imageArray = File.ReadAllBytes(imagePath);
            string base64ImageRepresentation = Convert.ToBase64String(imageArray);

            return base64ImageRepresentation;
        }
    }
}
