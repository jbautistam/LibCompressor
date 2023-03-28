using System;
using System.Collections.Generic;
using System.IO;

using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace Bau.Libraries.LibCompressor
{
	/// <summary>
	///		Compresor de archivos
	/// </summary>
	public class Compressor
	{
		// Eventos públicos
		public event EventHandler<CompressionEventArgs.ProgressEventArgs> Progress;
		public event EventHandler<CompressionEventArgs.EndEventArgs> End;

		/// <summary>
		///		Tipo de compresión
		/// </summary>
		public enum CompressType
		{
			/// <summary>Desconocido</summary>
			Unknown,
			/// <summary>Archivo ZIP</summary>
			Zip,
			/// <summary>Archivo RAR</summary>
			Rar,
			/// <summary>Archivo GZIP</summary>
			GZip,
			/// <summary>Archivo Tar</summary>
			Tar
		}

		/// <summary>
		///		Comprime un directorio
		/// </summary>
		public void Compress(string fileTarget, string path, CompressType type = CompressType.Zip)
		{
			switch (type)
			{
				case CompressType.Zip:
						CompressPathToZip(fileTarget, path);
					break;
				default:
						Compress(fileTarget, GetFiles(path));
					break;
			}
		}

		/// <summary>
		///		Comprime un archivo
		/// </summary>
		public void Compress(string fileTarget, List<string> files)
		{
			using (FileStream zipStream = File.OpenWrite(fileTarget))
			{
				using (IWriter zipWriter = WriterFactory.Open(zipStream, ArchiveType.GZip, new WriterOptions(CompressionType.GZip)))
				{
					foreach (string fileName in files)
						zipWriter.Write(Path.GetFileName(fileName), fileName);
				}
			}
		}

		/// <summary>
		///		Comprime un directorio a un archivo zip
		/// </summary>
		private void CompressPathToZip(string fileTarget, string path)
		{
			using (ZipArchive archive = ZipArchive.Create())
			{
				// Añade los archivos
				archive.AddAllFromDirectory(path);
				// y lo almacena comprimido
				archive.SaveTo(fileTarget, CompressionType.Deflate);
			}
		}

		/// <summary>
		///		Añade un directorio al Zip
		/// </summary>
		private List<string> GetFiles(string pathBase)
		{
			List<string> files = new List<string>();

				// Añade los archivos y directorios hijo
				if (Directory.Exists(pathBase))
				{
					foreach (string path in Directory.GetDirectories(pathBase))
						files.AddRange(GetFiles(path));
					foreach (string file in Directory.GetFiles(pathBase))
						files.Add(file);
				}
				else if (File.Exists(pathBase))
					files.Add(pathBase);
				// Devuelve la colección de archivos
				return files;
		}

		/// <summary>
		///		Descomprime un archivo en un directorio
		/// </summary>
		public void Uncompress(string fileSource, string pathTarget)
		{
			IArchive archive = ArchiveFactory.Open(fileSource);
			int file = 0;

				// Descomprime los archivos
				foreach (IArchiveEntry entry in archive.Entries)
					if (!entry.IsDirectory)
					{
						string fileTarget;

							// Descomprime un archivo
							UncompressFile(entry, pathTarget, out fileTarget);
							// Lanza el evento
							RaiseEventProgess(++file, file, fileTarget);
					}
				// Lanza el evento de fin
				RaiseEventEnd();
		}

		/// <summary>
		///		Descomprime un archivo
		/// </summary>
		private void UncompressFile(IArchiveEntry entry, string path, out string fileTarget)
		{
			// Obtiene el nombre del archivo de salida
			fileTarget = Path.Combine(path, NormalizeFileName(entry.Key));
			// Crea el directorio
			MakePath(Path.GetDirectoryName(fileTarget));
			// Borra el archivo de salida (por si acaso)
			KillFile(fileTarget);
			// Descomprime el archivo
			entry.WriteToFile(fileTarget);
		}

		/// <summary>
		///		Carga la lista de archivos de un archivo comprimido
		/// </summary>
		public List<string> ListFiles(string fileName)
		{
			IArchive archive = ArchiveFactory.Open(fileName);
			List<string> files = new List<string>();
			int fileIndex = 0;

				// Lista los archivos
				foreach (IArchiveEntry entry in archive.Entries)
					if (!entry.IsDirectory)
					{
						// Añade un archivo
						files.Add(NormalizeFileName(entry.Key));
						// Lanza el evento
						RaiseEventProgess(fileIndex++, fileIndex + 2, files[files.Count - 1]);
					}
				// Devuelve la colección de archivos
				return files;
		}

		/// <summary>
		///		Lanza el evento de progreso
		/// </summary>
		private void RaiseEventProgess(int actual, int total, string fileName)
		{
			Progress?.Invoke(this, new CompressionEventArgs.ProgressEventArgs(actual, total, fileName));
		}

		/// <summary>
		///		Lanza el evento de fin
		/// </summary>
		private void RaiseEventEnd()
		{
			End?.Invoke(this, new CompressionEventArgs.EndEventArgs());
		}

		/// <summary>
		///		Crea un directorio
		/// </summary>
		private bool MakePath(string path)
		{
			try
			{
				Directory.CreateDirectory(path);
				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				return false;
			}
		}

		/// <summary>
		///		Borra un archivo
		/// </summary>
		private void KillFile(string fileName)
		{
			try
			{
				File.Delete(fileName);
			}
			catch { }
		}

		/// <summary>
		///		Normaliza un nombre de archivo
		/// </summary>
		protected string NormalizeFileName(string fileName)
		{
			// Reemplaza los caracteres extraños
			fileName = fileName.Replace('/', '\\');
			// Devuelve el nombre de archivo
			return fileName;
		}
	}
}
