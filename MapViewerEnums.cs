namespace VAGSuite
{
    public enum ViewType : int
    {
        Hexadecimal = 0,
        Decimal,
        Easy,
        ASCII
    }

    public enum ViewSize : int
    {
        NormalView = 0,
        SmallView,
        ExtraSmallView
    }

    public enum XDFCategories : int
    {
        Undocumented = 0,
        Fuel,
        Ignition,
        Boost_control,
        Idle,
        Correction,
        Misc,
        Sensor,
        Runtime,
        Diagnostics
    }

    public enum XDFSubCategory : int
    {
        None = 0,
        Basic,
        Advanced,
        Undocumented,
        Startup,
        Enrichment,
        Idle,
        Axis,
        Basic_manual,
        Basic_automatic,
        Advanced_manual,
        Advanced_automatic,
        Warmup,
        Cranking,
        Temperature_compensation,
        Lambda_sensor,
        Transient,
        Airpump_control,
        Temperature_calculation,
        Limp_home,
        Misfire
    }

    public enum XDFUnits : int
    {
        Seconds,
        Milliseconds,
        Minutes,
        Degrees,
        DegF,
        DegC,
        RPM,
        MPH,
        KMPH,
        DC
    }

    public enum AxisIdent : int
    {
        X_Axis = 0,
        Y_Axis = 1,
        Z_Axis = 2
    }
}
