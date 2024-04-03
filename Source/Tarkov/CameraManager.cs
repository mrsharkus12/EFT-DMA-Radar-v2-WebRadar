﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace eft_dma_radar.Source.Tarkov
{
    public class CameraManager
    {
        private ulong _unityBase;
        private ulong _opticCamera;
        private ulong _fpsCamera;
        public bool IsReady
        {
            get
            {
                return this._opticCamera != 0 && this._fpsCamera != 0;
            }
        }

        private Config _config
        {
            get => Program.Config;
        }

        public CameraManager(ulong unityBase)
        {
            this._unityBase = unityBase;
            this.GetCamera();
        }

        private bool GetCamera()
        {
            try
            {
                var addr = Memory.ReadPtr(this._unityBase + 0x0179F500);
                for (int i = 0; i < 500; i++)
                {
                    var allCameras = Memory.ReadPtr(addr + 0x0);
                    var camera = Memory.ReadPtr(allCameras + (ulong)i * 0x8);

                    if (camera != 0)
                    {
                        var cameraObject = Memory.ReadPtr(camera + Offsets.GameObject.ObjectClass);
                        var cameraNamePtr = Memory.ReadPtr(cameraObject + Offsets.GameObject.ObjectName);

                        var cameraName = Memory
                            .ReadString(cameraNamePtr, 64)
                            .Replace("\0", string.Empty);
                        if (
                            cameraName.Contains(
                                "BaseOpticCamera(Clone)",
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        {
                            this._opticCamera = cameraObject;
                        }
                        if (cameraName.Contains("FPS Camera", StringComparison.OrdinalIgnoreCase))
                        {
                            this._fpsCamera = cameraObject;
                        }
                        if (this._opticCamera != 0 && this._fpsCamera != 0)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (DMAShutdown)
            {
                throw;
            }
            return false;
        }

        public async Task<bool> GetCameraAsync()
        {
            return await Task.Run(() => this.GetCamera());
        }

        public void UpdateCamera()
        {
            if (this._unityBase == 0)
                return;
            this.GetCamera();
        }

        private ulong GetComponentFromGameObject(ulong gameObject, string componentName)
        {
            var component = Memory.ReadPtr(gameObject + 0x30);
            // Loop through a fixed range of potential component slots
            for (int i = 0x8; i < 0x500; i += 0x10)
            {
                var componentPtr = Memory.ReadPtr(component + (ulong)i);
                if (componentPtr == 0)
                    continue;
                var fieldsPtr = Memory.ReadPtr(componentPtr + 0x28);
                componentPtr = Memory.ReadPtr(componentPtr + 0x28);
                var classNamePtr = Memory.ReadPtrChain(fieldsPtr, Offsets.UnityClass.Name);
                var className = Memory.ReadString(classNamePtr, 64).Replace("\0", string.Empty);
                //Console.WriteLine($"GetComponentFromGameObject: {className}");
                if (className.Contains(componentName, StringComparison.OrdinalIgnoreCase))
                {
                    return componentPtr;
                }
            }
            return 0;
        }

        /// <summary>
        /// public function to turn nightvision on and off
        /// </summary>
        public void NightVision(bool on)
        {
            if (this.IsReady)
            {
                var nightVisionComponent = this.GetComponentFromGameObject(this._fpsCamera, "NightVision");
                var nightVisionOn = Memory.ReadValue<bool>(nightVisionComponent + 0xEC);

                if (on != nightVisionOn)
                {
                    Memory.WriteValue(nightVisionComponent + 0xEC, !nightVisionOn);
                }
            }
        }

        /// <summary>
        /// public function to turn visor on and off
        /// </summary>
        public void VisorEffect(bool on)
        {
            if (this.IsReady)
            {
                var visorComponent = this.GetComponentFromGameObject(this._fpsCamera, "VisorEffect");
                bool visorDown = (Memory.ReadValue<float>(visorComponent + 0xC0) == 1.0f);

                if (on == visorDown)
                {
                    Memory.WriteValue(visorComponent + 0xC0, (on ? 0.0f : 1.0f));
                }
            }
        }

        /// <summary>
        /// public function to turn thermalvision on and off
        /// </summary>
        public void ThermalVision(bool on)
        {
            if (this.IsReady)
            {
                var fpsThermalComponent = this.GetComponentFromGameObject(this._fpsCamera, "ThermalVision");
                var thermalOn = Memory.ReadValue<bool>(fpsThermalComponent + 0xE0);

                if (on != thermalOn)
                {
                    Memory.WriteValue(fpsThermalComponent + 0xE0, !thermalOn);
                    Memory.WriteValue(fpsThermalComponent + 0xE1, thermalOn);
                    Memory.WriteValue(fpsThermalComponent + 0xE2, thermalOn);
                    Memory.WriteValue(fpsThermalComponent + 0xE3, thermalOn);
                    Memory.WriteValue(fpsThermalComponent + 0xE4, thermalOn);
                    Memory.WriteValue(fpsThermalComponent + 0xE5, thermalOn);

                    try
                    {
                        var thermalVisionUtilities = Memory.ReadPtr(fpsThermalComponent + 0x18);
                        var valuesCoefs = Memory.ReadPtr(thermalVisionUtilities + 0x18);
                        Memory.WriteValue(valuesCoefs + 0x10, this._config.MainThermalSetting.ColorCoefficient);
                        Memory.WriteValue(valuesCoefs + 0x14, this._config.MainThermalSetting.MinTemperature);
                        Memory.WriteValue(valuesCoefs + 0x18, this._config.MainThermalSetting.RampShift);
                        Memory.WriteValue(thermalVisionUtilities + 0x30, this._config.MainThermalSetting.ColorScheme);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// public function to turn optic thermalvision on and off
        /// </summary>
        public void OpticThermalVision(bool on)
        {
            if (this.IsReady)
            {
                ulong opticComponent = 0;
                var component = Memory.ReadPtr(this._opticCamera + 0x30);
                var opticThermal = this.GetComponentFromGameObject(this._opticCamera, "ThermalVision");
                for (int i = 0x8; i < 0x100; i += 0x10)
                {
                    var fields = Memory.ReadPtr(component + (ulong)i);
                    if (fields == 0)
                        continue;
                    var fieldsPtr_ = Memory.ReadPtr(fields + 0x28);
                    var classNamePtr = Memory.ReadPtrChain(fieldsPtr_, Offsets.UnityClass.Name);
                    var className = Memory.ReadString(classNamePtr, 64).Replace("\0", string.Empty);
                    if (className == "ThermalVision")
                    {
                        opticComponent = fields;
                        break;
                    }
                }

                Memory.WriteValue(opticComponent + 0x38, on);
                Memory.WriteValue(opticThermal + 0xE1, !on);
                Memory.WriteValue(opticThermal + 0xE2, !on);
                Memory.WriteValue(opticThermal + 0xE3, !on);
                Memory.WriteValue(opticThermal + 0xE4, !on);
                Memory.WriteValue(opticThermal + 0xE5, !on);

                try
                {
                    var thermalVisionUtilities = Memory.ReadPtr(opticThermal + 0x18);
                    var valuesCoefs = Memory.ReadPtr(thermalVisionUtilities + 0x18);
                    Memory.WriteValue(valuesCoefs + 0x10, this._config.OpticThermalSetting.ColorCoefficient);
                    Memory.WriteValue(valuesCoefs + 0x14, this._config.OpticThermalSetting.MinTemperature);
                    Memory.WriteValue(valuesCoefs + 0x18, this._config.OpticThermalSetting.RampShift);
                    Memory.WriteValue(thermalVisionUtilities + 0x30, this._config.OpticThermalSetting.ColorScheme);
                }
                catch { }

            }
        }
    }
}