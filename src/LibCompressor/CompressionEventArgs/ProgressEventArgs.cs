using System;

namespace Bau.Libraries.LibCompressor.CompressionEventArgs
{
	/// <summary>
	///		Argumentos del evento de progreso
	/// </summary>
	public class ProgressEventArgs : EventArgs
	{
		public ProgressEventArgs(int actual, int total, string fileName)
		{
			Actual = actual;
			Total = total;
			FileName = fileName;
		}

		/// <summary>
		///		Elemento que se está procesando actualmente
		/// </summary>
		public int Actual { get; }

		/// <summary>
		///		Número total de elementos
		/// </summary>
		public int Total { get; }

		/// <summary>
		///		Nombre de archivo
		/// </summary>
		public string FileName { get; }
	}
}
