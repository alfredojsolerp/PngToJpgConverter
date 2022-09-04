// Command to get .exe:
// dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true

// Usings
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

// Constants
const string JPG_EXTENSION = ".jpg";
const string PNG_EXTENSION = ".png";
const string NEW_DIRECTORY_NAME = "Converted images";
const int ONE_MEGABYTE = 1000000;

// Fields
int pngImagesCount = 0;
int jpgImagesCount = 0;
double pngImagesTotalSize = 0;
double jpgImagesTotalSize = 0;

// Methods
ImageCodecInfo GetEncoder(ImageFormat format)
{
	ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

	foreach (ImageCodecInfo codec in codecs)
	{
		if (codec.FormatID == format.Guid)
		{
			return codec;
		}
	}

	return null;
}

void WriteTotalImagesConverted()
{
	Console.Write("\r" + new string(' ', Console.BufferWidth - 1) + "\r");
	Console.Write("> Total images converted: {0}/{1}.", ++jpgImagesCount, pngImagesCount);
}

// Program start
if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
	Console.WriteLine("> This app only runs on Windows.");
	Console.WriteLine("> Press any key to exit.");
	Console.ReadKey();
	return -1;
}

Console.WriteLine("> Getting directory info.");
DirectoryInfo directoryInfo = new DirectoryInfo(".");
FileInfo[] pngsToConvert = directoryInfo.GetFiles($"*{PNG_EXTENSION}");
pngImagesCount = pngsToConvert?.Count() ?? 0;

// Check if there are images to convert
if (pngImagesCount > 0)
{
	Console.WriteLine("> {0} PNG images found. Press any key to start with the conversion.", pngImagesCount);
	Console.ReadKey();
}
// If not, exit the program
else
{
	Console.WriteLine("> No PNG images found in this directory.");
	Console.WriteLine("> Press any key to exit.");
	Console.ReadKey();
	return -1;
}

// Main loop
Console.WriteLine("> Converting...");

foreach (FileInfo file in pngsToConvert)
{
	try
	{
		// Get the PNG stream and save it to an image
		var pngStream = file.OpenRead();
		pngImagesTotalSize += pngStream.Length;
		var pngImage = Image.FromStream(pngStream);
		pngStream.Dispose();

		// Save the PNG image into a JPG stream
		var jpgStream = new MemoryStream();
		var encoderParameter = new EncoderParameter(Encoder.Quality, 100L);
		var encoderParameters = new EncoderParameters(1);
		encoderParameters.Param[0] = encoderParameter;
		pngImage.Save(jpgStream, GetEncoder(ImageFormat.Jpeg), encoderParameters);
		pngImage.Dispose();

		// If directory does not exist, create it
		string folder = string.Format(@"{0}\{1}", Directory.GetCurrentDirectory(), NEW_DIRECTORY_NAME);

		if (!Directory.Exists(folder))
		{
			Directory.CreateDirectory(folder);
		}

		// Copy the JPG stream into a file stream
		jpgImagesTotalSize += jpgStream.Length;

		var fileStream = new FileStream(
			string.Format(@"{0}\{1}" + JPG_EXTENSION, folder, file.Name.Replace(PNG_EXTENSION, string.Empty)),
			FileMode.OpenOrCreate,
			FileAccess.ReadWrite);
		
		jpgStream.Seek(0, SeekOrigin.Begin);
		jpgStream.CopyTo(fileStream);
		fileStream.Dispose();
		jpgStream.Dispose();		
		WriteTotalImagesConverted();		
		File.Delete(file.FullName);
	}
	catch (Exception ex)
	{
		Console.WriteLine("> Error converting image: {0}", file.Name);
	}
}

// Program end
Console.WriteLine("\n> Convertion ended.");
Console.WriteLine("> Total size before convertion: {0:0.##}Mb.", pngImagesTotalSize / ONE_MEGABYTE);
Console.WriteLine("> Total size after convertion: {0:0.##}Mb.", jpgImagesTotalSize / ONE_MEGABYTE);
Console.WriteLine("> Press any key to exit.");
Console.ReadKey();
return 0;