﻿namespace MauiBluetoothBLE.Models;

public class Uuids
{
    public static Guid HM10Service { get; private set; } = new Guid("00000000-0000-0000-0000-001590919d09"); //"HM10 Service"

    public static Guid TISensorTagSmartKeys { get; private set; } = new Guid("0000ffe0-0000-1000-8000-00805f9b34fb");
    public static Guid TXRX { get; private set; } = new Guid("0000ffe1-0000-1000-8000-00805f9b34fb");
    
}

