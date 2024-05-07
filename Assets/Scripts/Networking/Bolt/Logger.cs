using Kit;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Game.Networking.Bolt
{
	public class Logger : BoltLog.IWriter
	{
		public void Debug(string message)
		{
			Debugger.Log(Networking.NetworkManager.Instance.LogCategory, message, LogType.Log);
		}

		public void Error(string message)
		{
			Debugger.Log(Networking.NetworkManager.Instance.LogCategory, message, LogType.Error);
		}

		public void Info(string message)
		{
			Debugger.Log(Networking.NetworkManager.Instance.LogCategory, message, LogType.Assert);
		}

		public void Warn(string message)
		{
			Debugger.Log(Networking.NetworkManager.Instance.LogCategory, message, LogType.Warning);
		}

		public void Dispose()
		{

		}
	}
}