using UnityEngine;

namespace Kit
{
	public static class QuaternionExtensions
	{
		/// <summary>Create a rotation from a Z angle in radians.</summary>
		public static Quaternion RotationFromZ(float radians)
		{
			float half = radians * 0.5f;
			return new Quaternion(0.0f, 0.0f, Mathf.Sin(half), Mathf.Cos(half));
		}

		/// <summary>Returns the X angle in radians.</summary>
		public static float GetXAngle(this Quaternion quaternion)
		{
			float sinRCosP = 2.0f * (quaternion.w * quaternion.x + quaternion.y * quaternion.z);
			float cosRCosP = 1 - 2.0f * (quaternion.x * quaternion.x + quaternion.y * quaternion.y);
			return Mathf.Atan2(sinRCosP, cosRCosP);
		}

		/// <summary>Returns the Y angle in radians.</summary>
		public static float GetYAngle(this Quaternion quaternion)
		{
			float sinP = 2.0f * (quaternion.w * quaternion.y + quaternion.z * quaternion.x);
			if (Mathf.Abs(sinP) >= 1)
				return Mathf.PI / 2.0f * Mathf.Sign(sinP);
			return Mathf.Asin(sinP);
		}

		/// <summary>Returns the Z angle in radians.</summary>
		public static float GetZAngle(this Quaternion quaternion)
		{
			float sinYCosP = 2.0f * (quaternion.w * quaternion.z + quaternion.x * quaternion.y);
			float cosYCosP = 1 - 2.0f * (quaternion.y * quaternion.y + quaternion.z * quaternion.z);
			return Mathf.Atan2(sinYCosP, cosYCosP);
		}
	}
}