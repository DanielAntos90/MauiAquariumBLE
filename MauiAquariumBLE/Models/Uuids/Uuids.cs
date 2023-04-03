﻿namespace MauiBluetoothBLE.Models;

public class Uuids
{
    public static Guid[] HeartRateServiceUuids { get; private set; } = new Guid[] { new Guid("0000180d-0000-1000-8000-00805f9b34fb") }; //"Heart Rate Service"
    public static Guid HeartRateServiceUuid { get; private set; } = new Guid("0000180d-0000-1000-8000-00805f9b34fb"); //"Heart Rate Service"
    public static Guid HeartRateMeasurementCharacteristicUuid { get; private set; } = new Guid("00002a37-0000-1000-8000-00805f9b34fb"); //"Heart Rate Measurement"
    public static Guid BatteryLevelServiceUuid { get; private set; } = new Guid("0000180f-0000-1000-8000-00805f9b34fb"); //"Battery Service"
    public static Guid BatteryLevelCharacteristicUuid { get; private set; } = new Guid("00002a19-0000-1000-8000-00805f9b34fb"); //"Battery Level"

    public static Guid HM10Service { get; private set; } = new Guid("00000000-0000-0000-0000-001590919d09"); //"HM10 Service"

    public static Guid TISensorTagSmartKeys { get; private set; } = new Guid("0000ffe0-0000-1000-8000-00805f9b34fb");
    public static Guid TXRX { get; private set; } = new Guid("0000ffe1-0000-1000-8000-00805f9b34fb");
    
}
