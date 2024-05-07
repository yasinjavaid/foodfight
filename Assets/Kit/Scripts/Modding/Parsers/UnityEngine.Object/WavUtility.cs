﻿using System;
using UnityEngine;

namespace Kit.Parsers
{
	/// <summary>
	///     WAV utility for recording and audio playback functions in Unity. Version: 1.0 alpha 1 - Use "ToAudioClip" method for loading wav
	///     file / bytes. Loads .wav (PCM uncompressed) files at 8,16,24 and 32 bits and converts data to Unity's AudioClip. - Use "FromAudioClip"
	///     method for saving wav file / bytes. Converts an AudioClip's float data into wav byte array at 16 bit.
	/// </summary>
	/// <remarks>For documentation and usage examples: https://github.com/deadlyfingers/UnityWav</remarks>
	public static class WavUtility
	{
		public static AudioClip ToAudioClip(byte[] fileBytes, int offsetSamples = 0, string name = "wav")
		{
			//string riff = Encoding.ASCII.GetString (fileBytes, 0, 4);
			//string wave = Encoding.ASCII.GetString (fileBytes, 8, 4);
			int subchunk1 = BitConverter.ToInt32(fileBytes, 16);
			ushort audioFormat = BitConverter.ToUInt16(fileBytes, 20);

			// NB: Only uncompressed PCM wav files are supported.
			string formatCode = FormatCode(audioFormat);
			if (!(audioFormat == 1 || audioFormat == 65534))
				throw new
					FormatException($"Detected format code '{audioFormat}' {formatCode}, but only PCM and WaveFormatExtensible uncompressed formats are currently supported.");

			ushort channels = BitConverter.ToUInt16(fileBytes, 22);
			int sampleRate = BitConverter.ToInt32(fileBytes, 24);
			//int byteRate = BitConverter.ToInt32 (fileBytes, 28);
			//UInt16 blockAlign = BitConverter.ToUInt16 (fileBytes, 32);
			ushort bitDepth = BitConverter.ToUInt16(fileBytes, 34);

			int headerOffset = 16 + 4 + subchunk1 + 4;
			int subchunk2 = BitConverter.ToInt32(fileBytes, headerOffset);

			float[] data;
			switch (bitDepth)
			{
				case 8:
					data = Convert8BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);
					break;
				case 16:
					data = Convert16BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);
					break;
				case 24:
					data = Convert24BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);
					break;
				case 32:
					data = Convert32BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);
					break;
				default:
					throw new FormatException(bitDepth + " bit depth is not supported.");
			}

			AudioClip audioClip = AudioClip.Create(name, data.Length, channels, sampleRate, false);
			audioClip.SetData(data, 0);
			return audioClip;
		}

		#region wav file bytes to Unity AudioClip conversion methods

		private static float[] Convert8BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
		{
			int wavSize = BitConverter.ToInt32(source, headerOffset);
			headerOffset += sizeof(int);
			if (!(wavSize > 0 && wavSize == dataSize))
				throw new
					FormatException($"Failed to get valid 8-bit wav size: {wavSize} from data bytes: {dataSize} at offset: {headerOffset}");

			float[] data = new float[wavSize];

			sbyte maxValue = sbyte.MaxValue;

			int i = 0;
			while (i < wavSize)
			{
				data[i] = (float) source[i] / maxValue;
				++i;
			}

			return data;
		}

		private static float[] Convert16BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
		{
			int wavSize = BitConverter.ToInt32(source, headerOffset);
			headerOffset += sizeof(int);
			if (!(wavSize > 0 && wavSize == dataSize))
				throw new FormatException(string.Format("Failed to get valid 16-bit wav size: {0} from data bytes: {1} at offset: {2}",
														wavSize,
														dataSize,
														headerOffset));

			int x = sizeof(short); // block size = 2
			int convertedSize = wavSize / x;

			float[] data = new float[convertedSize];

			short maxValue = short.MaxValue;

			int i = 0;
			while (i < convertedSize)
			{
				int offset = i * x + headerOffset;
				data[i] = (float) BitConverter.ToInt16(source, offset) / maxValue;
				++i;
			}

			if (data.Length != convertedSize)
				throw new FormatException($"AudioClip .wav data is wrong size: {data.Length} == {convertedSize}");

			return data;
		}

		private static float[] Convert24BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
		{
			int wavSize = BitConverter.ToInt32(source, headerOffset);
			headerOffset += sizeof(int);
			if (!(wavSize > 0 && wavSize == dataSize))
				throw new
					FormatException($"Failed to get valid 24-bit wav size: {wavSize} from data bytes: {dataSize} at offset: {headerOffset}");

			int x = 3; // block size = 3
			int convertedSize = wavSize / x;

			int maxValue = int.MaxValue;

			float[] data = new float[convertedSize];

			byte[] block = new byte[sizeof(int)]; // using a 4 byte block for copying 3 bytes, then copy bytes with 1 offset

			int i = 0;
			while (i < convertedSize)
			{
				int offset = i * x + headerOffset;
				Buffer.BlockCopy(source, offset, block, 1, x);
				data[i] = (float) BitConverter.ToInt32(block, 0) / maxValue;
				++i;
			}

			if (data.Length != convertedSize)
				throw new FormatException($"AudioClip .wav data is wrong size: {data.Length} == {convertedSize}");

			return data;
		}

		private static float[] Convert32BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
		{
			int wavSize = BitConverter.ToInt32(source, headerOffset);
			headerOffset += sizeof(int);
			if (!(wavSize > 0 && wavSize == dataSize))
				throw new
					FormatException($"Failed to get valid 32-bit wav size: {wavSize} from data bytes: {dataSize} at offset: {headerOffset}");

			int x = sizeof(float); //  block size = 4
			int convertedSize = wavSize / x;

			int maxValue = int.MaxValue;

			float[] data = new float[convertedSize];

			int i = 0;
			while (i < convertedSize)
			{
				int offset = i * x + headerOffset;
				data[i] = (float) BitConverter.ToInt32(source, offset) / maxValue;
				++i;
			}

			if (data.Length != convertedSize)
				throw new FormatException($"AudioClip .wav data is wrong size: {data.Length} == {convertedSize}");

			return data;
		}

		#endregion

		private static string FormatCode(ushort code)
		{
			switch (code)
			{
				case 1:
					return "PCM";
				case 2:
					return "ADPCM";
				case 3:
					return "IEEE";
				case 7:
					return "μ-law";
				case 65534:
					return "WaveFormatExtensible";
				default:
					return "Unknown";
			}
		}
	}
}