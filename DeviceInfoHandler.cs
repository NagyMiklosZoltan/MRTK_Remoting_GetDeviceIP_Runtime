using System;
using System.Linq;
using System.Reflection;
using UnityEngine.XR.OpenXR;

namespace Assets
{
    internal class DeviceInfoHandler
    {
        public static string GetIpOfDevice()
        {
            Assembly asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.Contains("Microsoft.MixedReality.OpenXR,"));

            if (!ReferenceEquals(asm, null))
            {
                Type PlayModeRemotingPluginType = asm.GetType("Microsoft.MixedReality.OpenXR.Remoting.PlayModeRemotingPlugin");

                // Need refletion method invoke for using 'PlayModeRemotingPluginType' as type in generic method call
                var AllOpenXRSettingsMethods = typeof(OpenXRSettings).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

                // Filter the methods
                var asssss = AllOpenXRSettingsMethods.Where(x => x.Name.Contains("Feature")).ToList();

                // Require 'OpenXRSettings.Instance.GetFeature<>()' method
                var OpenXRSettingsMethod = asssss.FirstOrDefault(x => x.ReturnParameter.ParameterType.Name == "TFeature");

                // Call of the Generic method on OpenXRSettings.Instance with empty parameters
                var pluginVariable = OpenXRSettingsMethod.MakeGenericMethod(PlayModeRemotingPluginType).Invoke(OpenXRSettings.Instance, null);

                // Get 'RemotingSettings' FieldInfo from 'PlayModeRemotingPluginType'
                FieldInfo RemoteSettingsInfo = PlayModeRemotingPluginType.GetField("m_remotingSettings", BindingFlags.Instance | BindingFlags.NonPublic);
                if (RemoteSettingsInfo != null)
                {
                    // Get methodinfo of 'PlayModeRemotingPlugin.GetOrLoadRemotingSettings'
                    var pluginGetRemoteMethod = PlayModeRemotingPluginType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).ToList().FirstOrDefault(x => x.ReturnType == RemoteSettingsInfo.FieldType);

                    // Call 'PlayModeRemotingPlugin.GetOrLoadRemotingSettings' to get 'RemoteSettings' data
                    var remoteSettingsValue = pluginGetRemoteMethod.Invoke(pluginVariable, null);

                    // Get 'RemoteHostName' PropertyInfo from remoteSettingsValue
                    var RemoteHostNamePropertyinfo = RemoteSettingsInfo.FieldType.GetProperty("RemoteHostName", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

                    var DeviceIP = RemoteHostNamePropertyinfo.GetValue(remoteSettingsValue);
                    if (!String.IsNullOrEmpty(DeviceIP.ToString()))
                    {
                        return DeviceIP.ToString();
                    }
                }
            }
            return "";
        }
    }
}
