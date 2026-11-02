namespace DevToolsHub;

/// <summary>Curated topical links between electrical calculators and the /electrical-calculators hub.</summary>
public static class ElectricalLinks
{
    public const string HubHref = "electrical-calculators";

    public static readonly (string Heading, string Intro, string[] Slugs)[] Groups =
    [
        ("Cable sizing, voltage drop & wire gauge (IEC mm² and NEC AWG)",
            "Size cables to IEC 60364 (mm²) or NEC 310.16 (AWG / kcmil), check voltage drop and convert between gauge systems.",
            ["cable-size-calculator", "voltage-drop-calculator", "ampacity-table", "awg-to-mm2", "wire-resistance-calculator", "conduit-fill-calculator", "pcb-trace-width-calculator"]),
        ("Protection, fault current & earthing",
            "Choose breakers and fuses, calculate prospective short-circuit current and design earth electrodes.",
            ["breaker-size-calculator", "short-circuit-calculator", "transformer-calculator", "ground-rod-resistance"]),
        ("Power, motors & energy",
            "Convert kW, kVA and amps, size motors, generators and solar systems, and estimate running costs.",
            ["power-converter", "ohms-law-calculator", "power-factor-correction", "motor-current-calculator", "generator-size-calculator", "solar-calculator", "battery-life-calculator", "electricity-cost-calculator"]),
        ("Component codes & values",
            "Decode resistor, capacitor and inductor markings and find standard E-series values.",
            ["resistor-color-code", "smd-resistor-code", "standard-resistor-values", "capacitor-code-calculator", "inductor-color-code", "bom-calculator"]),
        ("Electronics circuit calculators",
            "Design LED, divider, Zener, timer and filter circuits and convert RF power levels.",
            ["led-resistor-calculator", "voltage-divider-calculator", "series-parallel-calculator", "zener-diode-calculator", "rc-time-constant-calculator", "555-timer-calculator", "dbm-converter"]),
    ];

    private static readonly Dictionary<string, string[]> Related = new()
    {
        ["cable-size-calculator"] = ["voltage-drop-calculator", "ampacity-table", "awg-to-mm2", "breaker-size-calculator", "wire-resistance-calculator", "short-circuit-calculator", "conduit-fill-calculator"],
        ["voltage-drop-calculator"] = ["cable-size-calculator", "ampacity-table", "awg-to-mm2", "wire-resistance-calculator", "breaker-size-calculator", "power-converter"],
        ["ampacity-table"] = ["cable-size-calculator", "voltage-drop-calculator", "awg-to-mm2", "breaker-size-calculator", "conduit-fill-calculator"],
        ["awg-to-mm2"] = ["ampacity-table", "cable-size-calculator", "voltage-drop-calculator", "wire-resistance-calculator", "pcb-trace-width-calculator"],
        ["wire-resistance-calculator"] = ["voltage-drop-calculator", "awg-to-mm2", "cable-size-calculator", "ohms-law-calculator"],
        ["conduit-fill-calculator"] = ["ampacity-table", "cable-size-calculator", "awg-to-mm2"],
        ["breaker-size-calculator"] = ["cable-size-calculator", "ampacity-table", "motor-current-calculator", "short-circuit-calculator", "power-converter"],
        ["short-circuit-calculator"] = ["transformer-calculator", "cable-size-calculator", "breaker-size-calculator", "ground-rod-resistance"],
        ["transformer-calculator"] = ["short-circuit-calculator", "power-converter", "cable-size-calculator", "breaker-size-calculator"],
        ["ground-rod-resistance"] = ["short-circuit-calculator", "breaker-size-calculator", "cable-size-calculator"],
        ["power-converter"] = ["motor-current-calculator", "power-factor-correction", "cable-size-calculator", "generator-size-calculator", "ohms-law-calculator"],
        ["power-factor-correction"] = ["power-converter", "motor-current-calculator", "transformer-calculator"],
        ["motor-current-calculator"] = ["power-converter", "breaker-size-calculator", "cable-size-calculator", "generator-size-calculator", "power-factor-correction"],
        ["generator-size-calculator"] = ["motor-current-calculator", "power-converter", "solar-calculator", "electricity-cost-calculator"],
        ["solar-calculator"] = ["battery-life-calculator", "voltage-drop-calculator", "electricity-cost-calculator", "generator-size-calculator", "cable-size-calculator"],
        ["battery-life-calculator"] = ["solar-calculator", "ohms-law-calculator", "electricity-cost-calculator"],
        ["electricity-cost-calculator"] = ["power-converter", "solar-calculator", "generator-size-calculator"],
        ["ohms-law-calculator"] = ["series-parallel-calculator", "voltage-divider-calculator", "led-resistor-calculator", "power-converter", "wire-resistance-calculator"],
        ["resistor-color-code"] = ["smd-resistor-code", "standard-resistor-values", "capacitor-code-calculator", "inductor-color-code", "series-parallel-calculator"],
        ["smd-resistor-code"] = ["resistor-color-code", "standard-resistor-values", "capacitor-code-calculator"],
        ["standard-resistor-values"] = ["resistor-color-code", "smd-resistor-code", "series-parallel-calculator", "voltage-divider-calculator"],
        ["capacitor-code-calculator"] = ["resistor-color-code", "inductor-color-code", "smd-resistor-code", "rc-time-constant-calculator"],
        ["inductor-color-code"] = ["resistor-color-code", "capacitor-code-calculator", "rc-time-constant-calculator"],
        ["series-parallel-calculator"] = ["standard-resistor-values", "ohms-law-calculator", "resistor-color-code", "voltage-divider-calculator"],
        ["led-resistor-calculator"] = ["resistor-color-code", "standard-resistor-values", "ohms-law-calculator", "zener-diode-calculator"],
        ["voltage-divider-calculator"] = ["standard-resistor-values", "series-parallel-calculator", "zener-diode-calculator", "ohms-law-calculator"],
        ["zener-diode-calculator"] = ["voltage-divider-calculator", "led-resistor-calculator", "standard-resistor-values"],
        ["rc-time-constant-calculator"] = ["capacitor-code-calculator", "555-timer-calculator", "inductor-color-code", "series-parallel-calculator"],
        ["555-timer-calculator"] = ["rc-time-constant-calculator", "capacitor-code-calculator", "standard-resistor-values"],
        ["pcb-trace-width-calculator"] = ["awg-to-mm2", "wire-resistance-calculator", "ampacity-table"],
        ["dbm-converter"] = ["ohms-law-calculator", "power-converter"],
        ["bom-calculator"] = ["resistor-color-code", "capacitor-code-calculator", "standard-resistor-values"],
    };

    public static bool IsElectrical(string href) => Groups.Any(g => g.Slugs.Contains(href));

    public static IEnumerable<ToolInfo> For(string href) =>
        Related.TryGetValue(href, out var slugs)
            ? slugs.Select(ToolCatalog.Find).OfType<ToolInfo>()
            : [];
}
