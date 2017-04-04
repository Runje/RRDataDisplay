using System;
using System.Diagnostics;
using R3E.Data;

namespace R3E
{
    public class Utilities
    {
        public static Single RpsToRpm(Single rps)
        {
            return rps * (60 / (2 * (Single)Math.PI));
        }

        public static Single MpsToKph(Single mps)
        {
            return mps * 3.6f;
        }

        public static bool IsRrreRunning()
        {
            return Process.GetProcessesByName("RRRE").Length > 0;
        }

        public static string byteToString(byte[] bytes)
        {
            int i = Array.FindIndex(bytes, (x) => x == 0);
            return System.Text.Encoding.UTF8.GetString(bytes, 0, i);
        }

        public static string floatToString(float brakeBias)
        {
            return brakeBias.ToString("0.00");
        }

        public static string brakeTempToString(TireData brakeTemp)
        {
            return string.Format("FL: {0} FR: {1} RL: {2} RR: {3}", floatToString(brakeTemp.FrontLeft), floatToString(brakeTemp.FrontRight), floatToString(brakeTemp.RearLeft), floatToString(brakeTemp.RearRight));
        }
    }
}